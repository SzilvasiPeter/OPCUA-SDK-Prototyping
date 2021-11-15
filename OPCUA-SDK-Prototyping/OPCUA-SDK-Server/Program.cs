using System;
using System.Collections.Generic;
using System.Threading;

using Opc.UaFx;
using Opc.UaFx.Server;

namespace OPCUA_SDK_Server
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
            myProducerControl = new CancellationTokenSource();
            myDataNodes = new List<OpcDataVariableNode<int>>();
            myDataNode = CreateDataNode();

            Thread producer = new Thread(ProduceDataChanges);

            using (OpcServer server = new OpcServer("opc.tcp://localhost:5555/", myDataNode))
            {
                server.Start();
                producer.Start(server);

                Console.WriteLine("Press any key to exit");
                Console.ReadKey(true);

                myProducerControl.Cancel();
                producer.Join();
            }
        }

        #region Private Fields ----------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        private const int NODES_NUMBER = 30;

        /// <summary>
        /// 
        /// </summary>
        private const int INTERVAL = 3000;

        /// <summary>
        /// 
        /// </summary>
        private static CancellationTokenSource myProducerControl;

        /// <summary>
        /// 
        /// </summary>
        private static OpcFolderNode myDataNode;

        /// <summary>
        /// 
        /// </summary>
        private static List<OpcDataVariableNode<int>> myDataNodes;

        /// <summary>
        /// 
        /// </summary>
        private static OpcDataVariableNode<DateTime> myTimestampNode;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static OpcFolderNode CreateDataNode()
        {
            OpcFolderNode dataNode = new OpcFolderNode("Data");

            for (int i = 0; i < NODES_NUMBER; i++)
            {
                myDataNodes.Add(new OpcDataVariableNode<int>(dataNode, $"node{i}", i));
            }

            myTimestampNode = new OpcDataVariableNode<DateTime>(dataNode, "Timestamp");

            return dataNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private static void ProduceDataChanges(object state)
        {
            OpcServer server = (OpcServer)state;

            while (!myProducerControl.IsCancellationRequested)
            {
                lock (state)
                {
                    foreach (OpcDataVariableNode<int> node in myDataNodes)
                    {
                        node.Value++;
                    }

                    myTimestampNode.Value = DateTime.UtcNow;
                    myDataNode.ApplyChanges(server.SystemContext, recursive: true);
                }
            
                Console.WriteLine("{0} BULK: New Data Incoming - pause for {1} ms", DateTime.Now, INTERVAL);

                // Wait for Cancel or next Bulk change.
                if (myProducerControl.Token.WaitHandle.WaitOne(INTERVAL))
                {
                    break;
                }
            }
        }

        #endregion Private Fields -------------------------------------------------------------------
    }
}
