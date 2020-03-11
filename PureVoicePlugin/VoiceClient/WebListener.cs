using PureVoice.VoiceClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PureVoice.VoiceClient
{
    internal class WebListener
    {
        private static AutoResetEvent webStopEvent = new AutoResetEvent(false);
        private static HttpListener _listener;
        private static string url = "http://localhost:4239/";
        public static event EventHandler<VoicePaketConfig> ConnectionRequestReceived;

        public static void StartListen()
        {
            var t = new Thread(HttpListenerThread)
            {
                IsBackground = true
            };
            t.Start();
        }

        public static void StopListen()
        {
            webStopEvent.Set();
        }

        private static void HttpListenerThread()
        {
            _listener = new HttpListener();
            try
            {
                VoicePlugin.Log($"Listening on {url}");

                _listener.Prefixes.Add(url);
                _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                _listener.Start();
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
                return;
            }
            IAsyncResult ar = _listener.BeginGetContext(ProcessRequest, null);
            webStopEvent.WaitOne();
        }

        private static void ProcessRequest(IAsyncResult ar)
        {
            var ctx = _listener.EndGetContext(ar);
            _listener.BeginGetContext(ProcessRequest, null);
            try
            {

                var request = ctx.Request;
                VoicePaketConfig cfg = new VoicePaketConfig();

                foreach (var item in ctx.Request.QueryString.AllKeys)
                {
                    var val = ctx.Request.QueryString[item];
                    switch (item.ToUpperInvariant())
                    {
                        case "SERVER":
                            cfg.ServerIP = val;
                            break;
                        case "PORT":
                            if (!int.TryParse(val, out var tInt))
                                return;
                            cfg.ServerPort = tInt;
                            break;
                        case "SECRET":
                            cfg.ServerSecret = val;
                            break;
                        case "CLIENTGUID":
                            cfg.ClientGUID = val;
                            break;
                        case "VERSION":
                            cfg._ClientVersionRequired = val;
                            if (Version.TryParse(val, out var v))
                                cfg.ClientVersionRequired = v;
                            break;
                    }
                }
                switch (ctx.Request.Url.LocalPath)
                {
                    case "/CONNECT":
                        ConnectionRequestReceived?.Invoke(null, cfg);
                        break;
                }
                var buf = Encoding.UTF8.GetBytes("OK");
                ctx.Response.ContentEncoding = Encoding.UTF8;
                ctx.Response.ContentType = "text/html";
                ctx.Response.ContentEncoding = Encoding.UTF8;
                ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                ctx.Response.Close();
                return;
            }
            catch (Exception ex)
            {
                VoicePlugin.Log(ex.ToString());
            }
        }
    }
}
