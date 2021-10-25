﻿using System;
using TShockAPI;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using TerrariaApi.Server;
using Terraria;

namespace AntiVPN_TShock
{
    [ApiVersion(2, 1)]
    public class AntiVPN_TShock : TerrariaPlugin
    {
        public override string Author => "hdseventh";
        public override string Description => "A simple anti-vpn plugin powered by iphub.info";
        public override string Name => "Anti-VPN Plugin";
        public override Version Version
        {
            get { return new Version(1, 0, 0, 2); }
        }
        public AntiVPN_TShock(Main game) : base(game)
        {

        }

        private static readonly HttpClient client = new HttpClient();

        public override void Initialize()
        {

            ServerApi.Hooks.ServerJoin.Register(this, OnJoinAsync);
        }

        async void OnJoinAsync(JoinEventArgs args)
        {

            if (TShock.Players[args.Who] == null)
                return;

            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create("http://v2.api.iphub.info/ip/" + TShock.Players[args.Who].IP);

                httpRequest.Headers["X-Key"] = "YOUR IPHUB KEY HERE";
                string getVPN;

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic data = JObject.Parse(result);
                    getVPN = (string)data.SelectToken("block");
                }

                var statusCode = httpResponse.StatusCode;
                if (statusCode.ToString() == "OK")
                {
                    if (getVPN == "1" && TShock.Players[args.Who] != null)
                    {
                        TShock.Players[args.Who].Disconnect("AntiProxy: Proxy and VPN connections are not permitted.");
                    }
                }
                else
                {
                    if (TShock.Players[args.Who] != null)
                    {
                        Console.WriteLine(TShock.Players[args.Who].IP + "'s Connection is not checked, Status : " + statusCode);
                    }
                }
            }
            catch (Exception e)
            {
                //TShock.Players[args.Who].Disconnect("Server join failed, try again in a few minutes.");
                Console.WriteLine(TShock.Players[args.Who].IP + "'s Connection is not checked, Error : " + e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoinAsync);
            }
            base.Dispose(disposing);
        }
    }
}
