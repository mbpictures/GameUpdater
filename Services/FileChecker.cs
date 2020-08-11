using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GameUpdater.Services
{
    public class FileChecker
    {
        private Queue<Updater.Patch> _patches;
        public FileChecker(Queue<Updater.Patch> patches)
        {
            _patches = patches;
        }

        public List<DownloadFile> GetCorruptedFiles()
        {
            Updater.Patch patch;
            Dictionary<string, DownloadFile> temp = new Dictionary<string, DownloadFile>();
            
            while (_patches.Count > 0)
            {
                patch = _patches.Dequeue();
                foreach (DownloadFile file in patch.Files)
                {
                    bool fileValid = true;
                    if (!string.IsNullOrEmpty(file.MD5))
                        fileValid &= VerifyMd5(file.FileName, file.MD5);
                    if (!string.IsNullOrEmpty(file.SHA1))
                        fileValid &= VerifySha1(file.FileName, file.SHA1);

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

        public bool VerifyMd5(string filename, string expectedMd5)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return expectedMd5.Equals(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
        }

        public bool VerifySha1(string filename, string expectedSha1)
        {
            using var stream = File.OpenRead(filename);
            using var sha = new SHA1Managed();
            var checksum = sha.ComputeHash(stream);
            var sendCheckSum = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
            return expectedSha1.Equals(sendCheckSum);
        }
    }
}