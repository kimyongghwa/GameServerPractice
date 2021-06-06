using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player
	{
		public int chNum;
		public PlayerInfo Info { get; set; } = new PlayerInfo() {PosInfo = new PositionInfo() };
		public GameRoom Room { get; set; }
		public ClientSession Session { get; set; }
	}
	public class Monster
    {
		public MonsterInfo Info { get; set; } = new MonsterInfo() { PosInfo = new PositionInfo() };
		public GameRoom Room { get; set; }
		public ClientSession Session { get; set; }
	}
}
