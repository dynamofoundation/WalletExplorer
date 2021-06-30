This program connects to a full node via RPC and queries the blockchain for wallet transactions.  By processing all blocks and their vin/vout transactions, it aggregates a balance per address.  This was initially developed to satisfy requirements for CoinMarketCap listing which requires endpoints for rich list, total supply and per wallet balances.

You will need to install MySQL and create a database of your choosing.  Params are located in Global.cs  You will also need to modify the bind address in the HTTP listener.

After initial download, the program will continually poll the RPC for new blocks and process them as they are mined.

This is compatible with any BTC chain fork.

Connect with us on Discord or Telegram for further support/details or to make a PR.   https://www.dynamocoin.org/

The endpoints are:

http://xxxx/topwallets
http://xxxx/show_wallet=<wallet addr>
http://xxxx/total_supply



The database tables are:

CREATE TABLE `addr` (
  `addr_id` varchar(64) NOT NULL,
  `addr_balance` decimal(18,0) NOT NULL,
  PRIMARY KEY (`addr_id`)
);

CREATE TABLE `setting` (
  `setting_name` varchar(50) NOT NULL,
  `setting_value` varchar(50) NOT NULL,
  PRIMARY KEY (`setting_name`)
);

CREATE TABLE `tx` (
  `tx_id` varchar(64) NOT NULL,
  `tx_vout` int NOT NULL,
  `tx_amount` decimal(18,0) NOT NULL,
  `tx_addr` varchar(64) NOT NULL,
  PRIMARY KEY (`tx_id`,`tx_vout`)
);

You will need to add one row to the setting table which keeps track of the last block processed.

INSERT INTO `dynwallet`.`setting`
(`setting_name`,
`setting_value`)
VALUES
('last_block', 0);
