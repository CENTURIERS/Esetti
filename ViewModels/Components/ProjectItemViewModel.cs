using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels.Components
{
    public partial class ProjectItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _projectId;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _estimatedTime;

        [ObservableProperty]
        private string _leaderName;

        [ObservableProperty]
        private bool _isSelected;

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