using System;
using System.IO;
using System.Collections.Specialized; // StringDictionary
using System.Net;
using System.Threading;

// Own
using SQLite;
using Tools;

namespace SQLiteServer {

	class Sync {
		public static Object signal;
		public static bool block;
	}

	class MainClass
	{
		public static SQLite.Client SQLite = null; // use lock!
		static Server TCPServer = null;
		public static UserAuth User = null; // use lock!

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

				// Init Synchronization
				Sync.signal = new object();
				Sync.block = true;

				// Init SQLite-Connection
				SQLite = new SQLite.Client (Path.Combine (Tools.System.GetProgramDir (), DBFile));

				// Init UserAuth
				User = new UserAuth();

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

				//while(true) {
				//	Sleep
				// }

			} catch (Exception e) {
				Console.WriteLine("Error: " + e.Message);
				Environment.Exit(99);
			}
		}

		// Destructor
		~ MainClass()
		{
			lock (Sync.signal)
			{
				Monitor.Wait(Sync.signal);

				TCP_Free ();
				SQLite = null;
			}
		}

		// Initalize TCP-Server
		public static void TCP_Init (IPAddress AIP, int APort)
		{
			TCPServer = new Server(AIP, APort);
			TCPServer.Start();
		}

		// Finalize TCP-Server
		public static void TCP_Free ()
		{
			TCPServer = null;
		}

	}
}
