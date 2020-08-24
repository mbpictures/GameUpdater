using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

// ORIGINAL PROGRAM CODE FROM https://stackoverflow.com/a/15996441/4613465

namespace GameUpdater.Services.Download
{
    public class FileDownloader
    {
        public delegate void ProgressHandler(object sender, long progress);
        public event ProgressHandler OnProgress;
        public delegate void FinishedHandler(object sender);
        public event FinishedHandler OnFinish;
        
        private readonly int _chunkSize;
        private readonly Lazy<long> _contentLength;
        private readonly string _destination;
        private readonly IProgress<double> _progress;
        private readonly string _sourceUrl;
        private volatile bool _allowedToRun;
        private long _maxBytesPerSecond;

        public long MaxBytesPerSecond
        {
            get => _maxBytesPerSecond;
            set
            {
                _maxBytesPerSecond = value;
                if(_responseStream != null)
                    _responseStream.MaximumBytesPerSecond = _maxBytesPerSecond;
            }
        }

        private ThrottledStream _responseStream;

        public FileDownloader(string source, string destination, long maxBytesPerSecond = ThrottledStream.Infinite, int chunkSizeInBytes = 10000 /*Default to 0.01 mb*/,
            IProgress<double> progress = null)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));

            _allowedToRun = true;
            _sourceUrl = source;
            _destination = destination;
            _chunkSize = chunkSizeInBytes;
            _contentLength = new Lazy<long>(GetContentLength);
            _progress = progress;
            _maxBytesPerSecond = maxBytesPerSecond;

            if (!File.Exists(destination))
                BytesWritten = 0;
            else
                try
                {
                    BytesWritten = new FileInfo(destination).Length;
                }
                catch
                {
                    BytesWritten = 0;
                }
        }

        public long BytesWritten { get; private set; }
        public long ContentLength => _contentLength.Value;

        public bool Done => ContentLength == BytesWritten;

        private long GetContentLength()
        {
            var request = (HttpWebRequest) WebRequest.Create(_sourceUrl);
            request.Method = "HEAD";

            using var response = request.GetResponse();
            return response.ContentLength;
        }

        private async Task Start(long range)
        {
            if (!_allowedToRun)
                throw new InvalidOperationException();

            if (Done)
                //file has been found in folder destination and is already fully downloaded 
                return;

            var request = (HttpWebRequest) WebRequest.Create(_sourceUrl);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            request.AddRange(range);

            using var response = await request.GetResponseAsync();
            _responseStream = new ThrottledStream(response.GetResponseStream(), _maxBytesPerSecond);
            await using var fs = new FileStream(_destination, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            while (_allowedToRun)
            {
                var buffer = new byte[_chunkSize];
                {
                    var bytesRead = _responseStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        OnFinish?.Invoke(this);
                        break;
                    }

                    await fs.WriteAsync(buffer, 0, bytesRead);
                    BytesWritten += bytesRead;
                }

                _progress?.Report((double) BytesWritten / ContentLength);
                OnProgress?.Invoke(this, BytesWritten);
            }
            
            await fs.FlushAsync();
        }

        public Task Start()
        {
            _allowedToRun = true;
            return Start(BytesWritten);
        }

        public void Pause()
        {
            _allowedToRun = false;
        }
    }
}