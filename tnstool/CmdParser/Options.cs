using CommandLine;

namespace TnsTool.CmdParser {
    internal class Options {
        [Option(shortName: 'c', longName: "command", Required = true, HelpText = "Command to run (e.g. ping, version, status, services)")]
        public string Cmd { get; set; } = string.Empty;

        [Option(shortName: 'a', longName: "args", Required = false, HelpText = "Arguments (for commands which require them, like log_file)")]
        public string Args { get; set; } = string.Empty;

        [Option(shortName: 'd', longName: "debug", Default = false, Required = false, HelpText = "Enable debug messages")]
        public bool Debug { get; set; }     
    }
}
