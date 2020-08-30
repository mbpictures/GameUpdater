using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameUpdater.Services;
using GameUpdater.ViewModels;
using ReactiveUI;

namespace GameUpdater.Views
{
    public class DownloadSettingsView : UserControl
    {
        private Slider _speedSlider;
        private TextBlock _speedText;
        private CheckBox _autoStartCheckbox;
        public DownloadSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _speedSlider = this.FindControl<Slider>("SpeedSlider");
            _speedText = this.FindControl<TextBlock>("SpeedText");
            _speedSlider.Value = Convert.ToDouble(IniLoader.Instance.Read("MaxBytesPerSecond", "Settings") ?? $"{_speedSlider.Maximum}");
            _speedSlider.WhenAnyValue(x => x.Value).Subscribe(_onSpeedSliderChange);

            _autoStartCheckbox = this.FindControl<CheckBox>("AutoStartCheckbox");
            _autoStartCheckbox.IsChecked = Convert.ToBoolean(IniLoader.Instance.Read("AutoStartGame", "Settings"));
            _autoStartCheckbox.WhenAnyValue(x => x.IsChecked).Subscribe(_onAutoStartCheckboxChange);
        }

        private void _onSpeedSliderChange(double value)
        {
            _speedText.Text = _formatDownloadSpeed(value, _speedSlider.Maximum);
            ((DownloadSettingsViewModel) DataContext)?.SetMaxBytesPerSecond(
                Convert.ToInt64(Util.GetRealBytesPerSecondsFromValue(value,_speedSlider.Maximum)), value);
        }

        private static void _onAutoStartCheckboxChange(bool? isChecked)
        {
            IniLoader.Instance.AddSetting("Settings", "AutoStartGame", isChecked.ToString());
            IniLoader.Instance.SaveSettings();
        }

        private static string _formatDownloadSpeed(double value, double max)
        {
            return $"Speed: {Util.FormatDownloadSpeed(Util.GetRealBytesPerSecondsFromValue(value, max), Math.Abs(value - max) < 0.001)}";
        }
    }
}