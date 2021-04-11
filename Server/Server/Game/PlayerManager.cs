using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class PlayerManager
	{
		public static PlayerManager Instance { get; } = new PlayerManager();

		object _lock = new object();
		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
		public int _playerId = 1; // TODO
		public int _mobId = 1; // TODO
		public Player Add()
		{
			Player player = new Player();

			lock (_lock)
			{
				player.Info.PlayerId = _playerId;
				_players.Add(_playerId, player);
				_playerId++;
			}

			return player;
		}

		public bool Remove(int playerId)
		{
			lock (_lock)
			{
				return _players.Remove(playerId);
			}
		}

		public Player Find(int playerId)
		{
			lock (_lock)
			{
				Player player = null;
				if (_players.TryGetValue(playerId, out player))
					return player;

				return null;
			}
		}

		public Monster AddMob()
		{
			Monster monster = new Monster();

			lock (_lock)
			{
				monster.Info.MonsterId = _mobId;
				_monsters.Add(_mobId, monster);
				_mobId++;
			}
			//Console.WriteLine("MobIDBeforeCreate : " + _mobId);
			return monster;
		}

		public bool RemoveMob(int mobId)
		{
			lock (_lock)
			{
				return _monsters.Remove(mobId);
			}
		}

		public Monster FindMob(int mobId)
		{
			lock (_lock)
			{
				Monster monster = null;
				if (_monsters.TryGetValue(mobId, out monster))
					return monster;

				return null;
			}
		}
	}
}
