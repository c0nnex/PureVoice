using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSpeakPlugin
{
    public enum ConnectionStatusEnum
    {
        STATUS_DISCONNECTED = 0,       //There is no activity to the server, this is the default value
        STATUS_CONNECTING,             //We are trying to connect, we haven't got a clientID yet, we
                                       //haven't been accepted by the server
        STATUS_CONNECTED,              //The server has accepted us, we can talk and hear and we got a
                                       //clientID, but we don't have the channels and clients yet, we
                                       //can get server infos (welcome msg etc.)
        STATUS_CONNECTION_ESTABLISHING,//we are CONNECTED and we are visible
        STATUS_CONNECTION_ESTABLISHED, //we are CONNECTED and we have the client and channels available
    }
}
