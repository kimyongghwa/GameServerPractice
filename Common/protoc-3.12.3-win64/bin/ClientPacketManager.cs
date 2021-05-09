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
		_onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.SDash, MakePacket<S_Dash>);
		_handler.Add((ushort)MsgId.SDash, PacketHandler.S_DashHandler);		
		_onRecv.Add((ushort)MsgId.SScale, MakePacket<S_Scale>);
		_handler.Add((ushort)MsgId.SScale, PacketHandler.S_ScaleHandler);		
		_onRecv.Add((ushort)MsgId.SRoom, MakePacket<S_Room>);
		_handler.Add((ushort)MsgId.SRoom, PacketHandler.S_RoomHandler);		
		_onRecv.Add((ushort)MsgId.SRoomCreateSuccess, MakePacket<S_RoomCreateSuccess>);
		_handler.Add((ushort)MsgId.SRoomCreateSuccess, PacketHandler.S_RoomCreateSuccessHandler);		
		_onRecv.Add((ushort)MsgId.SEnterRoom, MakePacket<S_EnterRoom>);
		_handler.Add((ushort)MsgId.SEnterRoom, PacketHandler.S_EnterRoomHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveRoom, MakePacket<S_LeaveRoom>);
		_handler.Add((ushort)MsgId.SLeaveRoom, PacketHandler.S_LeaveRoomHandler);		
		_onRecv.Add((ushort)MsgId.SMapSaveDataSend, MakePacket<S_MapSaveDataSend>);
		_handler.Add((ushort)MsgId.SMapSaveDataSend, PacketHandler.S_MapSaveDataSendHandler);		
		_onRecv.Add((ushort)MsgId.SMobSpawn, MakePacket<S_MobSpawn>);
		_handler.Add((ushort)MsgId.SMobSpawn, PacketHandler.S_MobSpawnHandler);		
		_onRecv.Add((ushort)MsgId.SMobDespawn, MakePacket<S_MobDespawn>);
		_handler.Add((ushort)MsgId.SMobDespawn, PacketHandler.S_MobDespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMobMove, MakePacket<S_MobMove>);
		_handler.Add((ushort)MsgId.SMobMove, PacketHandler.S_MobMoveHandler);		
		_onRecv.Add((ushort)MsgId.SMobAtk, MakePacket<S_MobAtk>);
		_handler.Add((ushort)MsgId.SMobAtk, PacketHandler.S_MobAtkHandler);		
		_onRecv.Add((ushort)MsgId.SShop, MakePacket<S_Shop>);
		_handler.Add((ushort)MsgId.SShop, PacketHandler.S_ShopHandler);		
		_onRecv.Add((ushort)MsgId.SBossPattern, MakePacket<S_BossPattern>);
		_handler.Add((ushort)MsgId.SBossPattern, PacketHandler.S_BossPatternHandler);		
		_onRecv.Add((ushort)MsgId.SBossMobMove, MakePacket<S_BossMobMove>);
		_handler.Add((ushort)MsgId.SBossMobMove, PacketHandler.S_BossMobMoveHandler);		
		_onRecv.Add((ushort)MsgId.SMobHit, MakePacket<S_MobHit>);
		_handler.Add((ushort)MsgId.SMobHit, PacketHandler.S_MobHitHandler);		
		_onRecv.Add((ushort)MsgId.SHit, MakePacket<S_Hit>);
		_handler.Add((ushort)MsgId.SHit, PacketHandler.S_HitHandler);		
		_onRecv.Add((ushort)MsgId.SWeafonChange, MakePacket<S_WeafonChange>);
		_handler.Add((ushort)MsgId.SWeafonChange, PacketHandler.S_WeafonChangeHandler);		
		_onRecv.Add((ushort)MsgId.SMoneySet, MakePacket<S_MoneySet>);
		_handler.Add((ushort)MsgId.SMoneySet, PacketHandler.S_MoneySetHandler);
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