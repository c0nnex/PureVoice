

Alt.on('LIPSYNC', (player, animDict, AnimName) => {
    //PlayFacialAnimation(args[0], args[1], args[2]); if player is streamed in
});

Alt.on("PUREVOICE", (VoiceServerIP, VoiceServerPort, VoiceServerSecret, playerIdentifier, VoiceServerPluginVersion, VoiceClientPort) => {
    let url = 'http://localhost:' + VoiceClientPort + '/CONNECT?SERVER=' + VoiceServerIP + '&PORT=' + VoiceServerPort + '&SECRET=' + VoiceServerSecret + '&CLIENTGUID=' + playerIdentifier +
        '&VERSION=' + VoiceServerPluginVersion;
    // Call CEF every 10 seconds

});

