using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameUpdater.Views
{
    public class CheckFilesView : UserControl
    {
        public CheckFilesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}