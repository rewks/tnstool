namespace TnsTool.Utils {
    internal static class TnsStrings {
        internal static string BuildString(string cmd, string cmdArgs, string service, string version) {
            string arguments = cmdArgs == "" ? cmdArgs : $"(VALUE={cmdArgs})";
            string tnsString = $"(DESCRIPTION=(CONNECT_DATA=(CID=(PROGRAM=)(HOST=localhost)(USER=oracle))(COMMAND={cmd})(ARGUMENTS=64)(SERVICE={service})(VERSION={version}){arguments}))";
            return tnsString;
        }
    }
}
