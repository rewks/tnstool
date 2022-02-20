using CommandLine;
using System.Diagnostics;
using TnsTool.CmdParser;
using TnsTool.Utils;
using System.Text.Json;

namespace TnsTool {
    public class Program {
        private static List<TnsService> targetList = new();
        private static bool debug;
        private static string cmd = string.Empty;
        private static string cmdArgs = string.Empty;
        private static string jsonFile = string.Empty;

        internal static void AddToList(string target, string version, string service) {
            TnsService tnsService = new TnsService();
            if (target.Contains(':')) {
                tnsService.Host = target[..target.IndexOf(':')];
                try { tnsService.Port = Int32.Parse(target[target.IndexOf(':')..]); } catch { tnsService.Port = 1521; }
            } else {
                tnsService.Host = target;
                tnsService.Port = 1521;
            }
            tnsService.Version = version;
            tnsService.Aliases = new List<string> { service };
            tnsService.IsSecure = false;
            tnsService.IsDead = false;
            targetList.Add(tnsService);
        }

        public static void Main(string[] args) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Parser.Default.ParseArguments<CmdOptions, JsonOptions>(args)
                   .WithParsed<CmdOptions>(options => {
                       if (File.Exists(options.Target)) {
                           foreach (string line in File.ReadLines(options.Target)) {
                               AddToList(line, options.Version, options.Service);                         
                           }
                       } else {
                           AddToList(options.Target, options.Version, options.Service);                           
                       }
                       debug = options.Debug;
                       cmd = options.Cmd;
                       cmdArgs = options.Args;
                       jsonFile = options.OutFile;
                   })
                   .WithParsed<JsonOptions>(options => {
                       if (File.Exists(options.InFile)) {
                           string jsonString = File.ReadAllText(options.InFile);
                           targetList = JsonSerializer.Deserialize<List<TnsService>>(jsonString)!;
                       } else {
                           Console.WriteLine($"{options.InFile} does not exist!");
                       }
                       debug = options.Debug;
                       cmd = options.Cmd;
                       cmdArgs = options.Args;
                       jsonFile = options.InFile;
                   });

            if (targetList.Count <= 0) {
                Console.WriteLine("No valid targets specified!");
                System.Environment.Exit(1);
            }

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
            Parallel.ForEach(targetList, options, h => {
                if (!h.IsSecure && !h.IsDead) {
                    Logger logger = new Logger(h.Host, h.Port, h.Aliases[0], debug);
                    string connString = TnsStrings.BuildString(cmd, cmdArgs, h.Aliases[0], h.Version);
                    byte[] connPacket = TnsPackets.BuildPacket(connString, h.Version);

                    string respData = h.SendToTarget(connPacket, logger);

                    if (respData != "") {
                        logger.Log(respData);
                        string respKey = cmdArgs == "" ? cmd : $"{cmd} {cmdArgs}";
                        if (h.ResponseData.ContainsKey(respKey)) {
                            h.ResponseData[respKey] = respData;
                        } else {
                            h.ResponseData.Add(respKey, respData);
                        }
                    }
                }
            });

            if (jsonFile != "") {
                string jsonData = JsonSerializer.Serialize(targetList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFile, jsonData);
            }

            stopwatch.Stop();
            Console.WriteLine($"Elapsed time is {stopwatch.ElapsedMilliseconds} ms. {targetList.Count} hosts processed.");
        }

    }
}