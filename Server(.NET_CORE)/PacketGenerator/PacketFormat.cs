using System;
namespace PacketGenerator
{
	public class PacketFormat
	{
        // {0} 패킷 이름/번호 목록 
        // {1} 패킷 목록 
        public static string fileFormat =
@"using System;
using ServerCore;
using System.Net;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

// 패킷의 분류를 위한 ID
public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}
{1}
";
        // {0} 패킷 이름
        // {1} 패킷 번호 
        public static string packetEnumFormat =
@"{0} = {1},";

        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write 
		public static string packetFormat =
@"
// 플레이어 정보 요청 패킷  
class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        // 공간이 모자르면 실패
        // Count -> 쓸 수 있는 공간
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}

        // 패킷의 size는 모든 작업이 끝난 후 넣어줘야 제대로 측정이 가능
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}";
        // {0} 변수 형식
        // {1} 변수 이름 
        public static string memberFormat =
@"public {0} {1};";
        // {0} 변수 이름
        // {1} To~ 변수 형식
        // {2} 변수 형식 
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";
        // {0} 변수 이름
        public static string readStringFormat =
@"
// string 길이 헤더 
ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
// string 데이터
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";
        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";
        // {0} 변수 이름
        public static string writeStringFormat =
@"
// 실제 string Data
ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
// string이 몇 byte인지 헤더로 지정
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}

        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"
// {1} list
this.{1}s.Clear();
// {1}이 몇 개인지의 헤더
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
// 실제 {1} Data
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"
// {0} list
// {1}이 몇 개인지 헤더로 지정
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);
// 실제 {1} Data
foreach({0} {1} in {1}s)
{{
    success &= {1}.Write(s, ref count);
}}";
        // {0} 변수 이름
        // {1} 변수 형식 
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});
";
        // {0} 변수 이름
        // {1} 변수 형식 
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});
";
        // {0} 패킷 등록  
        public static string packetManagerFormat =
@"using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance{{ get {{ return _instance; }} }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    // Protocol Id, 특정 행동
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    // PacketHandler 대상 함수
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    // 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업 -> 자동화 대상
    // 멀티쓰레드가 개입되기 전에 가장 먼저 실행해 주어야 함
    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        // 패킷에서 정보 추출 
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }}

    // Packet을 만들고 handler를 호출해 주는 작업
    void MakePacket<T>(PacketSession session,ArraySegment<byte> buffer) where T : IPacket,new()
    {{
        T pkt = new T();
        pkt.Read(buffer);
        Action<PacketSession, IPacket> action = null;
        // PacketHandler 대상 함수 _handler에서 pakcet에 맞는 Protocol을 찾은 뒤 해당 action 추출
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}
}}";
        // {0} 패킷 이름 
        public static string registerFormat =
@"      _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler); // 대상 함수는 직접 작성
";

    }
}

