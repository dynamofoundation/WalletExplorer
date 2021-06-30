using System;
using System.Threading;

namespace WalletExplorer
{
    class Program
    {
        static void Main(string[] args)
        {

            Global.shutdown = false;

            BlockScanner scanner = new BlockScanner();
            Thread t1 = new Thread(new ThreadStart(scanner.run));
            t1.Start();

            APIServer server = new APIServer();
            Thread t2 = new Thread(new ThreadStart(server.run));
            t2.Start();

            while (!Global.shutdown)
            {
                if (Console.KeyAvailable)
                {
                    Global.shutdown = true;
                }
                Thread.Sleep(100);
            }
        }
    }
}
