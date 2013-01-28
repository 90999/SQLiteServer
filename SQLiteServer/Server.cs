using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO; // StreamReader, StreamWriter

// Own
using Tools;

namespace SQLiteServer {

	public class Server {
		private TcpListener tcpListener;
		private Thread listenThread;

		// Constructor
		public Server (IPAddress AIP, int APort)
		{
			tcpListener = new TcpListener (AIP, APort);

			listenThread = new Thread(ListenForClients);
		}

		// Destructor
		~ Server ()
		{
			listenThread.Interrupt();
			listenThread = null;

			tcpListener.Stop();
			tcpListener = null;
		}
		
		// listenThread start
		public void Start() {
			listenThread.Start();
		}

		// listenThread
		private void ListenForClients ()
		{
			this.tcpListener.Start ();

			try
			{
				while (true) {
					if (! tcpListener.Pending()) {
						Thread.Sleep(10); 
						continue; 
					} else {
						TcpClient client = this.tcpListener.AcceptTcpClient();
						Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
						clientThread.Start(client);
//						client.Close ();
					}
				}
			}
			catch (ThreadInterruptedException)
			{
			}
		}	

		// clientThread
		private void HandleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream ns = tcpClient.GetStream();
			StreamReader inStream = new StreamReader(ns);
			StreamWriter outStream = new StreamWriter(ns);
			string Line;
			string AccessRight = ""; // ro = ReadOnly | rw = ReadWrite | other = NoAccess!

			// On Connect
			Console.WriteLine("*** Connect: " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());

			// Send Welcome Message to Client
			outStream.WriteLine("SQLiteServer v1.0");
			outStream.Flush();

			// Initialize Query buffer
			string Query = "";
			int Count = -1;
			bool QueryDone = false;
			bool NoResult = false;
			string[] LineSplited = {};

			// Communication

			// Protocol:
			// Server: SQLiteServer v1.0
			// Client: USER:Name:Password
			// Server: RIGHTS:rw        
			// Client: REQUEST:3:1      <- Where 3 is Number of Lines following and 1 means no result (0 = with result)
			// Client: .SELECT          <- Following 3 Lines are SQL Query-Lines prefixed by "."
			// Client: .*
			// Client: .FROM test;
			// (3 Lines Reached -> OnSQLQueryEvent fired within Server)
			// Server: RESULT:10        <- Where 10 is Number of Lines following
			// Server: .<xml...         <- Following 10 Lines is the XML-Result of the Query
			// (10 Lines Reached -> Client Parses Result)

			try
			{
				while (true)
				{
					// Data in Queue?
					if ((Line = inStream.ReadLine()) != null)
					{
						if (Line == "") {
							//						
						} else {
							LineSplited = Line.Split(':');

							// USER
							if ((LineSplited.Length == 3) && (LineSplited.GetValue(0).ToString().ToUpper() == "USER")) {

								lock (Sync.signal)
								{
									AccessRight = MainClass.User.CheckUserPassword(
										LineSplited.GetValue(1).ToString(),
										LineSplited.GetValue(2).ToString()
							   		);
								}
								
								Console.WriteLine ("*** Userlogin: " + LineSplited.GetValue(1).ToString()  + " (" + AccessRight + ")");

								outStream.WriteLine(
									"RIGHTS:" + AccessRight
								);
								outStream.Flush();

							// REQUEST
							} else if ((LineSplited.Length >= 1) && (LineSplited.GetValue(0).ToString().ToUpper() == "REQUEST")) {
								Query = "";
								QueryDone = false;
								if (LineSplited.Length >= 2) {
									Count = Convert.ToInt32( LineSplited.GetValue(1).ToString() );
								} else {
									Count = 0;
								}
								if (LineSplited.Length >= 3) {
									NoResult = Convert.ToBoolean( LineSplited.GetValue(2).ToString() == "1" );
								} else {
									NoResult = false;
								}
							// .
							} else if ((QueryDone == false) && (Count>0) && (Line.Substring(0,1) == ".")) {
								Count = Count - 1;
								Query = Query + Line.Substring(1,Line.Length-1) + Environment.NewLine;
								if (Count == 0) {
									Query = Query.TrimEnd('\r', '\n');
									QueryDone = true;
									Count = -1;
								}
							}
						}

						// Fire OnSQLQuery event -> Execute SQLite-Query
						if (QueryDone)
						{
							string QueryResult = "";

							// Execute Query
							lock (Sync.signal)
							{
								Console.WriteLine ( AccessRight + " " + (NoResult ? "-" : "+") + " "  + Query);
								MainClass.SQLite.ExecuteSQL(Query, AccessRight, ref QueryResult, NoResult);
							}

							// QueryResult = OnSQLQueryEvent(this, new OnSQLQueryEventArgs(Query, NoResult, AccessRight));

							// If there is a Result request return result...
							if (! NoResult) {
								outStream.WriteLine(
									"RESULT:" + QueryResult.Split('\n').Length
								);
								foreach (string QueryResultLine in QueryResult.Split('\n')) {
									outStream.WriteLine(
										"." + QueryResultLine
									);
								}
								outStream.Flush();
							}

						}
					}

					Thread.Sleep(10);
				} // while
			} // try
			catch
			{
			}

			// On Disconnect
			try
			{
				Console.WriteLine("*** Disconnect: " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
				tcpClient.Close();
			}
			catch
			{
			}

		}
	}
}