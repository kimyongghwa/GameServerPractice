using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static readonly int mapCounter = 3;

	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		//Console.WriteLine($"C_Move({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		if (clientSession.MyPlayer == null)
			return;
		if (clientSession.MyPlayer.Room == null)
			return;

		//TODO : 검증
		//서버에서 좌표 이동
		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PosInfo = movePacket.PosInfo;

		//다른 플레이어한테도 알려준다
		S_Move resMovePacket = new S_Move();
		resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		resMovePacket.PosInfo = movePacket.PosInfo;

		clientSession.MyPlayer.Room.Broadcast(resMovePacket);
	}

	public static void C_DashHandler(PacketSession session, IMessage packet)
	{
		C_Dash movePacket = packet as C_Dash;
		ClientSession clientSession = session as ClientSession;
		Console.WriteLine($"C_Dash({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");
		if (clientSession.MyPlayer == null)
			return;
		if (clientSession.MyPlayer.Room == null)
			return;

		//TODO : 검증

		//서버에서 좌표 이동
		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PosInfo = movePacket.PosInfo;

		//다른 플레이어한테도 알려준다
		S_Dash resMovePacket = new S_Dash();
		resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		resMovePacket.PosInfo = movePacket.PosInfo;
		clientSession.MyPlayer.Room.Broadcast(resMovePacket);
	}

	public static void C_ScaleHandler(PacketSession session, IMessage packet)
	{
		C_Scale scalePacket = packet as C_Scale;
		ClientSession clientSession = session as ClientSession;

		if (clientSession.MyPlayer == null)
			return;
		if (clientSession.MyPlayer.Room == null)
			return;
		Console.WriteLine($"ScaleSet : {scalePacket.Scale}");
		S_Scale resScalePacket = new S_Scale();
		resScalePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		resScalePacket.Scale = scalePacket.Scale;

		clientSession.MyPlayer.Room.Broadcast(resScalePacket);
	}

	public static void C_RoomHandler(PacketSession session, IMessage packet)
	{
		Console.WriteLine("Room Give Please");
		ClientSession clientSession = session as ClientSession;
		S_Room roomPacket = new S_Room();
		for (int i = 1; i < RoomManager.Instance._roomId; i++)
		{
			RoomInfo roomInfo = new RoomInfo();
			roomInfo.PlayerNumber = RoomManager.Instance.Find(i).PlayerNumber;
			roomInfo.RoomId = i;
			roomInfo.Name = RoomManager.Instance.Find(i).RoomName;
			roomInfo.Password = RoomManager.Instance.Find(i).Password;
			roomPacket.Room.Add(roomInfo);
		}
		clientSession.Send(roomPacket);
	}

	public static void C_CreateRoomHandler(PacketSession session, IMessage packet)
	{
		Console.WriteLine("CreateRoom!");
		ClientSession clientSession = session as ClientSession;
		C_CreateRoom createRoomPacket = packet as C_CreateRoom;
		GameRoom room = RoomManager.Instance.Add();
		room.RoomName = createRoomPacket.Name;
		room.Password = createRoomPacket.Password;
		S_RoomCreateSuccess roomSuccessPacket = new S_RoomCreateSuccess();
		roomSuccessPacket.Room = new RoomInfo();
		roomSuccessPacket.Room.Name = room.RoomName;
		roomSuccessPacket.Room.Password = room.Password;
		roomSuccessPacket.Room.RoomId = room.RoomId;
		roomSuccessPacket.Room.PlayerNumber = 0;
		roomSuccessPacket.Player = new PlayerInfo();
		clientSession.MyPlayer.Info.ChNum = createRoomPacket.ChNum;
		roomSuccessPacket.Player = clientSession.MyPlayer.Info;
		clientSession.Send(roomSuccessPacket);
	}

	public static void C_SendMapDataHandler(PacketSession session, IMessage packet)
	{
		Console.WriteLine("SMDH!");
		ClientSession clientSession = session as ClientSession;
		C_SendMapData sendMapPacket = packet as C_SendMapData;
		GameRoom gameRoom = RoomManager.Instance.Find(sendMapPacket.RoomId);
		gameRoom.MData.Map.Add(sendMapPacket.MapSave);
		if (gameRoom.MData.Map.Count >= mapCounter)
			gameRoom.isCreating = false;
		if (clientSession.MyPlayer.Room != RoomManager.Instance.Find(sendMapPacket.RoomId))
			RoomManager.Instance.Find(sendMapPacket.RoomId).EnterRoom(clientSession.MyPlayer);
	}

	public static void C_JoinRoomHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_JoinRoom joinPacket = packet as C_JoinRoom;
		Console.WriteLine("Joinroom " + joinPacket.RoomId);
		GameRoom gameRoom = RoomManager.Instance.Find(joinPacket.RoomId);
		if (gameRoom.isCreating)
			return;
		//TODO 비밀번호 체크
		gameRoom.EnterRoom(clientSession.MyPlayer);
		S_EnterRoom enterRoomPacket = new S_EnterRoom();
		clientSession.MyPlayer.Info.ChNum = joinPacket.ChNum;
		enterRoomPacket.Player = clientSession.MyPlayer.Info;
		clientSession.Send(enterRoomPacket);
		S_MapSaveDataSend mapSendPacket = new S_MapSaveDataSend();
		int index = 0;
		foreach (MapSave m in clientSession.MyPlayer.Room.MData.Map)
		{
			mapSendPacket.Map = m;
			mapSendPacket.Index = index;
			clientSession.Send(mapSendPacket);
			index++;
			Console.WriteLine("MapSend : " + m.MapCell.Count);
		}
		S_MobSpawn mobSpawnPacket = new S_MobSpawn();
		foreach (Monster m in clientSession.MyPlayer.Room._Monsters)
		{
			mobSpawnPacket.Mobs.Add(m.Info);
		}
		clientSession.Send(mobSpawnPacket);
	}
	public static void C_LeaveRoomHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		clientSession.MyPlayer.Room.LeaveRoom(clientSession.MyPlayer.Info.PlayerId);
	}

	public static void C_MobSpawnHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_MobSpawn monsterPacket = packet as C_MobSpawn;
		Monster monster = PlayerManager.Instance.AddMob();
		{
			int mobIdTmp = monster.Info.MonsterId;
			monster.Info = monsterPacket.Mob;
			monster.Info.MonsterId = mobIdTmp;
			monster.Session = clientSession;
			clientSession.MyPlayer.Room.EnterMob(monster, clientSession); //EnterMob 내에서 전송한다.
		}
		////나 말고 방안의 모든플레이어에게 전송
		//S_
		//mobSpawnPacket = new S_MobSpawn();
		//mobSpawnPacket.Mobs.Add(monster.Info);
		//mobSpawnPacket.IsMine = false;
		//Console.WriteLine($"mobid = {monster.Info.MonsterId}");
		//foreach (Player p in clientSession.MyPlayer.Room._Players)
		//{
		//	if (clientSession.MyPlayer !=9 p)
		//		p.Session.Send(mobSpawnPacket);
		//}
		////나에겐 내 몬스터 체크해서 전송
		//mobSpawnPacket.IsMine = true;
		//clientSession.Send(mobSpawnPacket);
	}
	public static void C_MobDespawnHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_MobDespawn mobDespawnPacket = packet as C_MobDespawn;
		S_MobDespawn sMobDespawnPacket = new S_MobDespawn();
		foreach (int id in mobDespawnPacket.MobIds)
		{
			sMobDespawnPacket.MobIds.Add(id);
		}
		clientSession.MyPlayer.Room.Broadcast(sMobDespawnPacket);
	}
	public static void C_MobMoveHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_MobMove monsterPacket = packet as C_MobMove;
		//서버에서 이동
		PlayerManager.Instance.FindMob(monsterPacket.MonsterId).Info.PosInfo.PosX = monsterPacket.PosInfo.PosX;
		PlayerManager.Instance.FindMob(monsterPacket.MonsterId).Info.PosInfo.PosY = monsterPacket.PosInfo.PosY;
		Console.WriteLine($"cmobmove{monsterPacket.PosInfo.PosX}, {monsterPacket.PosInfo.PosY}");
		//모두에게 전송
		S_MobMove sMonsterPacket = new S_MobMove();
		sMonsterPacket.MonsterId = monsterPacket.MonsterId;
		sMonsterPacket.PosInfo = monsterPacket.PosInfo;
		clientSession.MyPlayer.Room.Broadcast(sMonsterPacket);
	}
	public static void C_MobAtkHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_MobAtk monsterPacket = packet as C_MobAtk;
		S_MobAtk sMonsterPacket = new S_MobAtk();
		sMonsterPacket.MobId = monsterPacket.MobId;
		sMonsterPacket.GapVector = monsterPacket.GapVector;
		sMonsterPacket.IsBoss = monsterPacket.IsBoss;
		clientSession.MyPlayer.Room.Broadcast(sMonsterPacket);
	}
	public static void C_ShopHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_Shop shopPacket = packet as C_Shop;
		S_Shop sShopPacket = new S_Shop();
		sShopPacket.ShopPos = shopPacket.ShopPos;
		sShopPacket.ShopRank = shopPacket.ShopRank;
		clientSession.MyPlayer.Room.Broadcast(sShopPacket);
	}
	public static void C_BossPatternHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_BossPattern patternPacket = packet as C_BossPattern;
		S_BossPattern sPatternPacket = new S_BossPattern();
		sPatternPacket.BossId = patternPacket.BossId;
		sPatternPacket.BossPattern = patternPacket.BossPattern;
		clientSession.MyPlayer.Room.Broadcast(sPatternPacket);
	}
	public static void C_BossMobMoveHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_BossMobMove monsterPacket = packet as C_BossMobMove;
		//모두에게 전송
		S_BossMobMove sMonsterPacket = new S_BossMobMove();
		sMonsterPacket.BossId = monsterPacket.BossId;
		sMonsterPacket.PosInfo = monsterPacket.PosInfo;
		clientSession.MyPlayer.Room.Broadcast(sMonsterPacket);
	}
	public static void C_MobHitHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_MobHit monsterPacket = packet as C_MobHit;
		S_MobHit sMonsterPacket = new S_MobHit();
		sMonsterPacket.Damage = monsterPacket.Damage;
		sMonsterPacket.MobId = monsterPacket.MobId;
		sMonsterPacket.IsBoss = monsterPacket.IsBoss;
		clientSession.MyPlayer.Room.Broadcast(sMonsterPacket);
	}
	public static void C_HitHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_Hit hitPacket = packet as C_Hit;
		S_Hit sHitPacket = new S_Hit();
		sHitPacket.Damage = hitPacket.Damage;
		sHitPacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		clientSession.MyPlayer.Room.Broadcast(sHitPacket);
	}
	public static void C_WeafonChangeHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		C_WeafonChange weafonChagnePacket = packet as C_WeafonChange;
		S_WeafonChange sWeafonChangePacket = new S_WeafonChange();
		sWeafonChangePacket.WeafonNum = clientSession.MyPlayer.Info.WeafonNum = weafonChagnePacket.WeafonNum;
		sWeafonChangePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		clientSession.MyPlayer.Room.Broadcast(sWeafonChangePacket);
	}
	public static void C_WeafonChangeHandler(PacketSession session, IMessage packet)
	{
	}
}