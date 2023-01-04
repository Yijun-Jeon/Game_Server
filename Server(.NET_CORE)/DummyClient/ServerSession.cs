﻿using System;
using ServerCore;
using System.Net;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);

    }
    // 플레이어 정보 요청 패킷  
    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);

            // 범위 초과 값은 자동으로 에러 처리 
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string 길이 헤더 
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            // string 데이터
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // skill list
            skills.Clear();
            // skill이 몇 개인지의 헤더
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            // 실제 skill Data
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            // 공간이 모자르면 실패
            // Count -> 쓸 수 있는 공간
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // 실제 string Data
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            // string이 몇 byte인지 헤더로 지정
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // Skill list
            // skill이 몇 개인지 헤더로 지정
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            // 실제 skill Data
            foreach(SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref count);
            }


            // 패킷의 size는 모든 작업이 끝난 후 넣어줘야 제대로 측정이 가능
            success &= BitConverter.TryWriteBytes(s, count);


            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    // 패킷의 분류를 위한 ID
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "Yijun" };
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 301, level = 3, duration = 5.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 401, level = 4, duration = 6.0f });

            // 보낸다
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write();
                if(s != null)
                    Send(s);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}

