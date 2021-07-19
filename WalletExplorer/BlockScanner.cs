using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace WalletExplorer
{
    public class BlockScanner
    {

        static HttpWebRequest webRequest;

        public void run()
        {

            while (!Global.shutdown)
            {
                UInt32 lastBlock = (UInt32)Convert.ToInt32(Database.getSetting("last_block"));
                UInt32 currentHeight = getCurrentHeight();
                if (lastBlock < currentHeight - 20)
                {
                    while ((lastBlock < currentHeight - 20) && (!Global.shutdown)) {
                        lastBlock++;
                        parseBlock(lastBlock);
                        if (lastBlock % 100 == 0)
                            Console.WriteLine("Parsing block: " + lastBlock);
                        Database.setSetting("last_block", lastBlock.ToString());
                        //Thread.Sleep(1);
                    }
                }
                Thread.Sleep(5000);
            }
        }


        void parseBlock(UInt32 blockHeight)
        {

            string blockHash = rpcExec("{\"jsonrpc\": \"1.0\", \"id\":\"1\", \"method\": \"getblockhash\", \"params\": [" + blockHeight + "] }");

            dynamic dHashResult = JsonConvert.DeserializeObject<dynamic>(blockHash)["result"];

            string block = rpcExec("{\"jsonrpc\": \"1.0\", \"id\":\"1\", \"method\": \"getblock\", \"params\": [\"" + dHashResult +"\", 2] }");

            dynamic dBlockResult = JsonConvert.DeserializeObject<dynamic>(block)["result"];

            foreach (var tx in dBlockResult["tx"])
            {
                foreach (var vin in tx["vin"])
                {
                    if (vin.ContainsKey("txid"))
                    {
                        Database.spendTransaction(vin["txid"].ToString(), Convert.ToInt32(vin["vout"]));
                    }
                        
                }
                foreach (var vout in tx["vout"])
                    if (!vout["scriptPubKey"]["asm"].ToString().StartsWith("OP_RETURN"))
                    {
                        bool ok = true;
                        if (vout["scriptPubKey"].ContainsKey("type"))
                            if (vout["scriptPubKey"]["type"] == "nonstandard")
                                ok = false;
                        if (ok)
                        {
                            Database.saveTx(tx["txid"].ToString(), Convert.ToInt32(vout["n"]), Convert.ToDecimal(vout["value"]), vout["scriptPubKey"]["address"].ToString());
                            Database.updateWalletBalance(vout["scriptPubKey"]["address"].ToString(), Convert.ToDecimal(vout["value"]) * 100000000m);
                        }
                    }
            }

        }


        UInt32 getCurrentHeight()
        {
            string result = rpcExec("{\"jsonrpc\": \"1.0\", \"id\":\"1\", \"method\": \"getblockcount\", \"params\": [] }");

            dynamic dResult = JsonConvert.DeserializeObject<dynamic>(result)["result"];

            //UInt32 returnVal = Convert.ToInt32(dResult[0].ToString());

            return dResult;

        }

        public static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(ExpandoObject))
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }

        string rpcExec (string command)
        {
            webRequest = (HttpWebRequest)WebRequest.Create(Global.rpcServer);
            webRequest.KeepAlive = false;

            var data = Encoding.ASCII.GetBytes(command);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            var username = Global.rpcUser;
            var password = Global.rpcPassword;
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            webRequest.Headers.Add("Authorization", "Basic " + encoded);


            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }


            var webresponse = (HttpWebResponse)webRequest.GetResponse();

            string submitResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();

            webresponse.Dispose();
            

            return submitResponse;
        }

    }
}


