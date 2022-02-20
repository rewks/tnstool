namespace TnsTool.Utils {
    internal static class TnsStrings {
        internal static string BuildString(string cmd, string cmdArgs, string service, string version) {
            string arguments = cmdArgs == "" ? cmdArgs : $"(VALUE={cmdArgs})";
            string tnsString = version switch {
                "8i" => $"(CONNECT_DATA=(COMMAND={cmd})(ARGUMENTS=64)(SERVICE={service}){arguments})",
                "10g" => $"(CONNECT_DATA=(CID=(PROGRAM=)(HOST=localhost)(USER=oracle))(COMMAND={cmd})(ARGUMENTS=64)(SERVICE={service})(VERSION=169869568){arguments})",
                "19c" => $"(DESCRIPTION=(CONNECT_DATA=(CID=(PROGRAM=)(HOST=localhost)(USER=oracle))(COMMAND={cmd})(ARGUMENTS=64)(SERVICE={service})(VERSION=318767104){arguments}))",
                _ => $"(CONNECT_DATA=(CID=(PROGRAM=)(HOST=localhost)(USER=oracle))(COMMAND={cmd})(ARGUMENTS=64)(SERVICE={service})(VERSION=169869568){arguments})", // Default to 10g format
            };
            return tnsString;
        }
    }
}
