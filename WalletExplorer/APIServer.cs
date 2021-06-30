using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace WalletExplorer
{
    public class APIServer
    {

        public HttpListener listener;

        public void run()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("HTTP Listener not supported");
                return;
            }

            listener = new HttpListener();

            listener.Prefixes.Add("http://192.168.1.27:90/");

            listener.Start();
            Console.WriteLine("Listening...");

            while (!Global.shutdown)
            {
                HttpListenerContext context = listener.GetContext();
                APIWorker worker = new APIWorker();
                worker.context = context;
                Thread t1 = new Thread(new ThreadStart(worker.run));
                t1.Start();
            }

            listener.Stop();
        }

    }
}
