using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        // output 파일에 저장될 내용 
        static string genPackets;
        static ushort packetId;
        static string packetEnums;

        static string clientRegister;
        static string serverRegister;

        static void Main(string[] args)
        {
            // default 경로 값 
            string pdlPath = "../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            if (args.Length >= 1)
                pdlPath = args[0];

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                // 내용이 있는 부분으로 이동 
                r.MoveToContent();

                // 태그 하나씩 읽어나감 - close 태그도 포함 
                while (r.Read())
                {
                    // 패킷 태그의 시작부분을 만남 
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }
                // {0} 패킷 이름/번호 목록 {1} 패킷 목록 
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);

                // 서버
                // {0} 패킷 등록 
                string serverManagerText = string.Format(PacketFormat.packetManagerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);

                // 클
                // {0} 패킷 등록 
                string clientManagerText = string.Format(PacketFormat.packetManagerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
            }
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }
            // 정상적인 패킷이므로 패킷을 파싱 
            Tuple<string, string, string> t = ParseMembers(r);

            // 패킷 
            genPackets += string.Format(PacketFormat.packetFormat,
                packetName, t.Item1, t.Item2, t.Item3);

            // 패킷 enum
            // {0} 패킷 이름 {1} 패킷 번호 
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId);
            packetEnums += Environment.NewLine + "\t";

            if (packetName.StartsWith("C_") || packetName.StartsWith("c_"))
            {
                // 클라 -> 서버 패킷 Register
                serverRegister += string.Format(PacketFormat.registerFormat, packetName);
                serverRegister += Environment.NewLine;
            }
            else if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
            {
                // 서버 -> 클라 패킷 Register
                clientRegister += string.Format(PacketFormat.registerFormat, packetName);
                clientRegister += Environment.NewLine;
            }
            
        }

        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write 
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // 파싱 대상들의 depth 
            int depth = r.Depth + 1;
            while (r.Read())
            {
                // 패킷의 끝 부분을 만남 
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        // {0} 변수 형식 {1} 변수 이름 
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0} 변수 이름 {1} 변수 형식 
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        // {0} 변수 이름 {1} 변수 형식 
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        // {0} 변수 형식 {1} 변수 이름 
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0} 변수 이름 {1} To~ 변수 형식 {2} 변수 형식
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        // {0} 변수 이름 {1} 변수 형식
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        // {0} 변수 형식 {1} 변수 이름 
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0} 변수 이름
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        // {0} 변수 이름 
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string,string,string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            // 탭을 맞춰줌 
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        // To ~변수 형식으로 바꿔주는 메소드 
        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static Tuple<string,string,string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            // [1] 멤버 변수들 [2] 멤버 변수 Read [3] 멤버 변수 Write 
            Tuple<string, string, string> t = ParseMembers(r);

            // {0} 리스트 이름 [대문자] {1} 리스트 이름 [소문자] {2} 멤버 변수들 {3} 멤버 변수 Read {4} 멤버 변수 Write
            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName), FirstCharToLower(listName), t.Item1, t.Item2, t.Item3);

            // {0} 리스트 이름 [대문자] {1} 리스트 이름 [소문자]
            string readCode = string.Format(PacketFormat.readListFormat,
                 FirstCharToUpper(listName), FirstCharToLower(listName));

            // {0} 리스트 이름 [대문자] {1} 리스트 이름 [소문자]
            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        // 첫 글자를 대문자로 변환
        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }
        // 첫 글자를 문자로 변환
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}

