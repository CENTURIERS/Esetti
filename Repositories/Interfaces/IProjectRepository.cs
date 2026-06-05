using Models.Activities;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    /// <summary>
    /// Interfejs do operacji na projektach w bazie danych.
    /// Obsługuje podstawowe operacje CRUD i aktualizowanie studentów zaangażowanych w dany projekt.
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Pobiera z bazy listę wszystkich aktywnych projektów wraz z osobami za nie odpowiedzialnymi.
        /// </summary>
        /// <returns>Lista wszystkich projektów z bazy.</returns>
        Task<List<Project>> GetAllProjectsAsync();

        /// <summary>
        /// Wyciąga z bazy konkretny projekt po jego ID wraz z listą uczestników i osobą odpowiedzialną.
        /// </summary>
        /// <param name="id">ID projektu, który chcemy pobrać.</param>
        /// <returns>Obiekt projektu lub null, jeśli nie znaleziono go w bazie.</returns>
        Task<Project?> GetProjectByIdAsync(int id);

        /// <summary>
        /// Dodaje zupełnie nowy projekt do bazy danych i ustawia go jako aktywny.
        /// </summary>
        /// <param name="project">Obiekt projektu, który zapisujemy.</param>
        Task AddProjectAsync(Project project);

        /// <summary>
        /// Aktualizuje dane projektu w bazie i opcjonalnie listę jego uczestników.
        /// </summary>
        /// <param name="project">Obiekt projektu z nowymi danymi.</param>
        /// <param name="participantIds">Opcjonalna lista ID-ków studentów przypisanych do tego projektu.</param>
        Task UpdateProjectAsync(Project project, IEnumerable<int>? participantIds = null);

        /// <summary>
        /// Podmienia całą listę uczestników projektu na nową listę podaną w parametrze.
        /// </summary>
        /// <param name="projectId">ID projektu, w którym zmieniamy ludzi.</param>
        /// <param name="participantIds">Nowa lista ID-ków członków, którzy mają być w projekcie.</param>
        Task UpdateProjectParticipantsAsync(int projectId, IEnumerable<int> participantIds);

        /// <summary>
        /// Usuwa pojedynczy projekt z bazy (a dokładniej flaguje go jako nieaktywny).
        /// </summary>
        /// <param name="id">ID projektu do wyrzucenia.</param>
        Task DeleteSingleProjectAsync(int id);

        /// <summary>
        /// Usuwa grupowo projekty po ich ID-kach (ustawia IsActive na false).
        /// </summary>
        /// <param name="projectIds">Lista ID-ków projektów do wywalenia.</param>
        Task DeleteProjectsAsync(IEnumerable<int> projectIds);
    }
}


