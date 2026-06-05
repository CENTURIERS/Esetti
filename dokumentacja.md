# Esetti - System Zarządzania Kołami Naukowymi i Generowania Dokumentacji

Projekt Esetti to innowacyjny system desktopowy przeznaczony do kompleksowego wspomagania działalności kół naukowych, którego architektura została zorientowana na pełną skalowalność międzyuczelnianą. Aplikacja rozwiązuje problem trudnej ewidencji członków, projektów badawczych, spotkań oraz wyjazdów w ramach struktur akademickich, umożliwiając jednoczesną i niezależną obsługę wielu uczelni oraz ich wydziałów. Kluczowym elementem wyróżniającym system jest moduł automatycznego generowania spersonalizowanych zaświadczeń oraz list członków bezpośrednio do plików PDF przy użyciu nowoczesnego i wydajnego silnika QuestPDF.

## Uruchomienie projektu (developer)

| Technologia | Wersja | Zastosowanie | Link |
| :--- | :--- | :--- | :--- |
| **.NET SDK** | `9.0` | Środowisko deweloperskie i uruchomieniowe | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **C#** | `13` | Język programowania logicznego | [learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/csharp/) |
| **Avalonia UI** | `11.3.14` | Framework do wieloplatformowego interfejsu graficznego | [avaloniaui.net](https://avaloniaui.net/) |
| **EF Core SQLite** | `9.0.2` | O/RM dla zarządzania i komunikacji z bazą danych | [learn.microsoft.com](https://learn.microsoft.com/en-us/ef/core/) |
| **QuestPDF** | `2026.5.0` | Silnik do generowania dokumentów PDF | [questpdf.com](https://www.questpdf.com/) |

### Wymagania programowe

Aby zbudować i uruchomić projekt w trybie deweloperskim na czystym komputerze, wymagane są:
- **System operacyjny:** Windows 10 (wersja 1809+) lub Windows 11 (preferowany).
- **Środowisko uruchomieniowe / SDK:** .NET SDK 9.0 lub nowszy.
- **Baza danych:** SQLite (silnik SQLite jest wbudowany i plik bazy `.db` jest generowany automatycznie przy pierwszym uruchomieniu projektu).
- **IDE (opcjonalnie):** Visual Studio 2022, JetBrains Rider lub VS Code z zainstalowanym zestawem narzędzi C# Dev Kit.
- **Narzędzia CLI (opcjonalnie do migracji):** `dotnet-ef` w wersji 9.0.2 w celu zarządzania ewentualnymi migracjami bazy danych.

### Instrukcja uruchomienia (CLI)

1. Sklonuj lub rozpakuj kod źródłowy projektu do wybranego folderu.
2. Otwórz terminal w katalogu głównym projektu (tam, gdzie znajduje się plik `Essetti.csproj`).
3. Uruchom polecenie przywracania pakietów NuGet:
   ```bash
   dotnet restore
   ```
4. Zbuduj i uruchom aplikację za pomocą polecenia:
   ```bash
   dotnet run
   ```
5. Baza danych SQLite (`Data/esseti.db`) zostanie automatycznie zainicjalizowana, a tabele uzupełnione o testowe dane początkowe przy pierwszym starcie aplikacji.
