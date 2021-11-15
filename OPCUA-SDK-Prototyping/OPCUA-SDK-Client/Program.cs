using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Opc.UaFx;
using Opc.UaFx.Client;

namespace OPCUA_SDK_Client
{
    /// <summary>
    /// 
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        internal static void Main(string[] args)
        {
            myConsumerControl = new CancellationTokenSource();
            myDataChanges = new BlockingCollection<OpcValue>();

            Thread consumer = new Thread(ConsumeDataChanges);

            using(OpcClient client = new OpcClient("opc.tcp://localhost:5555/"))
            {
                client.Connect();
                consumer.Start(client);

                Console.WriteLine("Press any key to exit");
                Console.ReadKey(true);

                myConsumerControl.Cancel();
                consumer.Join();
            }
        }

        #region Private Fields ----------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        private static CancellationTokenSource myConsumerControl;
        
        /// <summary>
        /// 
        /// </summary>
        private static BlockingCollection<OpcValue> myDataChanges;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private static void ConsumeDataChanges(object state)
        {
            OpcClient client = (OpcClient)state;
            client.SubscribeNodes(CreateCommands(client, "ns=2;s=Data"));

            while (!myConsumerControl.IsCancellationRequested)
            {
                try
                {
                    OpcValue value = myDataChanges.Take(myConsumerControl.Token);

                    if(value.Value is DateTime timestamp)
                    {
                        Console.WriteLine("{0} BULK: Completed (Duration = {1} ms)", DateTime.Now, DateTime.UtcNow.Subtract(timestamp).TotalMilliseconds);
                    }
                    else
                    {
                        Console.WriteLine("{0} BULK: New Data: {1}", DateTime.Now, value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="rootNodeId"></param>
        /// <returns></returns>
        private static IEnumerable<OpcSubscribeDataChange> CreateCommands(OpcClient client, string rootNodeId)
        {
            OpcNodeInfo node = client.BrowseNode(rootNodeId);

            foreach (OpcNodeInfo childNode in node.Children())
            {
                yield return new OpcSubscribeDataChange(childNode.NodeId, HandleDataChange);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HandleDataChange(object sender, OpcDataChangeReceivedEventArgs e)
        {
            myDataChanges.Add(e.Item.Value);
        }

        #endregion Private Fields -------------------------------------------------------------------
    }
}
