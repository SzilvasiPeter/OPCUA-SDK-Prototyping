using System;
using System.Threading.Tasks;

using Opc.UaFx.Client;

namespace OPCUA_SDK_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpoint = "opc.tcp://localhost:5555/Temperature";
            using (var client = new OpcClient(endpoint))
            {
                client.Connect();

                while (true)
                {
                    var temperature = client.ReadNode("ns=2;s=Temperature");
                    Console.WriteLine("Temperature output value: {0}", temperature);

                    Task.Delay(1000).Wait();
                }
            }
        }
    }
}
