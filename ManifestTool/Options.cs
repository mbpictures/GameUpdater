using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace ManifestTool
{
    public class Options
    {
        [Option('f', "file", HelpText = "File or Folder to add to patch", Required = true)]
        public string File { get; set; }

        [Option('t', "target", HelpText = "客户端文件夹名字", Required = false, Default = "")]
        public string TargetFolder { get; set; }

        [Option('v', "version", HelpText = "Version of the patch", Required = true)]
        public string Version { get; set; }
        
        [Option('d', "depends-on", HelpText = "Version on which this patch depends on", Required = false)]
        public string DependsOnVersion { get; set; }
        
        [Option('u', "url", HelpText = "URL of the top level folder which contains all files relative to it", Required = true)]
        public string Url { get; set; }
        
        [Option('x', "xml", HelpText = "path to current server manifest xml file", Default = "update.xml")]
        public string Xml { get; set; }
        
        [Option('s', "save-location", HelpText = "Path to the generated server manifest xml file (source xml file will be overriden, if not provided)", Required = false)]
        public string XmlSave { get; set; }
        
        [Option("md5", HelpText = "Should a md5 hash be created to provide checksum validating", Required = false, Default = true)]
        public bool? GenerateMd5 { get; set; }
        
        [Option("sha1", HelpText = "Should a sha1 hash be created to provide checksum validating", Required = false, Default = true)]
        public bool? GenerateSha1 { get; set; }
        
        [Option("zip", HelpText = "Zip all files inside a single patch file and create and create a file, which contains all checksums (if enabled)", Required = false, Default = false)]
        public bool? Zip { get; set; }

        public static string GetUsage<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var sentenceBuilder = SentenceBuilder.Create();
            var output = "";

            var errorList = errors.ToList();
            if (errorList.ToArray().Length > 0 && !(errorList.ToList().Count == 1 && errorList.ToList()[0].Tag == ErrorType.HelpRequestedError))
            {
                output += "Errors:\n";
                output = errorList.Aggregate(output, (current, error) => current + $"{sentenceBuilder.FormatError(error)}\n");
                output += "Usage: \n\n";
            }

            output += HelpText.AutoBuild(result, h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "ManifestTool";
                    h.Copyright = "Copyright (c) 2020 Marius Butz";
                    h.AddDashesToOption = true;
                    return h;
                },
                e => e,
                verbsIndex: true).ToString(); //set verbsIndex to display verb help summary.

            return output;
        }
    }
}