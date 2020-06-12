
1. Copy PureVoiceServerCore.* to your alt:V Server resources directory of your gamemode and add it as Dependency to your gamemode
2. Copy "purevoice" directory to resources of your alt:V Server
3. include "purevoice" as resource in server.cfg 
4. add VoiceController.cs to your game mode 
5. Alter the function GetSetting to match your gamemode
6. Create a VoiceContrller Instance in your gamemode somewhere and call VoiceController.Start() whenever suitable.
5. (players/Users) need to install the ts3-Plugin ( PureVoice_*.ts3_plugin )

The VoiceController will emit the following events
(subscribe to them with Alt.On() )

Alt.On("PUREVOICE_CLIENT_DISCONNECTED", (player) => {} );
A player disconnected from Teamspeak.

Alt.On("PUREVOICE_CLIENT_SPEAKERSTATUS", (player, areSpeakersMuted) => {} );
A player muted/unmuted their speakers

Alt.On("PUREVOICE_CLIENT_MICROPHONESTATUS", (player, isMicMuted) => {} );
A player muted/unmuted their microphone

Alt.On("PUREVOICE_CLIENT_CONNECTED", (player, isMicMuted, areSpeakersMuted) => {} );
A player connected to purevoice

Alt.On("PUREVOICE_CLIENT_NOVOICE", (player) => {} );
A player did not conect to purevoice within 1 Minute. (Probably plugin not installed or outdated)




