using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace GameUpdater.Services.Download
{
    public class DownloadManager
    {
        private readonly Queue<DownloadFile> _files;
        private BackgroundWorker _worker;
        private bool _downloading;
        private FileDownloader _fd;
        private long _downloadedAmount;
        private long _totalDownloadedAmount;
        private long _totalAmountToDownload;
        
        public delegate void ProgressChangedHandler(object sender, int progress);
        public delegate void DownloadCompleteHandler(object sender);
        public delegate void ErrorHandler(object sender, string error);

        public delegate void FileChangeHandler(object sender, string currentFile);
        public event ProgressChangedHandler OnProgressChanged;
        public event DownloadCompleteHandler OnDownloadComplete;
        public event ErrorHandler OnError;
        public event FileChangeHandler OnFileChanged;

        public DownloadManager(Queue<DownloadFile> files, long totalAmountToDownload)
        {
            _files = files;
            _totalAmountToDownload = totalAmountToDownload;
        }

        public void StartDownload()
        {
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += _workerDoWork;
            _worker.ProgressChanged += _workerProgressChanged;
            _worker.RunWorkerCompleted += _workerCompleted;
            _totalAmountToDownload = 0;
            
            var tempClient = new WebClient();

            foreach (var file in _files)
            {
                tempClient.OpenRead(file.URL);
                _totalAmountToDownload += Convert.ToInt64(tempClient.ResponseHeaders["Content-Length"]);
            }

            _worker.RunWorkerAsync();
        }
        
        /*
         * WORKER
         */

        private void _workerDoWork(object sender, DoWorkEventArgs e)
        {
            while (_files.Count > 0 || _downloading)
            {
                if (_worker.CancellationPending)
                {
                    //_wc.CancelAsync();
                    _fd.Pause();
                    e.Cancel = true;
                    return;
                }

                if (!_downloading)
                    _downloadNextFile();
            }
        }
        
        private void _workerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                OnDownloadComplete?.Invoke(this);
            }
            OnProgressChanged?.Invoke(this, e.ProgressPercentage);
        }

        private void _workerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OnError?.Invoke(this, e.Error.Message);
                return;
            }
            OnDownloadComplete?.Invoke(this);
        }
        
        /*
         * DOWNLOADER
         */

        private void _downloadNextFile()
        {
            if (_files.Count <= 0) return;
            _downloading = true;
            var file = _files.Dequeue();
            OnFileChanged?.Invoke(this, file.FileName);
            var filePath = Path.GetFullPath("./") + file.FileName;
            if(File.Exists(filePath))
                File.Delete(filePath);
            else
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            _fd = new FileDownloader(file.URL, file.FileName);
            _fd.OnProgress += _downloadProgressChanged;
            _fd.OnFinish += _downloadComplete;
            _fd.Start();
        }

        private void _downloadProgressChanged(object sender, long bytesReceived)
        {
            _downloadedAmount = bytesReceived;
            var progress = (int) ((_totalDownloadedAmount + _downloadedAmount) / (double) _totalAmountToDownload * 100);
            OnProgressChanged?.Invoke(this, progress);
        }
        private void _downloadComplete(object sender)
        {
            _totalDownloadedAmount += _downloadedAmount;
            _downloading = false;
        }

        public void PauseDownload()
        {
            _fd?.Pause();
        }

        public void ResumeDownload()
        {
            _fd?.Start();
        }

        public void SetMaxBytesPerSecond(long maxBytesPerSecond)
        {
            if (_fd == null) return;
            _fd.MaxBytesPerSecond = maxBytesPerSecond;
        }
    }
}