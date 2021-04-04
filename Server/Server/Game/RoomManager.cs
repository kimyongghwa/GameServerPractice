using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class RoomManager
	{
		public static RoomManager Instance { get; } = new RoomManager();

		object _lock = new object();
		Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
		public int _roomId = 0;

		public GameRoom Add()
		{
			GameRoom gameRoom = new GameRoom();

			lock (_lock)
			{
				gameRoom.RoomId = _roomId;
				gameRoom.MData = new Google.Protobuf.Protocol.MapSaveData();
				_rooms.Add(_roomId, gameRoom);
				_roomId++;
			}
			
			return gameRoom;
		}

		public bool Remove(int roomId)
		{
			lock (_lock)
			{
				return _rooms.Remove(roomId);
			}
		}

		public GameRoom Find(int roomId)
		{
			lock (_lock)
			{
				GameRoom room = null;
				if (_rooms.TryGetValue(roomId, out room))
					return room;

				return null;
			}
		}
	}
}
