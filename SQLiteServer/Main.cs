using System;
using System.IO;
using System.Collections.Specialized; // StringDictionary
using System.Net;

// Own
using TCP;
using SQLite;
using Tools;

namespace SQLiteServer {

	class MainClass
	{
		static SQLite.Client SQLite = null;
		static TCP.Server TCPServer = null;
		static UserAuth User = new UserAuth();

		// Constructor
		public static void Main (string[] args)
		{
			try
			{
				// Initialize Variables
				string DBFile = "database.db3";
				string Host = "localhost";
				int Port = 11833;

				// Parse Commandline Parameters
				StringDictionary Parameters = Tools.System.ParseCommandlineArguments (args);
				if (Parameters ["dbfile"] != null)	DBFile = Parameters ["dbfile"];
				if (Parameters ["host"] != null)	Host = Parameters ["host"];
				if (Parameters ["port"] != null)	Port = Convert.ToInt32 (Parameters ["port"]);

				// Init SQLite-Connection
				SQLite = new SQLite.Client (Path.Combine (Tools.System.GetProgramDir (), DBFile));

				// Host -> IP
				IPAddress IP = null;
				if (Host == "0.0.0.0") {
					IP = IPAddress.Any;
				} else {
					IPHostEntry hostEntry = Dns.GetHostEntry(Host);
					if (hostEntry.AddressList.Length<1) {
						throw new System.SystemException("Cannot resolve Hostname: " + Host);
					} else {
						IP = hostEntry.AddressList[0];
					}
				}

				// Welcome Message showing Host and Port
				Console.WriteLine ("SQLiteServer v1.0 (" + DBFile + ")");
				Console.WriteLine ("Listening on " + IP.ToString() + ":" + Port);
				Console.WriteLine ("");

				// Init TCP Server
				TCP_Init (IP, Port);

			} catch (Exception e) {
				Console.WriteLine("Error: " + e.Message);
				Environment.Exit(99);
			}
		}

		// Destructor
		~ MainClass()
		{
			TCP_Free ();
			SQLite = null;
		}

		// Initalize TCP-Server
		public static void TCP_Init (IPAddress AIP, int APort)
		{
			TCPServer = new TCP.Server(AIP, APort);
			TCPServer.OnConnect += new TCP.Server.OnConnectEventHandler(TCP_OnConnect);
			TCPServer.OnDisconnect += new TCP.Server.OnDisconnectEventHandler(TCP_OnDisconnect);
			TCPServer.OnData += new TCP.Server.OnDataEventHandler(TCP_OnData);
			TCPServer.OnUser += new TCP.Server.OnUserEventHandler(TCP_OnUser);
			TCPServer.Start();
		}

		// Finalize TCP-Server
		public static void TCP_Free ()
		{
			TCPServer.Stop();
			TCPServer = null;
		}

		// TCP-Server Connect
		private static void TCP_OnConnect(object sender, TCP.Server.OnConnectDisconnectEventArgs e)
		{
			Console.WriteLine("*** Connect: " + e.RemoteEndPoint);
		}
		
		// TCP-Server Disconnect
		private static void TCP_OnDisconnect(object sender, TCP.Server.OnConnectDisconnectEventArgs e)
		{
			Console.WriteLine("*** Disconnect: " + e.RemoteEndPoint);
		}
		
		// TCP-Server got SQL Query
		private static string TCP_OnData (object sender, TCP.Server.OnDataEventArgs e)
		{
			Console.WriteLine ( e.AccessRights + " " + (e.NoResult ? "-" : "+") + " "  + e.SQLQuery);
			string SQLResult = SQLite.ExecuteSQL(e.SQLQuery, e.AccessRights, e.NoResult);
			
			return SQLResult;
		}
		
		// TCP-Server got User
		private static string TCP_OnUser (object sender, TCP.Server.OnUserEventArgs e)
		{
			string AccessRights = User.CheckUserPassword(e.Username, e.Password);

			Console.WriteLine ("*** Userlogin: " + e.Username  + " (" + AccessRights + ")");

			return AccessRights;
		}

	}
}
