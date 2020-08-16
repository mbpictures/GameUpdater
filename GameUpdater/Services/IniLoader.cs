using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GameUpdater.Services
{
    public class IniLoader
    {
        public const string CommentDelimiter = ";";

        private static IniLoader _instance;
        private readonly string _exe = Assembly.GetExecutingAssembly().GetName().Name;
        private string _path;
        private readonly Hashtable _keyPairs = new Hashtable();

        private struct SectionPair
        {
            public string Section;
            public string Key;
        }

        private IniLoader()
        {
            Path = new FileInfo(_exe + ".ini").FullName;
        }

        public static IniLoader Instance => _instance ??= new IniLoader();

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                TextReader iniFile = null;
                string currentRoot = null;

                if (File.Exists(_path))
                    try
                    {
                        iniFile = new StreamReader(_path);
                        var strLine = iniFile.ReadLine();
                        while (strLine != null)
                        {
                            strLine = strLine.Trim();
                            if (strLine != "")
                            {
                                if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                                {
                                    currentRoot = strLine.Substring(1, strLine.Length - 2);
                                }
                                else
                                {
                                    var keyPair = strLine.Split(new[] {'='}, 2);
                                    SectionPair sectionPair;
                                    string keyValue = null;
                                    currentRoot ??= "ROOT";

                                    sectionPair.Section = currentRoot;
                                    sectionPair.Key = keyPair[0];

                                    if (keyPair.Length > 1)
                                        keyValue = keyPair[1];

                                    _keyPairs.Add(sectionPair, keyValue);
                                }
                            }
                            strLine = iniFile.ReadLine();
                        }
                    }
                    finally
                    {
                        iniFile?.Close();
                    }
                else
                    throw new FileNotFoundException("Unable to locate " + _path);
            }
        }

        public string Read(string key, string section = null)
        {
            try
            {
                SectionPair sectionPair;
                sectionPair.Section = section;
                sectionPair.Key = key;

                return (string)_keyPairs[sectionPair];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        
        public void AddSetting(string sectionName, string settingName, string settingValue = null)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;

            if(_keyPairs.ContainsKey(sectionPair))
                _keyPairs.Remove(sectionPair);

            _keyPairs.Add(sectionPair, settingValue);
        }
        
        public void SaveSettings(string newFilePath)
        {
            var sections = new ArrayList();
            var strToSave = "";

            foreach (SectionPair sectionPair in _keyPairs.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }
            
            foreach (string section in sections)
            {
                strToSave += ("[" + section + "]\r\n");
                foreach (SectionPair sectionPair in _keyPairs.Keys)
                {
                    if (sectionPair.Section != section) continue;
                    var tmpValue = (string) _keyPairs[sectionPair];

                    if (tmpValue != null)
                        tmpValue = "=" + tmpValue;

                    strToSave += (sectionPair.Key + tmpValue + "\r\n");
                }
                strToSave += "\r\n";
            }

            TextWriter tw = new StreamWriter(newFilePath);
            tw.Write(strToSave);
            tw.Close();
        }

        public void SaveSettings()
        {
            SaveSettings(Path);
        }

        public bool KeyExists(string key, string section = null)
        {
            return Read(key, section).Length > 0;
        }
    }
}