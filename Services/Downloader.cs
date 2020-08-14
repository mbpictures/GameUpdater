using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace GameUpdater.Services
{
    public class Downloader
    {
        private Queue<DownloadFile> _files;
        private BackgroundWorker _worker;
        private bool _downloading = false;
        private WebClient _wc;
        private long downloadedAmount = 0;
        private long _totalDownloadedAmount = 0;
        private long _totalAmountToDownload = 0;
        
        public delegate void ProgressChangedHandler(object sender, int progress);
        public delegate void DownloadCompleteHandler(object sender);
        public delegate void ErrorHandler(object sender, string error);

        public delegate void FileChangeHandler(object sender, string currentFile);
        public event ProgressChangedHandler OnProgressChanged;
        public event DownloadCompleteHandler OnDownloadComplete;
        public event ErrorHandler OnError;
        public event FileChangeHandler OnFileChanged;

        public Downloader(Queue<DownloadFile> files, long totalAmountToDownload)
        {
            _files = files;
            _wc = new WebClient();
            _totalAmountToDownload = totalAmountToDownload;
        }

        public void StartDownload()
        {
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += workerDoWork;
            _worker.ProgressChanged += workerProgressChanged;
            _worker.RunWorkerCompleted += workerCompleted;
            _totalAmountToDownload = 0;
            
            WebClient tempClient = new WebClient();

            foreach (DownloadFile file in _files)
            {
                tempClient.OpenRead(file.URL);
                _totalAmountToDownload += Convert.ToInt64(tempClient.ResponseHeaders["Content-Length"]);
            }

            _worker.RunWorkerAsync();
        }
        
        /*
         * WORKER
         */

        private void workerDoWork(object sender, DoWorkEventArgs e)
        {
            while (_files.Count > 0 || _downloading)
            {
                if (_worker.CancellationPending)
                {
                    _wc.CancelAsync();
                    e.Cancel = true;
                    return;
                }

                if (!_downloading)
                    downloadNextFile();
            }
        }
        
        private void workerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                OnDownloadComplete?.Invoke(this);
            }
            OnProgressChanged?.Invoke(this, e.ProgressPercentage);
        }

        private void workerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

        private void downloadNextFile()
        {
            if (_files.Count <= 0) return;
            _downloading = true;
            DownloadFile file = _files.Dequeue();
            OnFileChanged?.Invoke(this, file.FileName);
            string filePath = System.IO.Path.GetFullPath("./") + file.FileName;
            if(System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            else
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
            _wc.DownloadProgressChanged += downloadProgressChanged;
            _wc.DownloadFileCompleted += downloadComplete;
            _wc.DownloadFileTaskAsync(new Uri(file.URL), System.IO.Path.GetFullPath("./") + file.FileName);
        }

        private void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadedAmount = e.BytesReceived;
            int progress = (int) (((_totalDownloadedAmount + downloadedAmount) / (double) _totalAmountToDownload) * 100);
            OnProgressChanged?.Invoke(this, progress);
        }
        private void downloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            _totalDownloadedAmount += downloadedAmount;
            _downloading = false;
        }
    }
}