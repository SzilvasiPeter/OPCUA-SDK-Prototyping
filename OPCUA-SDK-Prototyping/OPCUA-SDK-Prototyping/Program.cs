using System;

using Opc.UaFx.Client;

namespace OPCUA_SDK_Prototyping
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpoint = "opc.tcp://localhost:26543/BatchPlantServer";
            using (var client = new OpcClient(endpoint))
            {
                client.Connect();

                var mixerLCToutput = client.ReadNode("ns=2;i=400");
                Console.WriteLine("LoadcellTransmitter output value: {0}", mixerLCToutput);
            }
        }
    }
}
