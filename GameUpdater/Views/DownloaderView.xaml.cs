using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameUpdater.Views
{
    public class DownloaderView : UserControl
    {
        public DownloaderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}