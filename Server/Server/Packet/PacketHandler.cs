using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
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
}
