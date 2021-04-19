using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
		
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CDash, MakePacket<C_Dash>);
		_handler.Add((ushort)MsgId.CDash, PacketHandler.C_DashHandler);		
		_onRecv.Add((ushort)MsgId.CScale, MakePacket<C_Scale>);
		_handler.Add((ushort)MsgId.CScale, PacketHandler.C_ScaleHandler);		
		_onRecv.Add((ushort)MsgId.CRoom, MakePacket<C_Room>);
		_handler.Add((ushort)MsgId.CRoom, PacketHandler.C_RoomHandler);		
		_onRecv.Add((ushort)MsgId.CCreateRoom, MakePacket<C_CreateRoom>);
		_handler.Add((ushort)MsgId.CCreateRoom, PacketHandler.C_CreateRoomHandler);		
		_onRecv.Add((ushort)MsgId.CSendMapData, MakePacket<C_SendMapData>);
		_handler.Add((ushort)MsgId.CSendMapData, PacketHandler.C_SendMapDataHandler);		
		_onRecv.Add((ushort)MsgId.CJoinRoom, MakePacket<C_JoinRoom>);
		_handler.Add((ushort)MsgId.CJoinRoom, PacketHandler.C_JoinRoomHandler);		
		_onRecv.Add((ushort)MsgId.CLeaveRoom, MakePacket<C_LeaveRoom>);
		_handler.Add((ushort)MsgId.CLeaveRoom, PacketHandler.C_LeaveRoomHandler);		
		_onRecv.Add((ushort)MsgId.CMobSpawn, MakePacket<C_MobSpawn>);
		_handler.Add((ushort)MsgId.CMobSpawn, PacketHandler.C_MobSpawnHandler);		
		_onRecv.Add((ushort)MsgId.CMobDespawn, MakePacket<C_MobDespawn>);
		_handler.Add((ushort)MsgId.CMobDespawn, PacketHandler.C_MobDespawnHandler);		
		_onRecv.Add((ushort)MsgId.CMobMove, MakePacket<C_MobMove>);
		_handler.Add((ushort)MsgId.CMobMove, PacketHandler.C_MobMoveHandler);		
		_onRecv.Add((ushort)MsgId.CMobAtk, MakePacket<C_MobAtk>);
		_handler.Add((ushort)MsgId.CMobAtk, PacketHandler.C_MobAtkHandler);		
		_onRecv.Add((ushort)MsgId.CShop, MakePacket<C_Shop>);
		_handler.Add((ushort)MsgId.CShop, PacketHandler.C_ShopHandler);		
		_onRecv.Add((ushort)MsgId.CBossPattern, MakePacket<C_BossPattern>);
		_handler.Add((ushort)MsgId.CBossPattern, PacketHandler.C_BossPatternHandler);		
		_onRecv.Add((ushort)MsgId.CBossMobMove, MakePacket<C_BossMobMove>);
		_handler.Add((ushort)MsgId.CBossMobMove, PacketHandler.C_BossMobMoveHandler);		
		_onRecv.Add((ushort)MsgId.CMobHit, MakePacket<C_MobHit>);
		_handler.Add((ushort)MsgId.CMobHit, PacketHandler.C_MobHitHandler);		
		_onRecv.Add((ushort)MsgId.CHit, MakePacket<C_Hit>);
		_handler.Add((ushort)MsgId.CHit, PacketHandler.C_HitHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}