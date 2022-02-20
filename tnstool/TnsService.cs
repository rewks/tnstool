using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using TnsTool.Utils;

namespace TnsTool {
    public class TnsService {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Version { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public bool IsSecure { get; set; }
        public bool IsDead { get; set; } = true;
        public Dictionary<string, string> ResponseData { get; set; } = new();

        internal string SendToTarget(byte[] packet, Logger logger) {
            try {
                var client = new TcpClient();
                if (!client.ConnectAsync(this.Host, this.Port).Wait(1000)) {
                    logger.LogError("Failed to connect!");
                    this.IsDead = true;
                    return "";
                }

                NetworkStream ns = client.GetStream();
                ns.ReadTimeout = 5000;
                ns.Write(packet, 0, packet.Length);

                byte[] response = ReadResponse(ns, logger);

                ns.Close();
                client.Close();

                logger.LogDebug($"Raw response: {BitConverter.ToString(response)}");
                if (response.Length > 0) return Encoding.ASCII.GetString(response)[2..^2];  // Return data but trim start and end of data markers
            } catch (Exception e) {
                this.IsDead = true;
                logger.LogError($"Exception occurred during connection: {e}");
            }

            return "";
        }

        internal byte[] ReadResponse(NetworkStream ns, Logger logger) {
            if (ns.CanRead) {
                TNS_HEADER header = ReadHeader(ns, logger, false);
                byte[] content = new byte[header.PacketLength - 8];
                ns.Read(content, 0, content.Length);

                if (header.PacketType == 2) { // ACCEPT connection packet
                    TNS_ACCEPT_PACKET acceptPacket = new TNS_ACCEPT_PACKET(content);                 
                    logger.LogDebug($"Connection accepted, server version: {acceptPacket.TNSVersion}");
                    logger.LogDebug($"Acceptance TNS string: {acceptPacket.AcceptData}");
                    if (Regex.IsMatch(acceptPacket.AcceptData, "VSNNUM=\\d{9}")) this.Version = Regex.Match(acceptPacket.AcceptData, "VSNNUM=\\d{9}").ToString()[7..];

                    return ReadDataPackets(ns, logger); // Continue reading DATA packets until end of data marker [0x00, 0x40] is received

                } else if (header.PacketType == 4) { // REFUSE connection packet, also used when command is 'ping'
                    TNS_REFUSE_PACKET refusePacket = new TNS_REFUSE_PACKET(content);                
                    logger.LogDebug($"Connected refused, refusal code (user): {refusePacket.ReasonUser}. Refusal code (system): {refusePacket.ReasonSystem}");
                    logger.LogDebug($"Refusal TNS string: {refusePacket.RefuseData}");
                    if (Regex.IsMatch(refusePacket.RefuseData, "VSNNUM=\\d{9}")) this.Version = Regex.Match(refusePacket.RefuseData, "VSNNUM=\\d{9}").ToString()[7..];

                    PrintRefusal(refusePacket.RefuseData, logger);
                }
            }

            return Array.Empty<byte>();
        }

        internal TNS_HEADER ReadHeader(NetworkStream ns, Logger logger, bool dataPacket) {
            byte[] header = new byte[8]; // packets always have 8 byte header

            ns.Read(header, 0, header.Length);
            TNS_HEADER packetHeader = new TNS_HEADER(header, this.Version, dataPacket);

            logger.LogDebug($"Header: {BitConverter.ToString(header)}");
            logger.LogDebug($"Packet Len: {packetHeader.PacketLength}");
            logger.LogDebug($"Packet Type: {packetHeader.PacketType}");

            return packetHeader;
        }

        internal byte[] ReadDataPackets(NetworkStream ns, Logger logger) {
            if (ns.CanRead) {
                try {
                    // receiving data from 19c (and maybe others) is fucked, connection hangs or closes before receiving all data packets. can force connection to stay open
                    // by sending excess packets but its unreliable and ugly. 8i, 9i, 10g, 11g seem fine. Others untested.
                    //     while (!ns.DataAvailable) { Console.WriteLine("Data not yet available on NetworkStream, waiting.."); ns.WriteByte(0x00); };
                    //     Console.WriteLine("Can read");
                    TNS_HEADER header = ReadHeader(ns, logger, true);

                    if (header.PacketType != 6) throw new Exception($"Expected data packet but received packet of type {header.PacketType}!");
                    byte[] data = new byte[header.PacketLength - 8];

                    ns.Read(data, 0, data.Length);

                    if (!(data[^2] == 0x00 && data[^1] == 0x40)) {
                        logger.LogDebug("Data not complete yet, calling ReadDataPackets recursively");
                        data = data.Concat(ReadDataPackets(ns, logger)).ToArray();
                    }
                    //logger.LogDebug($"Data: {BitConverter.ToString(data)}");
                    return data;
                } catch (Exception e) {
                    logger.LogError($"Exception occured while reading data packets: {e}");
                }
            }

            return Array.Empty<byte>();
        }

        // Prints reason for connection refusal, this also includes PING responses
        internal void PrintRefusal(string TnsString, Logger logger) {
            int errorCode = 0;

            if (Regex.IsMatch(TnsString, "ERR=\\d{1,5}")) {
                errorCode = Convert.ToInt32((Regex.Match(TnsString, "ERR=\\d{1,5}").ToString())[4..]);
            } else if (Regex.IsMatch(TnsString, "ERROR_STACK=\\(ERROR=\\(CODE=\\d{1,5}")) {
                errorCode = Convert.ToInt32((Regex.Match(TnsString, "ERROR_STACK=\\(ERROR=\\(CODE=\\d{1,5}").ToString())[25..]);
            }

            if (errorCode == 1189) this.IsSecure = true;

            if (errorCode > 0) {
                bool hasArg = Regex.IsMatch(TnsString, "ARGS='.*'");
                if (hasArg) {
                    string arg = (Regex.Match(TnsString, "ARGS='.*'").ToString())[5..];
                    logger.LogError($"ERROR {errorCode}: {TnsError.GetError(errorCode)} {arg}");
                } else {
                    logger.LogError($"ERROR {errorCode}: {TnsError.GetError(errorCode)}");
                }
            } else if (Regex.IsMatch(TnsString, "ALIAS=")) { // PING response
                string aliasesString = Regex.Match(TnsString, "ALIAS=[0-z,]+").ToString();
                this.Aliases.Clear();
                foreach (string alias in aliasesString[6..].Split(',')) this.Aliases.Add(alias);
                logger.Log($"Ping response: {aliasesString}");
            }
        }

    }
}
