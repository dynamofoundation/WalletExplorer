using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace WalletExplorer
{
    public class Database

    {

        static string strConn = "datasource=localhost;port=3306;username=" + Global.dbUser + ";password=" + Global.dbPassword + ";database=" + Global.dbSchema;

        public static void setSetting(string name, string value)
        {
            string strSQL = "update setting set setting_value = @1 where setting_name = @2";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", value);
            cmd.Parameters.AddWithValue("@2", name);
            cmd.ExecuteNonQuery();
            conn.Close();
        }


        public static string getSetting(string name)
        {
            string strSQL = "select setting_value from setting where setting_name = @1";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", name);
            string result = cmd.ExecuteScalar().ToString();
            conn.Close();
            return result;
        }


        public static void saveTx (string txID, int n, decimal amount, string address)
        {
            amount *= 100000000m;

            string strSQL = "insert into tx (tx_id, tx_vout, tx_amount, tx_addr) values (@1, @2, @3, @4)";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", txID);
            cmd.Parameters.AddWithValue("@2", n);
            cmd.Parameters.AddWithValue("@3", amount);
            cmd.Parameters.AddWithValue("@4", address);
            cmd.ExecuteNonQuery();
            conn.Close();

        }

        public static void spendTransaction (string txid, int vout)
        {
            string strSQL = "select tx_addr, tx_amount from tx where tx_id = @1 and tx_vout = @2";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", txid);
            cmd.Parameters.AddWithValue("@2", vout);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                updateWalletBalance(reader.GetString(0), -1m * Convert.ToDecimal(reader.GetString(1)));
            }
            conn.Close();


        }

        public static void updateWalletBalance (string address, decimal amount)
        {
            

            string strSQL;
            if (walletExists(address)) {
                strSQL = "update addr set addr_balance = addr_balance + @1 where addr_id = @2";
                MySqlConnection conn = new MySqlConnection(strConn);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@1", amount);
                cmd.Parameters.AddWithValue("@2", address);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            else
            {
                strSQL = "insert into addr (addr_id, addr_balance) values (@1, @2)";
                MySqlConnection conn = new MySqlConnection(strConn);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@1", address);
                cmd.Parameters.AddWithValue("@2", amount);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        static bool walletExists (string address)
        {
            string strSQL = "select count(1) from addr where addr_id = @1";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", address);
            int result = Convert.ToInt32(cmd.ExecuteScalar().ToString());
            conn.Close();
            return result > 0;

        }

        public static string getTopWallets()
        {

            string result = "<html><body><table border=\"1\">";

            string strSQL = "select addr_id, addr_balance/ 100000000 from addr order by addr_balance desc limit 100";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result += "<tr>";
                result += "<td>" + reader.GetString(0) + "</td>";
                result += "<td>" + reader.GetString(1) + "</td>";
                result += "</tr>";
            }
            conn.Close();

            result += "</table></body></html>";

            return result;

        }


        public static string getOneWallet(string addr)
        {

            string result = "<html><body><table border=\"1\">";

            string strSQL = "select addr_id, addr_balance/ 100000000 from addr where addr_id = @1";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", addr);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result += "<tr>";
                result += "<td>" + reader.GetString(0) + "</td>";
                result += "<td>" + reader.GetString(1) + "</td>";
                result += "</tr>";
            }
            conn.Close();

            result += "</table></body></html>";

            return result;

        }

        public static string getTotalSupply()
        {
            return getSetting("last_block");
        }

        public static UInt32 walletCount()
        {
            string strSQL = "select count(1) from addr where addr_balance > 0";
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@1", address);
            UInt32 result = Convert.ToUInt32(cmd.ExecuteScalar().ToString());
            conn.Close();
            return result;

        }

    }
}
