protoc.exe -I=./ --csharp_out=./ Protocol.proto



REM START ../../PakcetGenerator/bin/PacketGenerator.exe ../../PakcetGenerator/PDL.xml
REM XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
REM XCOPY /Y GenPackets.cs "../../DayDreamSlay/DayDreamSlay/Assets/Scripts/Server/Packet"
REM XCOPY /Y GenPackets.cs "../../ServerPractice/Packet"
REM XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
REM XCOPY /Y ClientPacketManager.cs "../../DayDreamSlay/DayDreamSlay/Assets/Scripts/Server/Packet"
REM XCOPY /Y ServerPacketManager.cs "../../ServerPractice/Packet"