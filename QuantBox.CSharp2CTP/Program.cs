using QuantBox.CSharp2CTP.Event;
using QuantBox.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QuantBox.CSharp2CTP
{
    class Program
    {
        static MdApiWrapper MdApi = new MdApiWrapper();
        static void Main(string[] args)
        {
            MdApi.OnConnect += MdApi_OnConnect;
            MdApi.OnRtnDepthMarketData += MdApi_OnRtnDepthMarketData;

            MdApi.Connect(@"D:\", "tcp://210.51.25.90:32111", "1061", "00000010", "123456");

            Console.ReadKey();
        }

        static void MdApi_OnRtnDepthMarketData(object sender, OnRtnDepthMarketDataArgs e)
        {
            Console.WriteLine(e.pDepthMarketData.InstrumentID + " " + e.pDepthMarketData.LastPrice + ":" + Math.Round(e.pDepthMarketData.AveragePrice / 15) + ":" + e.pDepthMarketData.UpdateTime);
        }

        static void MdApi_OnConnect(object sender, OnConnectArgs e)
        {
            Console.WriteLine(e.result);
            if(e.result == ConnectionStatus.Logined)
            {
                MdApi.Subscribe("ag1412", "");
            }
        }
    }
}
