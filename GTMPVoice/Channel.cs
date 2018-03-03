using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTMPVoice
{
    public class Channel : TeamSpeakBase
    {
        public ulong ID;
        public Connection Connection;
        public List<ushort> Clients = new List<ushort>();
        public ushort[] AllClients => Clients.ToArray();

        public Channel(Connection connection, ulong id, bool waitForProperties = true)
        {
            ID = id;
            Connection = connection;
            if (waitForProperties)
                UpdateClientList();
        }

        public void UpdateClientList()
        {
            IntPtr result = IntPtr.Zero;
            if (Check(Functions.getChannelClientList(Connection.ID, ID, ref result)))
                Clients = ReadShortIDList(result, (u) => u);
        }

        internal void UnmuteAll()
        {
            Connection.DoUnmuteList(AllClients);
        }

        internal void RemoveClient(ushort id)
        {
            Clients.Remove(id);
        }
    }
}
