using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace WalletExplorer
{
    public class APIWorker
    {
        public HttpListenerContext context;

        public void run()
        {
            try
            {
                HttpListenerRequest request = context.Request;

                string strResponse = "";

                string endpoint = request.RawUrl.Substring(1);
                
                if (endpoint == "topwallets")
                {
                    strResponse = Database.getTopWallets();
                }
                else if (endpoint.StartsWith("show_wallet"))
                {
                    string[] args = endpoint.Split("=");
                    string walletAddr = args[1];

                    strResponse = Database.getOneWallet(walletAddr);

                }
                else if (endpoint.StartsWith("total_supply"))
                {
                    strResponse = Database.getTotalSupply();
                }





                HttpListenerResponse response = context.Response;

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strResponse);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                uint sum = 0;
                for (int i = 0; i < buffer.Length; i++)
                    sum += buffer[i];

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in worker thread:" + e.Message);
                Console.WriteLine(e.StackTrace);

            }
        }
    }
}
