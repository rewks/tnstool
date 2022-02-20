namespace TnsTool.Utils {
    internal class Logger {
        private string Host { get; set; }
        private int Port { get; set; }
        private string Service { get; set; }
        private bool PrintDebug { get; set; }

        internal Logger(string hostname, int port, string service, bool debug) {
            this.Host = hostname;
            this.Port = port;
            this.Service = service;
            this.PrintDebug = debug;
        }

        internal void Log(string message) {
            Console.WriteLine($"[{this.Host}:{this.Port}-{this.Service}] {message}");
        }

        internal void LogError(string message) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[{this.Host}:{this.Port}-{this.Service}] {message}");
            Console.ResetColor();
        }
        internal void LogDebug(string message) {
            if (this.PrintDebug) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[{this.Host}:{this.Port}-{this.Service}] {message}");
                Console.ResetColor();
            }
        }
    }
}