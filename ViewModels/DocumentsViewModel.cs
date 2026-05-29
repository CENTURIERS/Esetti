using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels.Member;

namespace Esseti.ViewModels
{
    public partial class DocumentsViewModel : ViewModelBase
    {
        public override string PageTitle => "Centrum Dokument\u00F3w";

        private readonly IMemberRepository _memberRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        public ObservableCollection<MemberItemViewModel> AvailableMembers { get; } = new();

        [ObservableProperty]
        private MemberItemViewModel? _selectedMember;

        [ObservableProperty]
        private DateTimeOffset? _startDate = DateTimeOffset.Now.AddMonths(-6);

        [ObservableProperty]
        private DateTimeOffset? _endDate = DateTimeOffset.Now;

        public DocumentsViewModel(IMemberRepository memberRepository, IClubRepository clubRepository, IPdfGeneratorService pdfGeneratorService)
        {
            _memberRepository = memberRepository;
            _clubRepository = clubRepository;
            _pdfGeneratorService = pdfGeneratorService;

            _ = LoadMembersAsync();
        }

        private async Task LoadMembersAsync()
        {
            try
            {
                var members = await _memberRepository.GetAllMembersAsync();
                var activeMembers = members.Where(m => m.IsActive).ToList();

                Dispatcher.UIThread.Post(() =>
                {
                    AvailableMembers.Clear();
                    foreach (var m in activeMembers)
                    {
                        var dept = m.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydzia\u0142u";
                        AvailableMembers.Add(new MemberItemViewModel(
                            memberId: m.MemberId,
                            avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                            firstName: m.FirstName ?? string.Empty,
                            lastName: m.LastName ?? string.Empty,
                            role: m.AuthorityRole?.Name ?? "Brak roli",
                            indexNumber: m.IndexNumber ?? string.Empty,
                            email: m.Account?.Email ?? string.Empty,
                            collegeDepartment: dept,
                            major: m.Major ?? string.Empty,
                            joinDate: m.JoinDate.ToString("dd.MM.yyyy"),
                            isActive: m.IsActive,
                            isSystemAddTile: false
                        ));
                    }
                    SelectedMember = AvailableMembers.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"B\u0142\u0105d podczas \u0142adowania cz\u0142onk\u00F3w w centrum dokument\u00F3w: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SelectAllMembers()
        {
            foreach (var m in AvailableMembers)
            {
                m.IsSelected = true;
            }
        }

        [RelayCommand]
        private void DeselectAllMembers()
        {
            foreach (var m in AvailableMembers)
            {
                m.IsSelected = false;
            }
        }

        [RelayCommand]
        private async Task GenerateActivityCertificateAsync()
        {
            var selectedMembers = AvailableMembers.Where(m => m.IsSelected).ToList();
            if (selectedMembers.Count == 0)
            {
                if (SelectedMember != null)
                {
                    selectedMembers.Add(SelectedMember);
                }
                else
                {
                    return;
                }
            }

            var club = await _clubRepository.GetClubInfoAsync();
            var clubName = club?.Name ?? "Ko\u0142o naukowe";

            var now = DateTime.Now;
            string academicYear = string.Empty;
            if (now.Month >= 10 && now.Month <= 12)
            {
                academicYear = $"{now.Year}/{now.Year + 1}";
            }
            else
            {
                academicYear = $"{now.Year - 1}/{now.Year}";
            }

            var fromDateStr = StartDate?.ToString("MM.yyyy") ?? "10.2024";
            var toDateStr = EndDate?.ToString("MM.yyyy") ?? "06.2025";

            byte[] logoBytes = Array.Empty<byte>();
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UR", "urSygnetPdf.jpg");
            if (!File.Exists(logoPath))
            {
                logoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "UR", "urSygnetPdf.jpg"));
            }

            if (File.Exists(logoPath))
            {
                try
                {
                    logoBytes = File.ReadAllBytes(logoPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to read logo bytes: {ex.Message}");
                }
            }

            if (logoBytes.Length == 0)
            {
                try
                {
                    using (var stream = Avalonia.Platform.AssetLoader.Open(new Uri("avares://Essetti/Assets/UR/urSygnetPdf.jpg")))
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        logoBytes = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load logo via AssetLoader: {ex.Message}");
                }
            }

            var membersData = new List<(Models.Users.Member Member, List<string> Achievements, string Dept)>();

            foreach (var sm in selectedMembers)
            {
                var fullMember = await _memberRepository.GetMemberByIdAsync(sm.MemberId);
                if (fullMember == null) continue;

                var dept = fullMember.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydzia\u0142u";

                var achievements = new List<string>();
                int idx = 1;

                if (fullMember.Projects != null && fullMember.Projects.Count > 0)
                {
                    foreach (var p in fullMember.Projects)
                    {
                        var pFrom = p.DateStart?.ToString("MM.yyyy") ?? fromDateStr;
                        var pTo = p.DateEnd?.ToString("MM.yyyy") ?? toDateStr;
                        var desc = !string.IsNullOrEmpty(p.Description) ? p.Description : (!string.IsNullOrEmpty(p.AdditionalInformation) ? p.AdditionalInformation : "Zadaniem studenta by\u0142 czynny udzia\u0142 w projekcie.");
                        achievements.Add($"{idx++}. Udzia\u0142 w projekcie: \u201E{p.Name}\u201D w okresie {pFrom} r. \u2013 {pTo} r. {desc}");
                    }
                }

                if (fullMember.Activities != null && fullMember.Activities.Count > 0)
                {
                    foreach (var a in fullMember.Activities)
                    {
                        var aDate = a.Date.ToString("dd.MM.yyyy");
                        var info = !string.IsNullOrEmpty(a.AdditionalInformation) ? a.AdditionalInformation : "Zaanga\u017Cowanie w organizacj\u0119 wydarzenia.";
                        achievements.Add($"{idx++}. Udzia\u0142 w aktywno\u015Bci: \u201E{a.Name}\u201D dnia {aDate} r. {info}");
                    }
                }

                if (achievements.Count == 0)
                {
                    achievements.Add("1. Czynny udzia\u0142 w spotkaniach ko\u0142a naukowego oraz zaanga\u017Cowanie w bie\u017C\u0105c\u0105 dzia\u0142alno\u015B\u0107 statutow\u0105.");
                }

                membersData.Add((fullMember, achievements, dept));
            }

            if (membersData.Count == 0) return;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string tempPdfPath = Path.Combine(desktopPath, "Zaswiadczenia_Aktywnosc.pdf");

            try
            {
                await _pdfGeneratorService.GenerateActivityCertificateAsync(
                    tempPdfPath,
                    logoBytes,
                    clubName,
                    academicYear,
                    fromDateStr,
                    toDateStr,
                    membersData);

                Process.Start(new ProcessStartInfo(tempPdfPath)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText("pdf_error_certificate.txt", ex.ToString());
                }
                catch { }
                Debug.WriteLine($"Nie mo\u017Cna wygenerowa\u0107 lub otworzy\u0107 za\u015Bwiadczenia: {ex}");
            }
        }

        [RelayCommand]
        private async Task GenerateMembersListAsync()
        {
            var members = (await _memberRepository.GetAllMembersAsync()).Where(m => m.IsActive).ToList();
            var club = await _clubRepository.GetClubInfoAsync();

            var clubName = club?.Name ?? "Ko\u0142o naukowe";
            var supervisor = club?.SupervisorName ?? "Brak opiekuna";
            var supervisorEmail = club?.SupervisorEmail ?? "---";
            var supervisorPhone = club?.SupervisorPhone ?? "---";

            var zarzadRoles = new[] { "Prezes", "Wiceprezes", "Skarbnik", "Sekretarz", "Zarz\u0105d" };
            var zarzadList = members.Where(m => zarzadRoles.Contains(m.AuthorityRole?.Name ?? ""))
                                    .OrderBy(m => m.LastName)
                                    .ToList();
            
            var zwykliCzlonkowie = members.OrderBy(m => m.LastName).ToList();

            byte[] logoBytes = Array.Empty<byte>();
            
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UR", "urSygnetPdf.jpg");
            if (!File.Exists(logoPath))
            {
                logoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "UR", "urSygnetPdf.jpg"));
            }

            if (File.Exists(logoPath))
            {
                try
                {
                    logoBytes = File.ReadAllBytes(logoPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to read logo bytes: {ex.Message}");
                }
            }

            if (logoBytes.Length == 0)
            {
                try
                {
                    using (var stream = Avalonia.Platform.AssetLoader.Open(new Uri("avares://Essetti/Assets/UR/urSygnetPdf.jpg")))
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        logoBytes = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load logo via AssetLoader: {ex.Message}");
                }
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string tempPdfPath = Path.Combine(desktopPath, "Lista_UR_SKN.pdf");

            try
            {
                await _pdfGeneratorService.GenerateMembersListAsync(
                    tempPdfPath,
                    logoBytes,
                    clubName,
                    supervisor,
                    supervisorEmail,
                    supervisorPhone,
                    zarzadList,
                    zwykliCzlonkowie);

                Process.Start(new ProcessStartInfo(tempPdfPath)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText("pdf_error.txt", ex.ToString());
                }
                catch { }
                Debug.WriteLine($"Nie mo\u017Cna wygenerowa\u0107 lub otworzy\u0107 pliku listy: {ex}");
            }
        }
    }
}