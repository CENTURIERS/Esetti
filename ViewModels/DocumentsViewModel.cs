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
    /// <summary>
    /// Model widoku obsługujący "Centrum Dokumentów".
    /// Umożliwia generowanie oficjalnych pism PDF, takich jak zaświadczenia o aktywności dla członków koła oraz kompletnej listy członków z podziałem na zarząd.
    /// </summary>
    public partial class DocumentsViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł wyświetlany na górze strony Centrum Dokumentów.
        /// </summary>
        public override string PageTitle => "Centrum Dokumentów";

        /// <summary>
        /// Repozytorium członków do odpytywania bazy o studentów.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Repozytorium klubu do wyciągania informacji o kole naukowym (np. nazwa, opiekun).
        /// </summary>
        private readonly IClubRepository _clubRepository;

        /// <summary>
        /// Serwis odpowiedzialny za faktyczne wypluwanie plików PDF.
        /// </summary>
        private readonly IPdfGeneratorService _pdfGeneratorService;

        /// <summary>
        /// Lista wszystkich aktywnych członków dostępnych do wyboru w celu wygenerowania dokumentu.
        /// </summary>
        public ObservableCollection<MemberItemViewModel> AvailableMembers { get; } = new();

        /// <summary>
        /// Wybrany członek z listy rozwijanej (ComboBoxa).
        /// </summary>
        [ObservableProperty]
        private MemberItemViewModel? _selectedMember;

        /// <summary>
        /// Data początkowa zakresu generowania zaświadczenia (domyślnie 6 miesięcy wstecz).
        /// </summary>
        [ObservableProperty]
        private DateTimeOffset? _startDate = DateTimeOffset.Now.AddMonths(-6);

        /// <summary>
        /// Data końcowa zakresu generowania zaświadczenia (domyślnie dzisiaj).
        /// </summary>
        [ObservableProperty]
        private DateTimeOffset? _endDate = DateTimeOffset.Now;

        /// <summary>
        /// Konstruktor modelu widoku Centrum Dokumentów. Wstrzykuje serwisy i repozytoria oraz pobiera listę aktywnych członków.
        /// </summary>
        /// <param name="memberRepository">Repozytorium członków koła.</param>
        /// <param name="clubRepository">Repozytorium klubu.</param>
        /// <param name="pdfGeneratorService">Serwis generowania PDF-ów.</param>
        public DocumentsViewModel(IMemberRepository memberRepository, IClubRepository clubRepository, IPdfGeneratorService pdfGeneratorService)
        {
            _memberRepository = memberRepository;
            _clubRepository = clubRepository;
            _pdfGeneratorService = pdfGeneratorService;

            _ = LoadMembersAsync();
        }

        /// <summary>
        /// Pobiera z bazy wszystkich aktywnych członków koła i tworzy dla nich modele widoku kafelków/linii.
        /// </summary>
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
                        var dept = m.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";
                        AvailableMembers.Add(new MemberItemViewModel(
                            memberId: m.MemberId,
                            avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                            firstName: m.FirstName ?? string.Empty,
                            lastName: m.LastName ?? string.Empty,
                            role: m.AuthorityRole?.Name ?? "Brak roli",
                            indexNumber: m.IndexNumber ?? string.Empty,
                            email: m.Account?.Email ?? string.Empty,
                            phoneNumber: m.PhoneNumber ?? string.Empty,
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
                System.Diagnostics.Debug.WriteLine($"Błąd podczas ładowania członków w centrum dokumentów: {ex.Message}");
            }
        }

        /// <summary>
        /// Komenda zaznaczająca wszystkich dostępnych członków na liście (bindowane np. pod przycisk "Zaznacz wszystko").
        /// </summary>
        [RelayCommand]
        private void SelectAllMembers()
        {
            foreach (var m in AvailableMembers)
            {
                m.IsSelected = true;
            }
        }

        /// <summary>
        /// Komenda odznaczająca wszystkich członków na liście.
        /// </summary>
        [RelayCommand]
        private void DeselectAllMembers()
        {
            foreach (var m in AvailableMembers)
            {
                m.IsSelected = false;
            }
        }

        /// <summary>
        /// Komenda asynchroniczna generująca zaświadczenie o aktywności naukowej (projekty, zadania) w wybranym przedziale czasowym.
        /// Zapisuje plik PDF na pulpicie i próbuje go otworzyć.
        /// </summary>
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
            var clubName = club?.Name ?? "Koło naukowe";

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
                    using (var stream = Avalonia.Platform.AssetLoader.Open(new Uri("avares://Esseti/Assets/UR/urSygnetPdf.jpg")))
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

                var dept = fullMember.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";

                var achievements = new List<string>();
                int idx = 1;

                if (fullMember.Projects != null && fullMember.Projects.Count > 0)
                {
                    foreach (var p in fullMember.Projects)
                    {
                        var pFrom = p.DateStart?.ToString("MM.yyyy") ?? fromDateStr;
                        var pTo = p.DateEnd?.ToString("MM.yyyy") ?? toDateStr;
                        var desc = !string.IsNullOrEmpty(p.Description) ? p.Description : (!string.IsNullOrEmpty(p.AdditionalInformation) ? p.AdditionalInformation : "Zadaniem studenta był czynny udział w projekcie.");
                        achievements.Add($"{idx++}. Udział w projekcie: „{p.Name}” w okresie {pFrom} r. – {pTo} r. {desc}");
                    }
                }

                if (fullMember.Activities != null && fullMember.Activities.Count > 0)
                {
                    foreach (var a in fullMember.Activities)
                    {
                        var aDate = a.Date.ToString("dd.MM.yyyy");
                        var info = !string.IsNullOrEmpty(a.AdditionalInformation) ? a.AdditionalInformation : "Zaangażowanie w organizację wydarzenia.";
                        achievements.Add($"{idx++}. Udział w aktywności: „{a.Name}” dnia {aDate} r. {info}");
                    }
                }

                if (achievements.Count == 0)
                {
                    achievements.Add("1. Czynny udział w spotkaniach koła naukowego oraz zaangażowanie w bieżącą działalność statutową.");
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
                Debug.WriteLine($"Nie można wygenerować lub otworzyć zaświadczenia: {ex}");
            }
        }

        /// <summary>
        /// Komenda asynchroniczna generująca oficjalną listę członków koła w formacie PDF (z podziałem na zarząd i zwykłych członków).
        /// Plik ląduje na pulpicie i uruchamia się automatycznie.
        /// </summary>
        [RelayCommand]
        private async Task GenerateMembersListAsync()
        {
            var members = (await _memberRepository.GetAllMembersAsync()).Where(m => m.IsActive).ToList();
            var club = await _clubRepository.GetClubInfoAsync();

            var clubName = club?.Name ?? "Koło naukowe";
            var supervisor = club?.SupervisorName ?? "Brak opiekuna";
            var supervisorEmail = club?.SupervisorEmail ?? "---";
            var supervisorPhone = club?.SupervisorPhone ?? "---";

            var zarzadRoles = new[] { "Prezes", "Wiceprezes", "Skarbnik", "Sekretarz", "Zarząd" };
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
                    using (var stream = Avalonia.Platform.AssetLoader.Open(new Uri("avares://Esseti/Assets/UR/urSygnetPdf.jpg")))
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
                Debug.WriteLine($"Nie można wygenerować lub otworzyć pliku listy: {ex}");
            }
        }
    }
}