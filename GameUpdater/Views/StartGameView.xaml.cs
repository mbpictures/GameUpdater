using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameUpdater.Views
{
    public class StartGameView : UserControl
    {
        public StartGameView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}