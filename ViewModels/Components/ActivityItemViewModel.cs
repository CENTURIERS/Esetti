using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels.Components 
{
    public partial class ActivityItemViewModel : ViewModelBase 
    {
        [ObservableProperty]
        private string _activityId;

        [ObservableProperty]
        private string _name;
        
        [ObservableProperty]
        private string _description;
        
        [ObservableProperty]
        private string _dateString;
        
        [ObservableProperty]
        private bool _isSelected;

        public ActivityItemViewModel(string activityId, string name, string description, string dateString, bool isSelected)
        {
            _activityId = activityId;
            _name = name;
            _description = description;
            _dateString = dateString;
            _isSelected = isSelected;
        }
    }
}