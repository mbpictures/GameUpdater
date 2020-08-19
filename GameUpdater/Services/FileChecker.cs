using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using GameUpdater.Services.Download;

namespace GameUpdater.Services
{
    public class FileChecker
    {
        private readonly Stack<Updater.Patch> _patches;

        public delegate void UpdateCurrentInfo(object sender, string currentFile);

        public event UpdateCurrentInfo OnUpdateCurrentInfo;
        
        public FileChecker(Stack<Updater.Patch> patches)
        {
            _patches = patches;
        }

        public List<DownloadFile> GetCorruptedFiles()
        {
            var temp = new Dictionary<string, DownloadFile>();
            
            while (_patches.Count > 0)
            {
                var patch = _patches.Pop();
                foreach (var file in patch.Files)
                {
                    OnUpdateCurrentInfo?.Invoke(this, file.FileName);
                    if (file.ZIP) continue;
                    var fileValid = true;
                    if (!string.IsNullOrEmpty(file.MD5))
                        fileValid &= _verifyMd5(file.FileName, file.MD5);
                    if (!string.IsNullOrEmpty(file.SHA1))
                        fileValid &= _verifySha1(file.FileName, file.SHA1);

                    if (!fileValid)
                    {
                        temp.Add(file.FileName, file);
                        continue;
                    }

                    temp.Remove(file.FileName);
                }
            }
            
            _checkChecksumFiles(temp);

            return temp.Values.ToList();
        }

        private static void _checkChecksumFiles(Dictionary<string, DownloadFile> dict)
        {
            var checkFiles = Directory.GetFiles(
                IniLoader.Instance.Read("GameDirectory", "General"), 
                "*.checksum.xml",
                SearchOption.AllDirectories);
            checkFiles.ToList().ForEach(checkFile =>
            {
                var doc = new XmlDocument();
                doc.Load(checkFile);
                var zipFilename = doc.SelectSingleNode("checksums/zipFilename").InnerText;
                var url = doc.SelectSingleNode("checksums/url")?.InnerText;
                foreach (var file in doc.GetElementsByTagName("file").Cast<XmlNode>().ToList())
                {
                    var filename = Path.Combine(Path.GetDirectoryName(checkFile) ?? "./", file.SelectSingleNode("filename").InnerText);
                    var md5 = file.SelectSingleNode("md5")?.InnerText;
                    var sha1 = file.SelectSingleNode("sha1")?.InnerText;

                    var fileValid = true;

                    if (!string.IsNullOrEmpty(md5)) 
                        fileValid &= _verifyMd5(filename, md5);
                    if (!string.IsNullOrEmpty(sha1)) 
                        fileValid &= _verifySha1(filename, sha1);

                    if (!fileValid)
                    {
                        dict.Add(filename, new DownloadFile(url, zipFilename, md5, sha1, true));
                        continue;
                    }

                    dict.Remove(filename);
                }
            });
        }

        private static bool _verifyMd5(string filename, string expectedMd5)
        {
            if (!File.Exists(filename)) return false;
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return expectedMd5.Equals(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
        }

        private static bool _verifySha1(string filename, string expectedSha1)
        {
            if (!File.Exists(filename)) return false;
            using var stream = File.OpenRead(filename);
            using var sha = new SHA1Managed();
            var checksum = sha.ComputeHash(stream);
            var sendCheckSum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
            return expectedSha1.Equals(sendCheckSum);
        }
    }
}