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
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

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
		ClientSession clientSession = session as ClientSession;
		S_Room roomPacket = new S_Room();
		for (int i = 1; i < RoomManager.Instance._roomId; i++)
		{
			RoomInfo roomInfo = new RoomInfo();
			roomInfo.PlayerNumber = RoomManager.Instance.Find(i).PlayerNumber;
			roomInfo.RoomId = i;
			roomInfo.Name = "Test Room";
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
		roomSuccessPacket.Player = clientSession.MyPlayer.Info;
		clientSession.Send(roomSuccessPacket);
    }

	public static void C_SendMapDataHandler(PacketSession session, IMessage packet)
    {
		Console.WriteLine("SMDH!");
		ClientSession clientSession = session as ClientSession;
		C_SendMapData sendMapPacket = packet as C_SendMapData;
		RoomManager.Instance.Find(sendMapPacket.RoomId).MData = sendMapPacket.MapSaves;
		if(clientSession.MyPlayer.Room != RoomManager.Instance.Find(sendMapPacket.RoomId))
			RoomManager.Instance.Find(sendMapPacket.RoomId).EnterRoom(clientSession.MyPlayer);
	}

	public static void C_JoinRoomHandler(PacketSession session, IMessage packet)
    {
		ClientSession clientSession = session as ClientSession;
		C_JoinRoom joinPacket = packet as C_JoinRoom;
		Console.WriteLine("Joinroom " + joinPacket.RoomId);
		//TODO 비밀번호 체크
		RoomManager.Instance.Find(joinPacket.RoomId).EnterRoom(clientSession.MyPlayer);
		S_EnterRoom enterRoomPacket = new S_EnterRoom();
		enterRoomPacket.Player = clientSession.MyPlayer.Info;
		clientSession.Send(enterRoomPacket);
	}
	public static void C_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
		ClientSession clientSession = session as ClientSession;
		clientSession.MyPlayer.Room.LeaveRoom(clientSession.MyPlayer.Info.PlayerId);
	}
}