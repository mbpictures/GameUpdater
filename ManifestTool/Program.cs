using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using CommandLine;

namespace ManifestTool
{
    class Program
    {
        private static void Main(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed(options => Run(options))
                .WithNotParsed(errs => Console.WriteLine(Options.GetUsage(parserResult)));
        }

        public static void Run(Options options)
        {
            var doc = new XmlDocument();
            doc.Load(options.Xml);
            var node = doc.GetElementsByTagName("patches")[0];
            node.AppendChild(GeneratePatchNode(doc, options));
            doc.Save(options.XmlSave ?? options.Xml);
            Console.WriteLine("XML Saved to file: " + (options.XmlSave ?? options.Xml));
        }

        public static XmlNode GeneratePatchNode(XmlDocument doc, Options options)
        {
            var node = doc.CreateNode(XmlNodeType.Element, "patch", "");
            var version = doc.CreateNode(XmlNodeType.Element, "version", "");
            version.InnerText = options.Version;
            var dependsOn = doc.CreateNode(XmlNodeType.Element, "dependsOn", "");
            dependsOn.InnerText = options.DependsOnVersion;
            node.AppendChild(version);
            node.AppendChild(dependsOn);

            var files = doc.CreateNode(XmlNodeType.Element, "files", "");
            GenerateFileList(doc, options).ForEach(file => files.AppendChild(file));
            node.AppendChild(files);
            return node;
        }

        public static List<XmlNode> GenerateFileList(XmlDocument doc, Options options)
        {
            var nodeList = new List<XmlNode>();
            var files = new List<string>();
            var attr = File.GetAttributes(options.File);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                files.AddRange(Directory.GetFiles(options.File, "*.*", SearchOption.AllDirectories));
            else
                files.Add(options.File);
            
            files.ForEach(file =>
            {
                Console.WriteLine($"Adding file to patch: {file}");
                if (file.StartsWith("./"))
                    file = file.Remove(0, 2);

                var node = doc.CreateNode(XmlNodeType.Element, "file", "");
                var url = doc.CreateNode(XmlNodeType.Element, "url", "");
                url.InnerText = $"{options.Url}/{file}";
                var location = doc.CreateNode(XmlNodeType.Element, "location", "");
                location.InnerText = file;
                
                var checksum = doc.CreateNode(XmlNodeType.Element, "checksum", "");
                var md5 = doc.CreateNode(XmlNodeType.Element, "md5", "");
                md5.InnerText = GetMd5FromFile(file);
                var sha1 = doc.CreateNode(XmlNodeType.Element, "sha1", "");
                sha1.InnerText = GetSha1FromFile(file);
                checksum.AppendChild(md5);
                checksum.AppendChild(sha1);

                node.AppendChild(url);
                node.AppendChild(location);
                node.AppendChild(checksum);
                
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
    }
}