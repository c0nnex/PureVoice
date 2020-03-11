import * as alt from "alt-client";
import * as game from "natives";


var purevoiceBrowser = null;

alt.onServer("LIPSYNC", (ped, animDict, animName) => {
    if (ped != null && ped.valid) {
        while (!game.hasAnimDictLoaded(animDict))
            game.requestAnimDict(animDict);
        game.playFacialAnim(ped.scriptID, animName, animDict);
    }
});

alt.onServer("PUREVOICE", (VoiceServerIP, VoiceServerPort, VoiceServerSecret, playerIdentifier, VoiceServerPluginVersion, VoiceClientPort) => {
    alt.log("PureVoice Connect");
    let url = 'http://localhost:' + VoiceClientPort + '/CONNECT?SERVER=' + VoiceServerIP + '&PORT=' + VoiceServerPort + '&SECRET=' + VoiceServerSecret + '&CLIENTGUID=' + playerIdentifier +
        '&VERSION=' + VoiceServerPluginVersion;
    if (purevoiceBrowser == null || !purevoiceBrowser.valid) {
        // Load as overlay so it will not influence game rendering
        purevoiceBrowser = new alt.WebView(url, true);
        // no need to show this.
        purevoiceBrowser.isVisible = false;
        waitUntilCefBrowserLoaded(purevoiceBrowser);

    }
    alt.setInterval(() => {
        // Make browser refresh every 5 seconds. Ensure chache is not used
        purevoiceBrowser.url = url + "&REF=" + game.getGameTimer();
    }, 5000);
});

export async function waitUntilCefBrowserLoaded(browser) {
    if ((browser.valid))
        return;
    while (!browser.valid) {
        alt.log("Waiting for Browser");
        await sleep(500);
    }
}
