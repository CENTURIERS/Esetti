using Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    /// <summary>
    /// Interfejs do obsługi operacji CRUD na członkach koła w bazie danych.
    /// Ogarnia pobieranie, dodawanie, edytowanie i usuwanie (dezaktywację) studentów/członków.
    /// </summary>
    public interface IMemberRepository
    {
        /// <summary>
        /// Pobiera z bazy wszystkich aktywnych członków koła razem z ich kontami i rolami.
        /// </summary>
        /// <returns>Lista aktywnych członków owinięta w Taska.</returns>
        Task<List<Member>> GetAllMembersAsync();
        
        /// <summary>
        /// Pobiera z bazy listę wszystkich dostępnych ról w klubie (np. zarząd, członek itp.).
        /// </summary>
        /// <returns>Lista ról z bazy danych.</returns>
        Task<List<Models.ClubBase.AuthorityRole>> GetAuthorityRolesAsync();
    
        /// <summary>
        /// Usuwa (dezaktywuje - ustawia IsActive na false) jednego członka o podanym ID.
        /// </summary>
        /// <param name="id">Identyfikator (id) członka do usunięcia.</param>
        Task DeleteSingleMemberAsync(int id);

        /// <summary>
        /// Usuwa grupowo członków z bazy po ich ID-kach (też ustawia IsActive na false).
        /// </summary>
        /// <param name="memberIds">Kolekcja ID-ków członków do wywalenia.</param>
        Task DeleteMembersAsync(IEnumerable<int> memberIds);

        /// <summary>
        /// Dodaje nowego członka do bazy. Jak już istnieje taki numer indeksu, to go reaktywuje i aktualizuje dane.
        /// </summary>
        /// <param name="member">Obiekt członka z danymi do zapisu.</param>
        /// <param name="departmentId">Opcjonalne ID wydziału, żeby przypisać go do odpowiedniego koła.</param>
        Task AddMemberAsync(Member member, int? departmentId);

        /// <summary>
        /// Pobiera szczegółowe dane jednego członka po jego ID wraz ze wszystkimi powiązanymi projektami i aktywnościami.
        /// </summary>
        /// <param name="id">ID szukanego członka.</param>
        /// <returns>Obiekt członka albo null, jak nie ma takiego w bazie.</returns>
        Task<Member?> GetMemberByIdAsync(int id);

        /// <summary>
        /// Aktualizuje dane członka w bazie i czyści projekty/aktywności, w których już nie uczestniczy.
        /// </summary>
        /// <param name="member">Obiekt członka z nowymi danymi.</param>
        /// <param name="remainingProjectIds">Lista ID projektów, w których członek ma pozostać.</param>
        /// <param name="remainingActivityIds">Lista ID aktywności, w których członek ma pozostać.</param>
        Task UpdateMemberAsync(Member member, List<int> remainingProjectIds, List<int> remainingActivityIds);

        /// <summary>
        /// Aktualizuje podstawowe dane członka (imię, nazwisko, telefon itp.) oraz przypisanie do wydziału/koła.
        /// </summary>
        /// <param name="member">Obiekt członka z podstawowymi informacjami.</param>
        /// <param name="departmentId">Opcjonalne ID wydziału.</param>
        Task UpdateMemberBasicInfoAsync(Member member, int? departmentId);

        /// <summary>
        /// Aktualizuje zdjęcie/avatar członka w bazie danych.
        /// </summary>
        /// <param name="memberId">ID członka, któremu zmieniamy avatar.</param>
        /// <param name="avatarData">Tablica bajtów ze zdjęciem.</param>
        Task UpdateMemberAvatarAsync(int memberId, byte[] avatarData);

        /// <summary>
        /// Pobiera z bazy listę wszystkich wydziałów uczelni.
        /// </summary>
        /// <returns>Lista wydziałów z bazy danych.</returns>
        Task<List<Models.University.CollegeDepartment>> GetCollegeDepartmentsAsync();
    }
}
