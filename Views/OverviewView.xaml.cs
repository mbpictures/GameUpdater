using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameUpdater.Views
{
    public class OverviewView : UserControl
    {
        public OverviewView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}