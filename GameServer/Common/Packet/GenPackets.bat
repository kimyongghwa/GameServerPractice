START ../../PakcetGenerator/bin/PacketGenerator.exe ../../PakcetGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../DayDreamSlay/DayDreamSlay/Assets/Scripts/Server/Packet"
XCOPY /Y GenPackets.cs "../../ServerPractice/Packet"
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ClientPacketManager.cs "../../DayDreamSlay/DayDreamSlay/Assets/Scripts/Server/Packet"
XCOPY /Y ServerPacketManager.cs "../../ServerPractice/Packet"