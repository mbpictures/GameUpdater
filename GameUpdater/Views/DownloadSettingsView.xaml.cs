using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameUpdater.Services;
using GameUpdater.Services.Download;
using GameUpdater.ViewModels;
using ReactiveUI;

namespace GameUpdater.Views
{
    public class DownloadSettingsView : UserControl
    {
        private Slider _speedSlider;
        private TextBlock _speedText;
        public DownloadSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _speedSlider = this.FindControl<Slider>("SpeedSlider");
            _speedText = this.FindControl<TextBlock>("SpeedText");
            _speedSlider.Value = Convert.ToDouble(IniLoader.Instance.Read("MaxBytesPerSecond", "Settings"));
            _speedSlider.WhenAnyValue(x => x.Value).Subscribe(_onSpeedSliderChange);
        }

        private void _onSpeedSliderChange(double value)
        {
            _speedText.Text = _formatDownloadSpeed(value, _speedSlider.Maximum);
            ((DownloadSettingsViewModel) DataContext)?.SetMaxBytesPerSecond(
                Convert.ToInt64(GetRealBytesPerSecondsFromValue(value,_speedSlider.Maximum)), value);
        }

        public static double GetRealBytesPerSecondsFromValue(double value, double max)
        {
            return Math.Abs(value - max) < 0.001 ? ThrottledStream.Infinite : (-Math.Pow(Math.E, (-value + 1500) * 0.01) + 3270000) * 10;
        }

        private static string _formatDownloadSpeed(double value, double max)
        {
            var isInfinite = Math.Abs(value - max) < 0.001;
            value = GetRealBytesPerSecondsFromValue(value, max);
            string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s", "TB/s" };
            var order = 0;
            while (value >= 1024 && order < sizes.Length - 1) {
                order++;
                value /= 1024;
            }

            value = Math.Round(value);
            
            var speedText = isInfinite ? "Infinite" : $"{value} {sizes[order]}";
            return $"Speed: {speedText}";
        }
    }
}