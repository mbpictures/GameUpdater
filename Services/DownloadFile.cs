#nullable enable

namespace GameUpdater.Services
{
    public readonly struct DownloadFile
    {
        public readonly string URL;
        public readonly string FileName;
        public readonly string MD5;
        public readonly string SHA1;

        public DownloadFile(string url, string fileName, string md5, string sha1)
        {
            URL = url;
            FileName = fileName;
            MD5 = md5;
            SHA1 = sha1;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && FileName.Equals(((DownloadFile) obj).FileName);
        }

        public bool Equals(DownloadFile other)
        {
            return FileName == other.FileName;
        }

        public override int GetHashCode()
        {
            return (FileName != null ? FileName.GetHashCode() : 0);
        }
    }
}