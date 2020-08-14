using System;
using System.Collections.Generic;
using System.Xml;

namespace GameUpdater.Services
{
    public class Updater
    {
        private readonly string _localManifest;
        private readonly string _remoteManifest;

        private XmlDocument _xmlLocal;
        private XmlDocument _xmlRemote;

        private Stack<Patch> _cachedPatches;
        private int _amountPatches;
        private int _currentPatchIndex = 1;
        private Patch _currentPatch;
        private bool _fileChecking;
        
        public delegate void PatchProgressHandler(object sender, int progress);
        public delegate void PatchChangedHandler(object sender, int currentPatch, int amountPatches);
        public delegate void PatchFinishedHandler(object sender);

        public event PatchProgressHandler OnPatchProgress;
        public event PatchChangedHandler OnPatchChanged;
        public event PatchFinishedHandler OnPatchFinished;

        public XmlDocument XmlLocal
        {
            get
            {
                if (_xmlLocal == null)
                {
                    _xmlLocal = new XmlDocument();
                    _xmlLocal.Load(_localManifest);
                }

                return _xmlLocal;
            }
        }

        public string LocalVersion => GetVersionOfNode(XmlLocal.SelectSingleNode("updater"));

        public XmlDocument XmlRemote
        {
            get
            {
                if (_xmlRemote == null)
                {
                    _xmlRemote = new XmlDocument();
                    _xmlRemote.Load(_remoteManifest);
                }

                return _xmlRemote;
            }
        }

        public Updater(string localManifest, string remoteManifest)
        {
            _localManifest = localManifest;
            _remoteManifest = remoteManifest;
        }

        public void StartDownload(bool fileChecking = true)
        {
            _cachedPatches = GetPatches();
            _amountPatches = _cachedPatches.Count;
            _currentPatchIndex = 1;
            _fileChecking = fileChecking;
            _downloadNextPatch();
        }

        private void _downloadNextPatch(bool redownloadPatch = false)
        {
            if (_cachedPatches.Count <= 0)
            {
                OnPatchFinished?.Invoke(this);
                return;
            }
            OnPatchChanged?.Invoke(this, _currentPatchIndex, _amountPatches);
            if(!(redownloadPatch && _fileChecking))
                _currentPatch = _cachedPatches.Pop();
            var downloader = new Downloader(_currentPatch.Files, 0);
            downloader.OnDownloadComplete += _downloadPatchComplete;
            downloader.OnProgressChanged += _downloadPatchProgress;
            downloader.StartDownload();
        }

        private void _downloadPatchComplete(object sender)
        {
            var patchVerified = true;
            if (_fileChecking)
            {
                var stack = new Stack<Patch>();
                stack.Push(FindPatch(_currentPatch.Version));
                var fc = new FileChecker(stack);
                var corruptedFiles = fc.GetCorruptedFiles();
                patchVerified = corruptedFiles.Count == 0;
                _currentPatch.Files = new Queue<DownloadFile>(corruptedFiles);
            }

            if (patchVerified)
            {
                _currentPatchIndex++;
                XmlLocal.SelectSingleNode("/updater/version").InnerText = _currentPatch.Version;
                XmlLocal.Save(_localManifest);
            }
            OnPatchProgress?.Invoke(this, 0);
            _downloadNextPatch(!patchVerified);
        }

        private void _downloadPatchProgress(object sender, int progress)
        {
            OnPatchProgress?.Invoke(this, progress);
        }
        
        /*
         * Returns true, when there are Updates available
         */
        public bool CheckUpdates()
        {
            var newestPatch = GetNewestPatch();
            return GetHighestVersionNumber(LocalVersion, newestPatch.Version) != LocalVersion;
        }

        public Stack<Patch> GetPatches(bool fullList = false)
        {
            if (!CheckUpdates() && !fullList) return new Stack<Patch>();
            
            var stack = new Stack<Patch>();

            var lCurrentPatch = GetNewestPatch();

            while (lCurrentPatch != null && (!PatchIsResolved(lCurrentPatch) || fullList))
            {
                stack.Push(lCurrentPatch);
                lCurrentPatch = FindPatch(lCurrentPatch?.DependsOn);
            }
            
            return stack;
        }

        public Patch GetNewestPatch()
        {
            XmlNode newestPatch = null;
            var patches = XmlRemote.GetElementsByTagName("patch");
            for (var i = 0; i < patches.Count; i++)
            {
                var node = patches[i];
                var version = GetVersionOfNode(node);
                newestPatch = GetHighestVersionNumber(version, GetVersionOfNode(newestPatch)) == version ? node : newestPatch;
            }

            return newestPatch == null ? null : Patch.ParseXmlNode(newestPatch);
        }

        public Patch FindPatch(string version)
        {
            var patches = XmlRemote.GetElementsByTagName("patch");
            for (var i = 0; i < patches.Count; i++)
            {
                var node = patches[i];
                if (version == GetVersionOfNode(node)) return Patch.ParseXmlNode(node);
            }

            return null;
        }

        public bool PatchIsResolved(Patch patch)
        {
            return patch.Version.Equals(LocalVersion);
        }

        public static string GetVersionOfNode(XmlNode node)
        {
            return node?.SelectSingleNode("version").InnerText;
        }

        public string GetHighestVersionNumber(string ver1, string ver2)
        {
            if (string.IsNullOrEmpty(ver1)) return ver2;
            if (string.IsNullOrEmpty(ver2)) return ver1;
            return Version.Parse(ver2).CompareTo(Version.Parse(ver1)) == 1 ? ver2 : ver1;
        }

        public class Patch
        {
            public Queue<DownloadFile> Files;
            public string Version;
            public string DependsOn;

            public static Patch ParseXmlNode(XmlNode node)
            {
                var patch = new Patch
                {
                    Version = GetVersionOfNode(node),
                    Files = new Queue<DownloadFile>(),
                    DependsOn = node.SelectSingleNode("dependsOn").InnerText
                };

                foreach (XmlNode file in node.SelectSingleNode("files").SelectNodes("file"))
                {
                    var checksum = file.SelectSingleNode("checksum");
                    patch.Files.Enqueue(new DownloadFile(
                        file.SelectSingleNode("url").InnerText,
                        file.SelectSingleNode("location").InnerText,
                        checksum.SelectSingleNode("md5")?.InnerText,
                        checksum.SelectSingleNode("sha1")?.InnerText));
                }

                return patch;
            }
        }
    }
}