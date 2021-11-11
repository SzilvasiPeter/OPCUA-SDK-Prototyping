using System;
using System.Threading.Tasks;

using Opc.UaFx;
using Opc.UaFx.Server;

namespace OPCUA_SDK_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpoint = "opc.tcp://localhost:5555/Temperature";
            var temperatureNode = new OpcDataVariableNode<double>("Temperature", 100.0);
            using (var server = new OpcServer(endpoint, temperatureNode))
            {
                server.Start();
                while (true)
                {
                    if(temperatureNode.Value == 110)
                        temperatureNode.Value = 100;
                    else
                        temperatureNode.Value++;

                    Console.WriteLine("Temperature output value: {0}", temperatureNode.Value);
                    temperatureNode.ApplyChanges(server.SystemContext);
                    Task.Delay(1000).Wait();
                }
            }
        }
    }
}
