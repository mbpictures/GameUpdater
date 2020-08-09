using System.Collections.Generic;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class DownloaderViewModel : ViewModelBase
    {
        public DownloaderViewModel()
        {
            Queue<DownloadFile> queue = new Queue<DownloadFile>();
            queue.Enqueue(new DownloadFile("https://file-examples-com.github.io/uploads/2017/04/file_example_MP4_1280_10MG.mp4", "test/test.mp4"));
            queue.Enqueue(new DownloadFile("https://file-examples-com.github.io/uploads/2017/04/file_example_MP4_1920_18MG.mp4", "test/test2.mp4"));
            Downloader dwnl = new Downloader(queue, 200);
            dwnl.StartDownload();
            dwnl.OnProgressChanged += DwnlOnOnProgressChanged;
            Progress = 0;
        }

        private void DwnlOnOnProgressChanged(object sender, int progress)
        {
            Progress = progress;
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }
        
        
    }
}