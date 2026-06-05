using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Esseti.Views
{
    /// <summary>
    /// Widok formularza edycji danych członka koła.
    /// </summary>
    public partial class MemberEditView : UserControl
    {
        /// <summary>
        /// Inicjalizuje komponenty widoku edycji członka.
        /// </summary>
        public MemberEditView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ładuje definicję interfejsu z pliku XAML.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}


