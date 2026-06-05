using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels.Components 
{
    /// <summary>
    /// Model widoku dla pojedynczego wiersza/kafelka aktywności na liście aktywności.
    /// Przechowuje podstawowe informacje o aktywności i jej stan zaznaczenia.
    /// </summary>
    public partial class ActivityItemViewModel : ViewModelBase 
    {
        /// <summary>
        /// Unikalny identyfikator aktywności (jako ciąg znaków).
        /// </summary>
        [ObservableProperty]
        private string _activityId;

        /// <summary>
        /// Nazwa aktywności.
        /// </summary>
        [ObservableProperty]
        private string _name;
        
        /// <summary>
        /// Opis lub dodatkowe uwagi do aktywności.
        /// </summary>
        [ObservableProperty]
        private string _description;
        
        /// <summary>
        /// Sformatowana data wydarzenia w formie tekstowej.
        /// </summary>
        [ObservableProperty]
        private string _dateString;
        
        /// <summary>
        /// Czy ta aktywność jest obecnie zaznaczona na liście checkboxem.
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// Czy ta aktywność jest powtarzalna cyklicznie.
        /// </summary>
        [ObservableProperty]
        private bool _isRepeatable;

        /// <summary>
        /// Konstruktor tworzący model kafelka aktywności z kompletem podstawowych danych.
        /// </summary>
        /// <param name="activityId">Identyfikator aktywności.</param>
        /// <param name="name">Nazwa aktywności.</param>
        /// <param name="description">Opis.</param>
        /// <param name="dateString">Sformatowana data jako string.</param>
        /// <param name="isSelected">Czy jest zaznaczona.</param>
        /// <param name="isRepeatable">Czy jest powtarzalna.</param>
        public ActivityItemViewModel(string activityId, string name, string description, string dateString, bool isSelected, bool isRepeatable)
        {
            _activityId = activityId;
            _name = name;
            _description = description;
            _dateString = dateString;
            _isSelected = isSelected;
            _isRepeatable = isRepeatable;
        }
    }
}
