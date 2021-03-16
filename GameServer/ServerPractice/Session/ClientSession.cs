using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{

    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"onConnected:{endPoint}");
            Program.Room.Push(() => Program.Room.Enter(this));
            
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room != null)
            {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }
            Console.WriteLine($"onDisconnected:{endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes:{numOfBytes}");
        }
    }
}
