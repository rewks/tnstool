using CommandLine;

namespace TnsTool.CmdParser {
    [Verb("cmd", isDefault: true, HelpText = "Specify target(s) on command line or in plain text file")]
    internal class CmdOptions : Options {
        [Option(shortName: 'h', longName: "host", Required = true, HelpText = "Hostname/IP or file containing list of targets. Format <address>[:<port>]")]
        public string Target { get; set; } = string.Empty;

        [Option(shortName: 'v', longName: "version", Default = "8", Required = false, HelpText = "Version of Oracle db [8, 10g, 11g, 12c, 18c, 19c, 21c]")]
        public string Version { get; set; } = string.Empty;

        [Option(shortName: 's', longName: "service", Default = "LISTENER", Required = false, HelpText = "TNS service name (can be enumerated with 'ping' command)")]
        public string Service { get; set; } = string.Empty;

        [Option(shortName: 'o', longName: "outfile", Required = false, HelpText = "Write output to file (JSON)")]
        public string OutFile { get; set; } = string.Empty;
    }
}