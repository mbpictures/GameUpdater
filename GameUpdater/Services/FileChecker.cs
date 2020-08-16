using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

            return temp.Values.ToList();
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