import * as alt from "alt-client";
import * as game from "natives";


var purevoiceBrowser = null;

alt.onServer("LIPSYNC", (ped, isOn) => {
    if (ped != null && ped.valid) {
        let animDict = "mp_facial";
        let animName = "mic_chatter";
        if (!isOn) {
            animDict = "facials@gen_male@variations@normal";
            animName = "mood_normal_1";
        }
        if (!game.hasAnimDictLoaded(animDict))
		{
            game.requestAnimDict(animDict);
			return;
		}
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
