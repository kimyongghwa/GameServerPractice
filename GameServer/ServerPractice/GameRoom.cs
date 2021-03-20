using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);
            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
                //foreach (ClientSession s in _sessions)
                //    s.Send(segment);
        }

        public void Enter(ClientSession session)
        {
            //플레이어 추가
                _sessions.Add(session);
                session.Room = this;
            //새로 들어온 플레이어에게 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach(ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                }) ;
            }
            session.Send(players.Write());
            //새로 플레이어가 들어왔다는 것을 모든 플레이어에게 알림
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }
        public void Leave(ClientSession session)
        {
                _sessions.Remove(session);

            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }
        public void Move(ClientSession session, C_Move packet)
        {
            //좌표수정
            session.PosX = packet.posY;
            session.PosX = packet.posZ;
            session.PosX = packet.posX;
            //모든 플레이어에게 전달
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}
