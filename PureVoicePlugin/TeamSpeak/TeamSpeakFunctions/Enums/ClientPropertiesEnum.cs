using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSpeakPlugin
{
    public enum ClientProperty
    {
        CLIENT_UNIQUE_IDENTIFIER = 0,   //automatically up-to-date for any client "in view", can be used
                                        //to identify this particular client installation
        CLIENT_NICKNAME,                //automatically up-to-date for any client "in view"
        CLIENT_VERSION,                 //for other clients than ourself, this needs to be requested
                                        //(=> requestClientVariables)
        CLIENT_PLATFORM,                //for other clients than ourself, this needs to be requested
                                        //(=> requestClientVariables)
        CLIENT_FLAG_TALKING,            //automatically up-to-date for any client that can be heard
                                        //(in room / whisper)
        CLIENT_INPUT_MUTED,             //automatically up-to-date for any client "in view", this clients
                                        //microphone mute status
        CLIENT_OUTPUT_MUTED,            //automatically up-to-date for any client "in view", this clients
                                        //headphones/speakers mute status
        CLIENT_OUTPUTONLY_MUTED,         //automatically up-to-date for any client "in view", this clients
                                  //headphones/speakers only mute status
        CLIENT_INPUT_HARDWARE,          //automatically up-to-date for any client "in view", this clients
                                  //microphone hardware status (is the capture device opened?)
        CLIENT_OUTPUT_HARDWARE,         //automatically up-to-date for any client "in view", this clients
                                        //headphone/speakers hardware status (is the playback device opened?)
        CLIENT_INPUT_DEACTIVATED,       //only usable for ourself, not propagated to the network
        CLIENT_IDLE_TIME,               //internal use
        CLIENT_DEFAULT_CHANNEL,         //only usable for ourself, the default channel we used to connect
                                        //on our last connection attempt
        CLIENT_DEFAULT_CHANNEL_PASSWORD,//internal use
        CLIENT_SERVER_PASSWORD,         //internal use
        CLIENT_META_DATA,               //automatically up-to-date for any client "in view", not used by
                                        //TeamSpeak, free storage for sdk users
        CLIENT_IS_MUTED,                //only make sense on the client side locally, "1" if this client is
                                        //currently muted by us, "0" if he is not
        CLIENT_IS_RECORDING,            //automatically up-to-date for any client "in view"
        CLIENT_VOLUME_MODIFICATOR,      //internal use
        CLIENT_VERSION_SIGN,              //internal use
        CLIENT_SECURITY_HASH,           //SDK only: Hash is provided by an outside source. A channel will
                                        //use the security salt + other client data to calculate a hash,
                                        //which must be the same as the one provided here.
        CLIENT_ENDMARKER,
    };

	public enum ChannelProperty
	{
		// Token: 0x04000120 RID: 288
		CHANNEL_NAME,
		// Token: 0x04000121 RID: 289
		CHANNEL_TOPIC,
		// Token: 0x04000122 RID: 290
		CHANNEL_DESCRIPTION,
		// Token: 0x04000123 RID: 291
		CHANNEL_PASSWORD,
		// Token: 0x04000124 RID: 292
		CHANNEL_CODEC,
		// Token: 0x04000125 RID: 293
		CHANNEL_CODEC_QUALITY,
		// Token: 0x04000126 RID: 294
		CHANNEL_MAXCLIENTS,
		// Token: 0x04000127 RID: 295
		CHANNEL_MAXFAMILYCLIENTS,
		// Token: 0x04000128 RID: 296
		CHANNEL_ORDER,
		// Token: 0x04000129 RID: 297
		CHANNEL_FLAG_PERMANENT,
		// Token: 0x0400012A RID: 298
		CHANNEL_FLAG_SEMI_PERMANENT,
		// Token: 0x0400012B RID: 299
		CHANNEL_FLAG_DEFAULT,
		// Token: 0x0400012C RID: 300
		CHANNEL_FLAG_PASSWORD,
		// Token: 0x0400012D RID: 301
		CHANNEL_CODEC_LATENCY_FACTOR,
		// Token: 0x0400012E RID: 302
		CHANNEL_CODEC_IS_UNENCRYPTED,
		// Token: 0x0400012F RID: 303
		CHANNEL_SECURITY_SALT,
		// Token: 0x04000130 RID: 304
		CHANNEL_DELETE_DELAY,
		// Token: 0x04000131 RID: 305
		CHANNEL_ENDMARKER,
		// Token: 0x04000132 RID: 306
		CHANNEL_DUMMY_2 = 17,
		// Token: 0x04000133 RID: 307
		CHANNEL_DUMMY_3,
		// Token: 0x04000134 RID: 308
		CHANNEL_DUMMY_4,
		// Token: 0x04000135 RID: 309
		CHANNEL_DUMMY_5,
		// Token: 0x04000136 RID: 310
		CHANNEL_DUMMY_6,
		// Token: 0x04000137 RID: 311
		CHANNEL_DUMMY_7,
		// Token: 0x04000138 RID: 312
		CHANNEL_FLAG_MAXCLIENTS_UNLIMITED,
		// Token: 0x04000139 RID: 313
		CHANNEL_FLAG_MAXFAMILYCLIENTS_UNLIMITED,
		// Token: 0x0400013A RID: 314
		CHANNEL_FLAG_MAXFAMILYCLIENTS_INHERITED,
		// Token: 0x0400013B RID: 315
		CHANNEL_FLAG_ARE_SUBSCRIBED,
		// Token: 0x0400013C RID: 316
		CHANNEL_FILEPATH,
		// Token: 0x0400013D RID: 317
		CHANNEL_NEEDED_TALK_POWER,
		// Token: 0x0400013E RID: 318
		CHANNEL_FORCED_SILENCE,
		// Token: 0x0400013F RID: 319
		CHANNEL_NAME_PHONETIC,
		// Token: 0x04000140 RID: 320
		CHANNEL_ICON_ID,
		// Token: 0x04000141 RID: 321
		CHANNEL_FLAG_PRIVATE,
		// Token: 0x04000142 RID: 322
		CHANNEL_ENDMARKER_RARE
	};
}
