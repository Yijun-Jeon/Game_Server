using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // Protocol Id, 특정 행동
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    // PacketHandler 대상 함수
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    // 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업 -> 자동화 대상
    // 멀티쓰레드가 개입되기 전에 가장 먼저 실행해 주어야 함
    public void Register()
    {
      _onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
        _handler.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler); // 대상 함수는 직접 작성


    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        // 패킷에서 정보 추출 
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    // Packet을 만들고 handler를 호출해 주는 작업
    void MakePacket<T>(PacketSession session,ArraySegment<byte> buffer) where T : IPacket,new()
    {
        T pkt = new T();
        pkt.Read(buffer);
        Action<PacketSession, IPacket> action = null;
        // PacketHandler 대상 함수 _handler에서 pakcet에 맞는 Protocol을 찾은 뒤 해당 action 추출
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}