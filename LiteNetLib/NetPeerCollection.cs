using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LiteNetLib
{
    internal sealed class NetPeerCollection
    {
        private readonly ConcurrentDictionary<NetEndPoint, NetPeer> _peersDict;
        public int Count => _peersDict.Count;


        public NetPeerCollection(int maxPeers)
        {
            _peersDict = new ConcurrentDictionary<NetEndPoint, NetPeer>();
        }

        public bool TryGetValue(NetEndPoint endPoint, out NetPeer peer)
        {
            return _peersDict.TryGetValue(endPoint, out peer);
        }

        public void Clear()
        {
            _peersDict.Clear();
        }

        public List<NetPeer> All => _peersDict.Values.ToList();

        public void Add(NetEndPoint endPoint, NetPeer peer)
        {
            _peersDict[endPoint] = peer;
        }

        public bool ContainsAddress(NetEndPoint endPoint)
        {
            return _peersDict.Values.Any(p => p.EndPoint == endPoint);
        }

        public NetPeer[] ToArray()
        {
            return _peersDict.Values.ToArray();
        }

        public void Remove(NetEndPoint peerEndPoint)
        {
            _peersDict.TryRemove(peerEndPoint, out var _);
        }

    }
}
