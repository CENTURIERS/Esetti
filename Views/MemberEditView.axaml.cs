using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Esseti.Views
{
    public partial class MemberEditView : UserControl
    {
        public MemberEditView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
