using Models.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    /// <summary>
    /// Interfejs do obsługi aktywności koła naukowego w bazie danych.
    /// Zawiera standardowe operacje CRUD i przypisywanie studentów do danych wydarzeń/aktywności.
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Wyciąga z bazy wszystkie aktywne wydarzenia/aktywności koła.
        /// </summary>
        /// <returns>Lista aktywności z bazy danych.</returns>
        Task<List<Activity>> GetAllActivitiesAsync();

        /// <summary>
        /// Pobiera jedną konkretną aktywność po jej ID z bazy, razem z listą uczestników.
        /// </summary>
        /// <param name="id">ID szukanej aktywności.</param>
        /// <returns>Zwraca aktywność lub null, jak nie ma w bazie takiego ID.</returns>
        Task<Activity?> GetActivityByIdAsync(int id);

        /// <summary>
        /// Dodaje nową aktywność do bazy danych.
        /// </summary>
        /// <param name="activity">Obiekt nowej aktywności.</param>
        Task AddActivityAsync(Activity activity);

        /// <summary>
        /// Aktualizuje dane aktywności w bazie i opcjonalnie podmienia listę uczestników.
        /// </summary>
        /// <param name="activity">Obiekt aktywności ze zmienionymi wartościami.</param>
        /// <param name="participantIds">Opcjonalne ID-ki studentów, którzy w tym biorą udział.</param>
        Task UpdateActivityAsync(Activity activity, IEnumerable<int>? participantIds = null);

        /// <summary>
        /// Aktualizuje samą listę uczestników przypisanych do danej aktywności.
        /// </summary>
        /// <param name="activityId">ID modyfikowanej aktywności.</param>
        /// <param name="participantIds">Nowa lista ID-ków członków koła biorących udział.</param>
        Task UpdateActivityParticipantsAsync(int activityId, IEnumerable<int> participantIds);

        /// <summary>
        /// Usuwa jedną aktywność (ustawia IsActive na false w bazie).
        /// </summary>
        /// <param name="id">ID usuwanej aktywności.</param>
        Task DeleteSingleActivityAsync(int id);

        /// <summary>
        /// Usuwa grupowo aktywności po zestawie ich ID-ków (ustawia IsActive na false).
        /// </summary>
        /// <param name="activitiesIds">Lista ID-ków aktywności do wywalenia.</param>
        Task DeleteActivitesAsync(IEnumerable<int> activitiesIds);
    }
}


