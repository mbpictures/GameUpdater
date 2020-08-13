using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameUpdater.Views
{
    public class ManageGameView : UserControl
    {
        public ManageGameView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}