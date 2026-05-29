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
using Esseti.ViewModels.Member;
using QuestPDF.Fluent;

namespace Esseti.ViewModels
{
    public partial class DocumentsViewModel : ViewModelBase
    {
        public override string PageTitle => "Centrum Dokumentów";

        private readonly IMemberRepository _memberRepository;
        private readonly IClubRepository _clubRepository;

        public ObservableCollection<MemberItemViewModel> AvailableMembers { get; } = new();

        [ObservableProperty]
        private MemberItemViewModel? _selectedMember;

        [ObservableProperty]
        private DateTimeOffset? _startDate = DateTimeOffset.Now.AddMonths(-6);

        [ObservableProperty]
        private DateTimeOffset? _endDate = DateTimeOffset.Now;

        public DocumentsViewModel(IMemberRepository memberRepository, IClubRepository clubRepository)
        {
            _memberRepository = memberRepository;
            _clubRepository = clubRepository;

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
                        var dept = m.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";
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
                System.Diagnostics.Debug.WriteLine($"Błąd podczas ładowania członków w centrum dokumentów: {ex.Message}");
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
                QuestPDF.Fluent.Document.Create(container =>
                {
                    foreach (var data in membersData)
                    {
                        var m = data.Member;
                        var achievements = data.Achievements;
                        var dept = data.Dept;

                        container.Page(page =>
                        {
                            page.Size(QuestPDF.Helpers.PageSizes.A4);
                            page.MarginVertical(36);
                            page.MarginHorizontal(72);
                            page.DefaultTextStyle(x => x.FontFamily("Corbel").FontSize(10));

                            page.Content().PaddingVertical(15).Column(col =>
                            {
                                
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().AlignRight().Column(c =>
                                    {
                                        c.Item().Text("Załącznik nr 1 do Statutu").Italic().FontSize(8.5f).FontColor("#475569");
                                        c.Item().Text("Studenckiego Towarzystwa Naukowego UR").Italic().FontSize(8.5f).FontColor("#475569");
                                    });
                                });

                                
                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Uniwersytet Rzeszowski").FontColor("#0033A0").FontSize(11);
                                        c.Item().Text("Studenckie Towarzystwo Naukowe").Bold().FontColor("#0033A0").FontSize(11);
                                    });

                                    if (logoBytes.Length > 0)
                                    {
                                        row.ConstantItem(60).AlignRight().Image(logoBytes);
                                    }
                                });

                                
                                col.Item().PaddingTop(15).Row(row =>
                                {
                                    row.RelativeItem().Column(studentData =>
                                    {
                                        studentData.Item().Text($"{m.FirstName} {m.LastName}").FontSize(11);
                                        studentData.Item().PaddingBottom(5).Text("Imię i Nazwisko studenta").FontSize(7.5f).FontColor("#64748b");
                                    });

                                    row.RelativeItem().AlignRight().AlignBottom().Text("Rzeszów, ........................").Bold().FontSize(10);
                                });

                                
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(studentData =>
                                    {
                                        studentData.Item().Text($"{m.IndexNumber}").FontSize(11);
                                        studentData.Item().PaddingBottom(5).Text("Numer albumu").FontSize(7.5f).FontColor("#64748b");

                                        studentData.Item().Text($"{m.Major}").FontSize(11);
                                        studentData.Item().Text("Kierunek studiów").FontSize(7.5f).FontColor("#64748b");
                                    });
                                });

                                col.Item().PaddingTop(30).AlignCenter().Text(t =>
                                {
                                    t.Line("ZAŚWIADCZENIE O DZIAŁALNOŚCI W STUDENCKIM KOLE NAUKOWYM").Bold().FontSize(12);
                                    t.Line($"W ROKU AKADEMICKIM {academicYear}").Bold().FontSize(12);
                                });

                                col.Item().PaddingTop(25).Text(t =>
                                {
                                    t.Span("Potwierdzam mój udział w pracach Studenckiego Koła Naukowego ").FontSize(10.5f);
                                    t.Span(clubName).Bold().FontSize(10.5f);
                                    t.Span(" działającego przy ").FontSize(10.5f);
                                    t.Span(dept).Bold().FontSize(10.5f);
                                    t.Span($", w okresie od {fromDateStr} do {toDateStr}").FontSize(10.5f);
                                });

                                col.Item().PaddingTop(20).Text("Wykaz szczególnych osiągnięć studenta (pełnione funkcje/udział w projektach/ prace badawcze/ publikacje/ inne)").Bold().FontSize(10);

                                col.Item().PaddingTop(10).Column(list =>
                                {
                                    foreach (var ach in achievements)
                                    {
                                        list.Item().PaddingBottom(6).Text(ach).FontSize(9.5f).Justify();
                                    }
                                });

                                col.Item().ShowEntire().PaddingTop(40).Column(signatures =>
                                {
                                    signatures.Item().PaddingBottom(15).Row(r =>
                                    {
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                            c.Item().AlignCenter().Text("akceptacja merytoryczna Opiekuna Koła Naukowego").FontSize(7.5f).FontColor("#64748b");
                                        });
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                            c.Item().AlignCenter().Text("podpis studenta składającego zaświadczenie").FontSize(7.5f).FontColor("#64748b");
                                        });
                                    });

                                    signatures.Item().PaddingVertical(10).BorderBottom(0.5f).BorderColor("#cbd5e1");

                                    signatures.Item().PaddingTop(15).Row(r =>
                                    {
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                            c.Item().AlignCenter().Text("podpis Pełnomocnika Rektora ds. SKN").FontSize(7.5f).FontColor("#64748b");
                                        });
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                            c.Item().AlignCenter().Text("podpis reprezentanta Zarządu STN").FontSize(7.5f).FontColor("#64748b");
                                        });
                                    });
                                });
                            });
                        });
                    }
                })
                .GeneratePdf(tempPdfPath);

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
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(36);
                        page.DefaultTextStyle(x => x.FontFamily("Corbel").FontSize(10));

                        page.Content().PaddingVertical(15).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Rektor").Bold().FontColor("#0033A0").FontSize(11);
                                    c.Item().Text("Uniwersytetu Rzeszowskiego").FontColor("#0033A0").FontSize(11);
                                });
                                if (logoBytes.Length > 0)
                                {
                                    
                                    row.ConstantItem(60).Image(logoBytes);
                                }
                            });

                            col.Item().PaddingTop(10).AlignRight().Text(t =>
                            {
                                t.Line("Załącznik nr 6").Italic().FontSize(9).FontColor("#475569");
                                t.Line("do Regulaminu działalności SKN z dnia 25.11.2024 r.").Italic().FontSize(9).FontColor("#475569");
                            });

                            col.Item().PaddingTop(15).AlignCenter().Text("LISTA CZŁONKÓW STUDENCKIEGO KOŁA NAUKOWEGO").Bold().FontSize(13);

                            col.Item().PaddingTop(12).AlignCenter().Text(t =>
                            {
                                t.Line(clubName).Bold().FontSize(11);
                                t.Line("...............................................................................").Bold();
                                t.Line("Nazwa Studenckiego Koła Naukowego").FontSize(8).FontColor("#64748b");
                            });

                            col.Item().PaddingVertical(10).AlignCenter().Width(240).Border(0.5f).Padding(8).Text(t =>
                            {
                                t.Span("stan na dzień ").Bold();
                                t.Span(DateTime.Now.ToString("dd.MM.yyyy"));
                            });

                            col.Item().PaddingTop(10).Text("SKŁAD ZARZĄDU STUDENCKIEGO KOŁA NAUKOWEGO:").Bold().FontSize(11);

                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Lp.").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Imię i nazwisko").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Pełniona funkcja").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Nr telefonu").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Adres e-mail").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Podpis").Bold().FontSize(8);
                                });

                                int lpZarzad = 1;
                                foreach (var m in zarzadList)
                                {
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).AlignCenter().Text($"{lpZarzad++}.");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text($"{m.FirstName} {m.LastName}");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text(m.AuthorityRole?.Name ?? "");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text(m.PhoneNumber ?? "---");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text(m.Account?.Email ?? "---");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text("");
                                }
                            });

                            col.Item().PaddingTop(15).Text("OPIEKUN NAUKOWY STUDENCKIEGO KOŁA NAUKOWEGO:").Bold().FontSize(11);
                            col.Item().PaddingTop(5).Border(0.5f).Column(box =>
                            {
                                box.Item().Background("#F1F5F9").BorderBottom(0.5f).BorderColor("#000000").Padding(5).Text("Imię i nazwisko, stopień naukowy/tytuł naukowy, nr telefonu, e-mail").FontSize(8).Bold();
                                box.Item().Padding(8).Text($"{supervisor}, tel: {supervisorPhone}, e-mail: {supervisorEmail}").FontSize(10);
                            });

                            col.Item().PaddingTop(15).Text("LISTA CZŁONKÓW SKN:").Bold().FontSize(11);
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Lp.").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Imię i nazwisko").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Nr telefonu").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Adres e-mail").Bold().FontSize(8);
                                    header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Podpis").Bold().FontSize(8);
                                });

                                int lpCzlonkowie = 1;
                                foreach (var m in zwykliCzlonkowie)
                                {
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).AlignCenter().Text($"{lpCzlonkowie++}.");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text($"{m.FirstName} {m.LastName}");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text(m.PhoneNumber ?? "---");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text(m.Account?.Email ?? "---");
                                    table.Cell().Border(0.5f).PaddingVertical(6).PaddingHorizontal(4).Text("");
                                }
                            });

                            
                            col.Item().PaddingTop(20).Border(0.5f).Column(box =>
                            {
                                box.Item().Background("#E6EEFF").BorderBottom(0.5f).BorderColor("#000000").Padding(5).Text("PODPISY").FontSize(8).Bold();
                                box.Item().PaddingVertical(15).Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                        c.Item().AlignCenter().Text("Czytelny podpis Prezesa SKN").FontSize(8).FontColor("#64748b");
                                    });
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                        c.Item().AlignCenter().Text("Opiekun SKN i pieczęć jednostki zatrudniającej").FontSize(8).FontColor("#64748b");
                                    });
                                });
                            });

                            
                            col.Item().PaddingTop(25).Text("KLAUZULA INFORMACYJNA").Bold().Underline().FontSize(8);
                            col.Item().PaddingTop(4).Text(
                                "Zgodnie z art. 13 Rozporządzenia Parlamentu Europejskiego i Rady (UE) 2016/679 z dnia 27 kwietnia 2016 r. w sprawie ochrony osób fizycznych w związku z przetwarzaniem danych osobowych i w sprawie swobodnego przepływu takich danych oraz uchylenia dyrektywy 95/46/WE (Dz. Urz. UE L 119 z 04.05.2016) informujemy, iż:\n" +
                                "1) Administratorem Pani/Pana danych osobowych jest Uniwersytet Rzeszowski, al. Rejtana 16 C, 35-959 Rzeszów, reprezentowany przez Rektora.\n" +
                                "2) Dane kontaktowe Inspektora Ochrony Danych w Uniwersytecie Rzeszowskim adres email: antochow@ur.edu.pl, +48 17 872 34 39, +48 17 872 36 46.\n" +
                                "3) Pani/Pana dane osobowe przetwarzane będą w celach prowadzenia działalności organizacji studenckich/kół naukowych na podstawie ustawy z dnia 20 lipca 2018 r. – Prawo o szkolnictwie wyższym i nauce (Dz. U. z 2018 r. poz. 1668).\n" +
                                "4) Pani/Pana dane osobowe przetwarzane będą na podstawie art. 6 ust. 1 lit. b ww. Rozporządzenia, (przetwarzanie jest niezbędne do wykonania umowy, której stroną jest osoba, której dane dotyczą, lub do podjęcia działań na żądanie osoby, której dane dotyczą, przed zawarciem umowy).\n" +
                                "5) Podanie danych jest dobrowolne, jednak niezbędne do realizacji celu, do jakiego zostały zebrane.\n" +
                                "6) Pani/Pani dane osobowe przechowywane będą przez okres niezbędny do realizacji ww. celu z uwzględnieniem okresów przechowywania określonych w przepisach odrębnych, w tym przepisów archiwalnych.\n" +
                                "7) Odbiorcami Pani/Pana danych będą podmioty, które na podstawie zawartych umów przetwarzają dane osobowe w imieniu Administratora.\n" +
                                "8) Posiada Pani/Pan prawo do: żądania dostępu do treści swoich danych osobowych, do ich sprostowania, usunięcia lub ograniczenia przetwarzania, prawo do wniesienia sprzeciwu wobec przetwarzania, a także prawo do przenoszenia danych.\n" +
                                "9) Posiada Pani/Pan prawo wniesienia skargi do organu nadzorczego, gdy uzasadnione jest, że Pani/Pana dane osobowe przetwarzane są przez Administratora Danych niezgodnie z ww. Rozporządzeniem."
                            ).FontSize(7.5f).FontColor("#475569").Justify();
                        });

                        page.Footer().Column(footer =>
                        {
                            footer.Item().BorderTop(0.5f).BorderColor("#cbd5e1").PaddingTop(4).Row(row =>
                            {
                                row.RelativeItem().Text(t =>
                                {
                                    t.Line("al. T. Rejtana 16c, 35-959 Rzeszów").FontSize(7.5f).FontColor("#64748b");
                                    t.Line("tel. +48 17 872 10 10, faks +48 17 872 12 65").FontSize(7.5f).FontColor("#64748b");
                                    t.Line("rektorur@ur.edu.pl").FontSize(7.5f).FontColor("#64748b");
                                });
                            });
                        });
                    });
                })
                .GeneratePdf(tempPdfPath);

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