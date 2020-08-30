using System;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class DownloaderViewModel : ViewModelBase
    {
        private readonly Updater _updater;
        private readonly MainWindowViewModel _parentViewModel;
        public DownloaderViewModel(MainWindowViewModel parentViewModel, Updater updater)
        {
            _updater = updater;
            _parentViewModel = parentViewModel;
            updater.OnPatchProgress += _onProgressChanged;
            updater.OnPatchChanged += _onPatchChanged;
            Progress = 0;
            PauseVisible = true;
        }

        public void PauseDownload()
        {
            PauseVisible = false;
            PlayVisible = true;
            _updater?.Pause();
        }
        
        public void ResumeDownload()
        {
            PauseVisible = true;
            PlayVisible = false;
            _updater?.Resume();
        }

        public void OpenDownloadSettings()
        {
            _parentViewModel.OpenPopup(new DownloadSettingsViewModel(_parentViewModel, _updater));
        }

        private void _onPatchChanged(object sender, int currentPatch, int amountPatches)
        {
            PatchInfo = $"Downloading Patch {currentPatch}/{amountPatches}";
        }

        private void _onProgressChanged(object sender, int progress)
        {
            Progress = progress;
            long downloadAmountLong = _updater.TotalDownloadAmount;
            var downloadAmount = Convert.ToDouble(downloadAmountLong);
            var downloadAmountUnit = Util.FormatBytes(ref downloadAmount);
            downloadAmount = Math.Round(downloadAmount, 2);
            var downloadedAmount = Convert.ToDouble(_updater.DownloadedAmount);
            var downloadedAmountUnit = Util.FormatBytes(ref downloadedAmount);
            downloadedAmount = Math.Round(downloadedAmount, 2);
            ProgressText = $"{downloadedAmount} {downloadedAmountUnit}/{downloadAmount} {downloadAmountUnit}";
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }
        
        private string _patchInfo;
        public string PatchInfo
        {
            get => _patchInfo;
            set => this.RaiseAndSetIfChanged(ref _patchInfo, value);
        }
        
        private bool _pauseVisible;
        public bool PauseVisible
        {
            get => _pauseVisible;
            set => this.RaiseAndSetIfChanged(ref _pauseVisible, value);
        }
        
        private bool _playVisible;
        public bool PlayVisible
        {
            get => _playVisible;
            set => this.RaiseAndSetIfChanged(ref _playVisible, value);
        }
        
    }
}