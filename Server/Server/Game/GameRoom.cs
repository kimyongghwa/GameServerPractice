﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class GameRoom
	{
		object _lock = new object();
		public int RoomId { get; set; }
		public string RoomName { get; set; }
		public string Password { get; set; }
		public MapSaveData MData { get; set; }
		List<Player> _players = new List<Player>();
		List<Monster> _monsters = new List<Monster>();
		public int PlayerNumber
        {
            get
            {
				return _players.Count;
            }
        }

        public List<Player> _Players { get => _players; set => _players = value; }

        public void EnterRoom(Player newPlayer)
		{
			if (newPlayer == null)
				return;
			Console.WriteLine(newPlayer.Info.Name + "  join  " + RoomId);
			lock (_lock)
			{
				_players.Add(newPlayer);
				newPlayer.Room = this;

				// 본인한테 정보 전송
				{
					//S_EnterRoom enterPacket = new S_EnterRoom();
					//enterPacket.Player = newPlayer.Info;
					//newPlayer.Session.Send(enterPacket);

					S_Spawn spawnPacket = new S_Spawn();
					foreach (Player p in _players)
					{
						if (newPlayer != p)
							spawnPacket.Players.Add(p.Info);
					}
					newPlayer.Session.Send(spawnPacket);
				}

				// 타인한테 정보 전송
				{
					S_Spawn spawnPacket = new S_Spawn();
					spawnPacket.Players.Add(newPlayer.Info);
					foreach (Player p in _players)
					{
						if (newPlayer != p)
							p.Session.Send(spawnPacket);
					}
				}
			}
		}
		public void EnterGame(Player newPlayer) //로비로 접속
		{
			if (newPlayer == null)
				return;

			lock (_lock)
			{
				_players.Add(newPlayer);
				newPlayer.Room = this;
			}
		}
		public void EnterMob(Monster newMonster)
		{
			if(newMonster == null)
				return;
			lock (_lock)
			{
				_monsters.Add(newMonster);
				newMonster.Room = this;
				// 타인한테 정보 전송
				{
					S_MobSpawn spawnMobPacket = new S_MobSpawn();
					spawnMobPacket.Mobs.Add(newMonster.Info);
					foreach (Player p in _players)
					{
						if (newMonster.Session != p.Session)
							p.Session.Send(spawnMobPacket);
					}
				}
			}
		}
		public void LeaveGame(int playerId)
		{
			lock (_lock)
			{
				Player player = _players.Find(p => p.Info.PlayerId == playerId);
				if (player == null)
					return;

				_players.Remove(player);
				player.Room = null;
				if (RoomId != 0)
				{
					// 본인한테 정보 전송
					{
						S_LeaveGame leavePacket = new S_LeaveGame();
						player.Session.Send(leavePacket);
					}

					// 타인한테 정보 전송
					{
						S_Despawn despawnPacket = new S_Despawn();
						despawnPacket.PlayerIds.Add(player.Info.PlayerId);
						foreach (Player p in _players)
						{
							if (player != p)
								p.Session.Send(despawnPacket);
						}
					}
				}
			}
		}
		public void LeaveRoom(int playerId)
		{
			lock (_lock)
			{
				Player player = _players.Find(p => p.Info.PlayerId == playerId);
				if (player == null)
					return;

				_players.Remove(player);
				RoomManager.Instance.Find(0).EnterGame(player); //로비로 접속
				if (RoomId != 0)
				{
					// 본인한테 정보 전송
					{
						S_LeaveGame leavePacket = new S_LeaveGame();
						player.Session.Send(leavePacket);
					}

					// 타인한테 정보 전송
					{
						S_Despawn despawnPacket = new S_Despawn();
						despawnPacket.PlayerIds.Add(player.Info.PlayerId);
						foreach (Player p in _players)
						{
							if (player != p)
								p.Session.Send(despawnPacket);
						}
					}
				}
			}
		}
		public void Broadcast(IMessage packet)
        {
			lock(_lock)
            {
				foreach(Player p in _players)
                {
					p.Session.Send(packet);
                }
            }
        }
	}
}
