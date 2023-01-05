using System;
namespace PacketGenerator
{
	public class PacketFormat
	{
        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write 
		public static string packetFormat =
@"
// 플레이어 정보 요청 패킷  
class {0}
{{
    {1}

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
}}
";
        // {0} 변수 형식
        // {1} 변수 이름 
        public static string memberFormat =
@"public {0} {1};
";
        // {0} 변수 이름
        // {1} To~ 변수 형식
        // {2} 변수 형식 
        public static string readFormat =
@"
// 범위 초과 값은 자동으로 에러 처리 
this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});
";
        // {0} 변수 이름
        public static string readStringFormat =
@"
// string 길이 헤더 
ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
// string 데이터
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;
";
        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});
";
        // {0} 변수 이름
        public static string writeStringFormat =
@"
// 실제 string Data
ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
// string이 몇 byte인지 헤더로 지정
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;
";
    }
}

