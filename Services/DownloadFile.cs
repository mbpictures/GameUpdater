namespace GameUpdater.Services
{
    public struct DownloadFile
    {
        public string URL;
        public string FileName;

        public DownloadFile(string URL, string FileName)
        {
            this.URL = URL;
            this.FileName = FileName;
        }
    }
}