using System.Collections.Generic;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class DownloaderViewModel : ViewModelBase
    {
        public DownloaderViewModel()
        {
            Updater dwnl = new Updater("./update.xml", "http://projects.marius-butz.de/updater/update.xml");
            dwnl.OnPatchProgress += _onProgressChanged;
            dwnl.OnPatchChanged += _onPatchChanged;
            dwnl.StartDownload();
            Progress = 0;
        }

        private void _onPatchChanged(object sender, int currentPatch, int amountPatches)
        {
            PatchInfo = $"Downloading Patch {currentPatch}/{amountPatches}";
        }

        private void _onProgressChanged(object sender, int progress)
        {
            Progress = progress;
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }
        
        private string _patchInfo;
        public string PatchInfo
        {
            get => _patchInfo;
            set => this.RaiseAndSetIfChanged(ref _patchInfo, value);
        }
        
        
    }
}