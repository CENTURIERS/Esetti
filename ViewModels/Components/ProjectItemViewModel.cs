using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels.Components
{
    /// <summary>
    /// Model widoku reprezentujący pojedynczy projekt na liście projektów (kafelek projektu).
    /// Przechowuje podstawowe dane o projekcie oraz stan zaznaczenia checkboxem.
    /// </summary>
    public partial class ProjectItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Unikalny identyfikator projektu (jako ciąg znaków).
        /// </summary>
        [ObservableProperty]
        private string _projectId;

        /// <summary>
        /// Nazwa projektu.
        /// </summary>
        [ObservableProperty]
        private string _name;

        /// <summary>
        /// Opis lub cele projektu.
        /// </summary>
        [ObservableProperty]
        private string _description;

        /// <summary>
        /// Szacowany czas trwania projektu (w godzinach) jako ciąg znaków.
        /// </summary>
        [ObservableProperty]
        private string _estimatedTime;

        /// <summary>
        /// Imię i nazwisko lidera odpowiedzialnego za ten projekt.
        /// </summary>
        [ObservableProperty]
        private string _leaderName;

        /// <summary>
        /// Czy dany projekt jest zaznaczony checkboxem na liście projektów.
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// Konstruktor kafelka projektu uzupełniający jego wszystkie podstawowe właściwości.
        /// </summary>
        /// <param name="projectId">Identyfikator projektu.</param>
        /// <param name="name">Nazwa projektu.</param>
        /// <param name="description">Opis.</param>
        /// <param name="estimatedTime">Szacowany czas.</param>
        /// <param name="leaderName">Imię i nazwisko lidera.</param>
        /// <param name="isSelected">Czy zaznaczony.</param>
        public ProjectItemViewModel(string projectId, string name, string description, string estimatedTime, string leaderName, bool isSelected)
        {
            _projectId = projectId;
            _name = name;
            _description = description;
            _estimatedTime = estimatedTime;
            _leaderName = leaderName;
            _isSelected = isSelected;
        }
    }
}
