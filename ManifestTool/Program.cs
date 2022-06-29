using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using CommandLine;

namespace ManifestTool
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed(Run)
                .WithNotParsed(errs => Console.WriteLine(Options.GetUsage(parserResult, errs)));
        }

        private static void Run(Options options)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(options.Xml);
            }
            catch
            {
                doc.InnerXml = $"<updater><version>{options.Version}</version><patches></patches></updater>";
            }

            var node = doc.GetElementsByTagName("patches")[0];
            if (CheckVersionExists(doc, options.Version))
            {
                Console.WriteLine("A patch with this version already exists! Should it be overriden? [y/n]");
                var response = Console.ReadKey(false).Key;
                if (response == ConsoleKey.N) return;
                // search for all patches with the same version key as provided and remove them from the patch list
                doc.GetElementsByTagName("patch").Cast<XmlNode>().ToList()
                    .Where(patch => patch.SelectSingleNode("version").InnerText == options.Version).ToList()
                    .ForEach(patch => node.RemoveChild(patch));
            }

            if (!string.IsNullOrEmpty(options.DependsOnVersion) && !CheckVersionExists(doc, options.DependsOnVersion) && node.HasChildNodes)
            {
                Console.WriteLine("The patch, on which this patch depends, doesn't exist!");
                return;
            }
            node.AppendChild(GeneratePatchNode(doc, options));
            doc.Save(options.XmlSave ?? options.Xml);
            Console.WriteLine("XML Saved to file: " + (options.XmlSave ?? options.Xml));
        }

        private static bool CheckVersionExists(XmlDocument doc, string version)
        {
            return doc.SelectNodes("/updater/patches/patch/version")?.Cast<XmlNode>().Any(ver => ver.InnerText == version) ?? false;
        }

        private static XmlNode GeneratePatchNode(XmlDocument doc, Options options)
        {
            var node = doc.CreateNode(XmlNodeType.Element, "patch", "");
            var version = doc.CreateNode(XmlNodeType.Element, "version", "");
            version.InnerText = options.Version;
            if (!string.IsNullOrEmpty(options.DependsOnVersion))
            {
                var dependsOn = doc.CreateNode(XmlNodeType.Element, "dependsOn", "");
                dependsOn.InnerText = options.DependsOnVersion;
                node.AppendChild(dependsOn);
            }
            node.AppendChild(version);

            var files = doc.CreateNode(XmlNodeType.Element, "files", "");
            GenerateFileList(doc, options).ForEach(file => files.AppendChild(file));
            node.AppendChild(files);
            return node;
        }

        private static List<XmlNode> GenerateFileList(XmlDocument doc, Options options)
        {
            var nodeList = new List<XmlNode>();
            var files = new List<string>();
            var attr = File.GetAttributes(options.File);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                files.AddRange(Directory.GetFiles(options.File, "*.*", SearchOption.AllDirectories));
            else
                files.Add(options.File);

            var zipAttribute = false;
            if (options.Zip.HasValue && options.Zip.Value && (attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var zipFileName = new DirectoryInfo(options.File).Name + ".zip";
                if((options.GenerateMd5.HasValue && options.GenerateMd5.Value) || (options.GenerateSha1.HasValue && options.GenerateSha1.Value))
                    GenerateChecksumFile(options.File, options, zipFileName);
                ZipFile.CreateFromDirectory(options.File, zipFileName);
                files.Clear();
                files.Add(zipFileName);
                zipAttribute = true;
            }
            
            files.ForEach(file =>
            {
                Console.WriteLine($"Adding file to patch: {file}");
                if (file.StartsWith("./"))
                    file = file.Remove(0, 2);

                var node = doc.CreateNode(XmlNodeType.Element, "file", "");
                if (zipAttribute)
                {
                    XmlAttribute attribute = doc.CreateAttribute("zip");
                    attribute.InnerText = "true";
                    node.Attributes.Append(attribute);
                }

                var url = doc.CreateNode(XmlNodeType.Element, "url", "");
                url.InnerText = Path.Combine(options.Url, file);
                var location = doc.CreateNode(XmlNodeType.Element, "location", "");
                location.InnerText = file;

                if ((options.GenerateMd5.HasValue && options.GenerateMd5.Value) || (options.GenerateSha1.HasValue && options.GenerateSha1.Value))
                {
                    var checksum = doc.CreateNode(XmlNodeType.Element, "checksum", "");
                    if ((options.GenerateMd5.HasValue && options.GenerateMd5.Value))
                    {
                        var md5 = doc.CreateNode(XmlNodeType.Element, "md5", "");
                        md5.InnerText = GetMd5FromFile(file);
                        checksum.AppendChild(md5);
                    }

                    if ((options.GenerateSha1.HasValue && options.GenerateSha1.Value))
                    {
                        var sha1 = doc.CreateNode(XmlNodeType.Element, "sha1", "");
                        sha1.InnerText = GetSha1FromFile(file);
                        checksum.AppendChild(sha1);
                    }
                    node.AppendChild(checksum);
                }

                node.AppendChild(url);
                node.AppendChild(location);
                
                nodeList.Add(node);
            });

            return nodeList;
        }

        private static string GetMd5FromFile(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
        
        private static string GetSha1FromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var sha = new SHA1Managed();
            var checksum = sha.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void GenerateChecksumFile(string directoryPath, Options options, string zipFileName)
        {
            var doc = new XmlDocument {InnerXml = "<checksums><url></url><version></version><zipFilename></zipFilename><files></files></checksums>"};
            doc.GetElementsByTagName("url")[0].InnerText = Path.Combine(options.Url, zipFileName);
            doc.GetElementsByTagName("version")[0].InnerText = options.Version;
            doc.GetElementsByTagName("zipFilename")[0].InnerText = zipFileName;
            var files = doc.GetElementsByTagName("files")[0];
            foreach (var file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            {
                if(file.EndsWith(".checksum.xml")) continue;
                var node = doc.CreateNode(XmlNodeType.Element, "file", "");
                var filename = doc.CreateNode(XmlNodeType.Element, "filename", "");
                filename.InnerText = Path.GetRelativePath(directoryPath, file);
                node.AppendChild(filename);
                if (options.GenerateMd5.HasValue && options.GenerateMd5.Value)
                {
                    var md5 = doc.CreateNode(XmlNodeType.Element, "md5", "");
                    md5.InnerText = GetMd5FromFile(file);
                    node.AppendChild(md5);
                }
                if (options.GenerateSha1.HasValue && options.GenerateSha1.Value)
                {
                    var sha1 = doc.CreateNode(XmlNodeType.Element, "sha1", "");
                    sha1.InnerText = GetSha1FromFile(file);
                    node.AppendChild(sha1);
                }

                files.AppendChild(node);
            }
            doc.Save(Path.Combine(directoryPath, new DirectoryInfo(directoryPath).Name + ".checksum.xml"));
        }
    }
}