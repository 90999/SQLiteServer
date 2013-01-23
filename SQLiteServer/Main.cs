// SQLiteServer v1.0
//
// TCP-Server zum Verbinden mehrere TCP-Clients zu einer SQLite Datenbank
//
// Änderungen:
// 23.01.2013 Tim David Saxen: Erste Version

using System;
using System.IO;
using System.Collections.Generic; // List<T>

// Eigene
using TCP;
using SQLite;
using Tools;

namespace SQLiteServer {

	class MainClass
	{
		static int TCP_Port = 11833;
		static SQLite.Client SQLite = new SQLite.Client(
			Path.Combine(Tools.System.GetProgramDir(), "Database.db3")
		);
		static TCP.Server TCPServer;

		// Constructor
		public static void Main (string[] args)
		{
			Console.WriteLine("Starting TCP Server on port #" + TCP_Port);
			TCP_Init();
		}

		// Destructor
		~ MainClass()
		{
			TCP_Free ();
			SQLite = null;
		}

		// TCP-Server initialisieren
		public static void TCP_Init ()
		{
			TCPServer = new TCP.Server(TCP_Port);
			TCPServer.OnConnect += new TCP.Server.OnConnectEventHandler(TCP_OnConnect);
			TCPServer.OnDisconnect += new TCP.Server.OnDisconnectEventHandler(TCP_OnDisconnect);
			TCPServer.OnData += new TCP.Server.OnDataEventHandler(TCP_OnData);
			TCPServer.Start();
		}

		// TCP-Server beenden
		public static void TCP_Free ()
		{
			TCPServer.Stop();
			TCPServer = null;
		}

		// TCP-Server Connect
		private static void TCP_OnConnect(object sender, TCP.Server.OnConnectDisconnectEventArgs e)
		{
			Console.WriteLine("TCP_OnConnect: " + e.RemoteEndPoint);
		}
		
		// TCP-Server Disconnect
		private static void TCP_OnDisconnect(object sender, TCP.Server.OnConnectDisconnectEventArgs e)
		{
			Console.WriteLine("TCP_OnDisconnect: " + e.RemoteEndPoint);
		}
		
		// TCP-Server Daten empfangen
		private static string TCP_OnData (object sender, TCP.Server.OnDataEventArgs e)
		{
			Console.WriteLine ("TCP_OnData: " + e.Data);
			string SQLResult = SQLite.ExecuteSQL(e.Data);

			return SQLResult;
		}
		
	}
}
