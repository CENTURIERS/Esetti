using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Esseti.Services
{
    /// <summary>
    /// Klasa implementująca serwis generowania plików PDF przy użyciu QuestPDF.
    /// </summary>
    public class PdfGeneratorService : IPdfGeneratorService
    {
        /// <inheritdoc />
        public Task GenerateActivityCertificateAsync(
            string outputPath,
            byte[] logoBytes,
            string clubName,
            string academicYear,
            string fromDateStr,
            string toDateStr,
            List<(Models.Users.Member Member, List<string> Achievements, string Dept)> membersData)
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
                        page.Size(PageSizes.A4);
                        page.MarginVertical(36);
                        page.MarginHorizontal(72);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                        page.Content().PaddingVertical(15).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Za\u0142\u0105cznik nr 1 do Statutu").Italic().FontSize(8.5f).FontColor("#475569");
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

                                if (logoBytes != null && logoBytes.Length > 0)
                                {
                                    row.ConstantItem(60).AlignRight().Image(logoBytes);
                                }
                            });

                            col.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Column(studentData =>
                                {
                                    studentData.Item().Text($"{m.FirstName} {m.LastName}").FontSize(11);
                                    studentData.Item().PaddingBottom(5).Text("Imi\u0119 i Nazwisko studenta").FontSize(7.5f).FontColor("#64748b");
                                });

                                row.RelativeItem().AlignRight().AlignBottom().Text("Rzesz\u00F3w, ........................").Bold().FontSize(10);
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(studentData =>
                                {
                                    studentData.Item().Text($"{m.IndexNumber}").FontSize(11);
                                    studentData.Item().PaddingBottom(5).Text("Numer albumu").FontSize(7.5f).FontColor("#64748b");

                                    studentData.Item().Text($"{m.Major}").FontSize(11);
                                    studentData.Item().Text("Kierunek studi\u00F3w").FontSize(7.5f).FontColor("#64748b");
                                });
                            });

                            col.Item().PaddingTop(30).AlignCenter().Text(t =>
                            {
                                t.Line("ZA\u015AWIADCZENIE O DZIA\u0141ALNO\u015BCI W STUDENCKIM KOLE NAUKOWYM").Bold().FontSize(12);
                                t.Line($"W ROKU AKADEMICKIM {academicYear}").Bold().FontSize(12);
                            });

                            col.Item().PaddingTop(25).Text(t =>
                            {
                                t.Span("Potwierdzam m\u00F3j udzia\u0142 w pracach Studenckiego Ko\u0142a Naukowego ").FontSize(10.5f);
                                t.Span(clubName).Bold().FontSize(10.5f);
                                t.Span(" dzia\u0142aj\u0105cego przy ").FontSize(10.5f);
                                t.Span(dept).Bold().FontSize(10.5f);
                                t.Span($", w okresie od {fromDateStr} do {toDateStr}").FontSize(10.5f);
                            });

                            col.Item().PaddingTop(20).Text("Wykaz szczeg\u00F3lnych osi\u0105gni\u0119\u0107 studenta (pe\u0142nione funkcje/udzia\u0142 w projektach/ prace badawcze/ publikacje/ inne)").Bold().FontSize(10);

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
                                        c.Item().AlignCenter().Text("akceptacja merytoryczna Opiekuna Ko\u0142a Naukowego").FontSize(7.5f).FontColor("#64748b");
                                    });
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                        c.Item().AlignCenter().Text("podpis studenta sk\u0142adaj\u0105cego za\u015Bwiadczenie").FontSize(7.5f).FontColor("#64748b");
                                    });
                                });

                                signatures.Item().PaddingVertical(10).BorderBottom(0.5f).BorderColor("#cbd5e1");

                                signatures.Item().PaddingTop(15).Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                        c.Item().AlignCenter().Text("podpis Pe\u0142nomocnika Rektora ds. SKN").FontSize(7.5f).FontColor("#64748b");
                                    });
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().AlignCenter().Text("......................................................").FontSize(9);
                                        c.Item().AlignCenter().Text("podpis reprezentanta Zarz\u0105du STN").FontSize(7.5f).FontColor("#64748b");
                                    });
                                });
                            });
                        });
                    });
                }
            })
            .GeneratePdf(outputPath);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task GenerateMembersListAsync(
            string outputPath,
            byte[] logoBytes,
            string clubName,
            string supervisor,
            string supervisorEmail,
            string supervisorPhone,
            List<Models.Users.Member> zarzadList,
            List<Models.Users.Member> zwykliCzlonkowie)
        {
            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Rektor").Bold().FontColor("#0033A0").FontSize(11);
                                c.Item().Text("Uniwersytetu Rzeszowskiego").FontColor("#0033A0").FontSize(11);
                            });
                            if (logoBytes != null && logoBytes.Length > 0)
                            {
                                row.ConstantItem(60).Image(logoBytes);
                            }
                        });

                        col.Item().PaddingTop(10).AlignRight().Text(t =>
                        {
                            t.Line("Za\u0142\u0105cznik nr 6").Italic().FontSize(9).FontColor("#475569");
                            t.Line("do Regulaminu dzia\u0142alno\u015Bci SKN z dnia 25.11.2024 r.").Italic().FontSize(9).FontColor("#475569");
                        });

                        col.Item().PaddingTop(15).AlignCenter().Text("LISTA CZ\u0141ONK\u00D3W STUDENCKIEGO KO\u0141A NAUKOWEGO").Bold().FontSize(13);

                        col.Item().PaddingTop(12).AlignCenter().Text(t =>
                        {
                            t.Line(clubName).Bold().FontSize(11);
                            t.Line("...............................................................................").Bold();
                            t.Line("Nazwa Studenckiego Ko\u0142a Naukowego").FontSize(8).FontColor("#64748b");
                        });

                        col.Item().PaddingVertical(10).AlignCenter().Width(240).Border(0.5f).Padding(8).Text(t =>
                        {
                            t.Span("stan na dzie\u0144 ").Bold();
                            t.Span(DateTime.Now.ToString("dd.MM.yyyy"));
                        });

                        col.Item().PaddingTop(10).Text("SK\u0141AD ZARZ\u0104DU STUDENCKIEGO KO\u0141A NAUKOWEGO:").Bold().FontSize(11);

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
                                header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Imi\u0119 i nazwisko").Bold().FontSize(8);
                                header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Pe\u0142niona funkcja").Bold().FontSize(8);
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

                        col.Item().PaddingTop(15).Text("OPIEKUN NAUKOWY STUDENCKIEGO KO\u0141A NAUKOWEGO:").Bold().FontSize(11);
                        col.Item().PaddingTop(5).Border(0.5f).Column(box =>
                        {
                            box.Item().Background("#F1F5F9").BorderBottom(0.5f).BorderColor("#000000").Padding(5).Text("Imi\u0119 i nazwisko, stopie\u0144 naukowy/tytu\u0142 naukowy, nr telefonu, e-mail").FontSize(8).Bold();
                            box.Item().Padding(8).Text($"{supervisor}, tel: {supervisorPhone}, e-mail: {supervisorEmail}").FontSize(10);
                        });

                        col.Item().PaddingTop(15).Text("LISTA CZ\u0141ONK\u00D3W SKN:").Bold().FontSize(11);
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
                                header.Cell().Border(0.5f).Background("#E6EEFF").AlignCenter().AlignMiddle().PaddingVertical(6).PaddingHorizontal(4).Text("Imi\u0119 i nazwisko").Bold().FontSize(8);
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
                                    c.Item().AlignCenter().Text("Opiekun SKN i piecz\u0119\u0107 jednostki zatrudniaj\u0105cej").FontSize(8).FontColor("#64748b");
                                });
                            });
                        });

                        col.Item().PaddingTop(25).Text("KLAUZULA INFORMACYJNA").Bold().Underline().FontSize(8);
                        col.Item().PaddingTop(4).Text(
                            "Zgodnie z art. 13 Rozporz\u0105dzenia Parlamentu Europejskiego i Rady (UE) 2016/679 z dnia 27 kwietnia 2016 r. w sprawie ochrony os\u00F3b fizycznych w zwi\u0105zku z przetwarzaniem danych osobowych i w sprawie swobodnego przep\u0142ywu takich danych oraz uchylenia dyrektywy 95/46/WE (Dz. Urz. UE L 119 z 04.05.2016) informujemy, i\u017C:\n" +
                            "1) Administratorem Pani/Pana danych osobowych jest Uniwersytet Rzeszowski, al. Rejtana 16 C, 35-959 Rzesz\u00F3w, reprezentowany przez Rektora.\n" +
                            "2) Dane kontaktowe Inspektora Ochrony Danych w Uniwersytecie Rzeszowskim adres email: antochow@ur.edu.pl, +48 17 872 34 39, +48 17 872 36 46.\n" +
                            "3) Pani/Pana dane osobowe przetwarzane b\u0119d\u0105 w celach prowadzenia dzia\u0142alno\u015Bci organizacji studenckich/k\u00F3\u0142 naukowych na podstawie ustawy z dnia 20 lipca 2018 r. \u2013 Prawo o szkolnictwie wy\u017Cszym i nauce (Dz. U. z 2018 r. poz. 1668).\n" +
                            "4) Pani/Pana dane osobowe przetwarzane b\u0119d\u0105 na podstawie art. 6 ust. 1 lit. b ww. Rozporz\u0105dzenia, (przetwarzanie jest niezb\u0119dne do wykonania umowy, kt\u00F3rej stron\u0105 jest osoba, kt\u00F3rej dane dotycz\u0105, lub do podj\u0119cia dzia\u0142a\u0144 na \u017C\u0105danie osoby, kt\u00F3rej dane dotycz\u0105, przed zawarciem umowy).\n" +
                            "5) Podanie danych jest dobrowolne, jednak niezb\u0119dne do realizacji celu, do jakiego zosta\u0142y zebrane.\n" +
                            "6) Pani/Pani dane osobowe przechowywane b\u0119d\u0105 przez okres niezb\u0119dny do realizacji ww. celu z uwzgl\u0119dnieniem okres\u00F3w przechowywania okre\u015Blonych w przepisach odr\u0119bnych, w tym przepis\u00F3w archiwalnych.\n" +
                            "7) Odbiorcami Pani/Pana danych b\u0119d\u0105 podmioty, kt\u00F3re na podstawie zawartych um\u00F3w przetwarzaj\u0105 dane osobowe w imieniu Administratora.\n" +
                            "8) Posiada Pani/Pan prawo do: \u017C\u0105dania dost\u0119pu do tre\u015Bci swoich danych osobowych, do ich sprostowania, usuni\u0119cia lub ograniczenia przetwarzania, prawo do wniesienia sprzeciwu wobec przetwarzania, a tak\u017Ce prawo do przenoszenia danych.\n" +
                            "9) Posiada Pani/Pan prawo wniesienia skargi do organu nadzorczego, gdy uzasadnione jest, \u017Ce Pani/Pana dane osobowe przetwarzane s\u0105 przez Administratora Danych niezgodnie z ww. Rozporz\u0105dzeniem."
                        ).FontSize(7.5f).FontColor("#475569").Justify();
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().BorderTop(0.5f).BorderColor("#cbd5e1").PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Line("al. T. Rejtana 16c, 35-959 Rzesz\u00F3w").FontSize(7.5f).FontColor("#64748b");
                                t.Line("tel. +48 17 872 10 10, faks +48 17 872 12 65").FontSize(7.5f).FontColor("#64748b");
                                t.Line("rektorur@ur.edu.pl").FontSize(7.5f).FontColor("#64748b");
                            });
                        });
                    });
                });
            })
            .GeneratePdf(outputPath);

            return Task.CompletedTask;
        }
    }
}