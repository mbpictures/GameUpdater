using GameUpdater.Services;

namespace GameUpdater.ViewModels
{
    public class CheckFilesViewModel : ViewModelBase
    {
        public delegate void CheckFilesFinished(object sender);

        public event CheckFilesFinished OnCheckFilesFinished;

        public CheckFilesViewModel()
        {
            Updater updater = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            
        }
    }
}