using System.Buffers.Binary;
using System.Text;

namespace TnsTool.Utils {

    public struct TNS_HEADER {
        public int PacketLength;
        public byte[] PacketChksum = new byte[2];
        public int PacketType;
        public int Reserved;
        public byte[] HeaderChksum = new byte[2];

        public TNS_HEADER(byte[] data, string version, bool isDataPacket) {
            if (isDataPacket && version == "19c") {
                this.PacketLength = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(data, 0));
            } else {
                this.PacketLength = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 0));
                this.PacketChksum = data[2..3];
            }
            this.PacketType = data[4];
            this.Reserved = data[5];
            this.HeaderChksum = data[6..];
        }

    }

    public struct TNS_ACCEPT_PACKET {
        public int TNSVersion { get; private set; }
        public int ServiceOptions { get; private set; }
        public int SDU_size { get; private set; }
        public int MTDU_size { get; private set; }
        public int Hardware_1 { get; private set; }
        public int DataLength { get; private set; }
        public int DataOffset { get; private set; }
        public int ConnectFlag0 { get; private set; }
        public int ConnectFlag1 { get; private set; }
        public byte[] Misc = new byte[8];
        public string AcceptData { get; private set; }

        public TNS_ACCEPT_PACKET(byte[] data) {
            this.TNSVersion = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 0));
            this.ServiceOptions = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 2));
            this.SDU_size = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 4));
            this.MTDU_size = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 6));
            this.Hardware_1 = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 8));
            this.DataLength = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 10));
            this.DataOffset = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 12)) - 8; // Offset included header length but data passed into function doesnt include header, so -8
            this.ConnectFlag0 = data[14];
            this.ConnectFlag1 = data[15];
            Array.Copy(data, 16, this.Misc, 0, 8);
            this.AcceptData = data.Length == this.DataOffset + this.DataLength ? Encoding.ASCII.GetString(data, this.DataOffset, this.DataLength) : "";
        }
    }

    public struct TNS_REFUSE_PACKET {
        public int ReasonUser { get; set; }
        public int ReasonSystem { get; set; }
        public int DataLength { get; set; }
        public string RefuseData { get; set; }

        public TNS_REFUSE_PACKET(byte[] data) {
            this.ReasonUser = data[0];
            this.ReasonSystem = data[1];
            this.DataLength = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(data, 2));
            this.RefuseData = Encoding.ASCII.GetString(data, 4, this.DataLength);
        }
    }

    public static class TnsPackets {
        internal static byte[] BuildPacket(string message, string version) {
            byte[] packet = Array.Empty<byte>();
            switch (version) {
                case "10g":
                    packet = new byte[] {
                        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                        0x01, 0x36, 0x01, 0x2c, 0x00, 0x00, 0x08, 0x00,
                        0x7f, 0xff, 0x7f, 0x08, 0x00, 0x00, 0x00, 0x01,
                        0x00, 0x00, 0x00, 0x3a, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x34, 0xe6, 0x00, 0x00,
                        0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00 };
                    break;
                case "19c":
                    packet = new byte[] {
                        0x00, 0xe4, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                        0x01, 0x3e, 0x01, 0x2c, 0x00, 0x81, 0x20, 0x00,
                        0xff, 0xff, 0x7f, 0x08, 0x00, 0x00, 0x01, 0x00,
                        0x00, 0x9a, 0x00, 0x4a, 0x00, 0x00, 0x10, 0x00, // last 4 bytes in this line specify maximum packet size that client can receive. (2048 = 000007f8)
                        0x0c, 0x0c, 0x6d, 0xe0, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x20,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00 };
                    break;
                default:
                    packet = new byte[] {
                        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                        0x01, 0x36, 0x01, 0x2c, 0x00, 0x00, 0x08, 0x00,
                        0x7f, 0xff, 0x7f, 0x08, 0x00, 0x00, 0x00, 0x01,
                        0x00, 0x00, 0x00, 0x3a, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x34, 0xe6, 0x00, 0x00,
                        0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00 }; // default to 10g for max compatibility
                    break;
            }

            int cLen = message.Length;
            int cLenH = cLen >> 8;
            int cLenL = cLen & 0xff;

            int pLen = cLen + packet.Length;
            int pLenH = pLen >> 8;
            int pLenL = pLen & 0xff;

            packet[0] = (byte)pLenH;
            packet[1] = (byte)pLenL;
            packet[24] = (byte)cLenH;
            packet[25] = (byte)cLenL;

            int i = packet.Length;
            Array.Resize<byte>(ref packet, i + cLen);
            Encoding.ASCII.GetBytes(message).CopyTo(packet, i);

            return packet;
        }
    }

}
