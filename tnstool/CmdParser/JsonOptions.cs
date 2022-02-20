using CommandLine;

namespace TnsTool.CmdParser {
    [Verb("json", HelpText = "Read targets from JSON file (previously output by tnstool)")]
    internal class JsonOptions : Options {
        [Option(shortName: 'f', longName: "file", Required = true, HelpText = "JSON file")]
        public string InFile { get; set; } = string.Empty;
    }
}