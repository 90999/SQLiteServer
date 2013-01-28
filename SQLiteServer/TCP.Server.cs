using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO; // StreamReader, StreamWriter

// Own
using Tools;

namespace TCP {

	public class Server {
		private TcpListener tcpListener;
		private Thread listenThread;

		// OnData Event
		public class OnDataEventArgs : EventArgs
		{
			private string _SQLQuery;
			private Boolean _NoResult;
			private string _AccessRights;
			public OnDataEventArgs(string SQLQuery, Boolean NoResult, string AccessRights)
			{
				_SQLQuery = SQLQuery;
				_NoResult = NoResult;
				_AccessRights = AccessRights;
			}
			public string SQLQuery
			{
				get { return _SQLQuery; }
			}
			public Boolean NoResult
			{
				get { return _NoResult; }
			}
			public string AccessRights
			{
				get { return _AccessRights; }
			}
		}
		public delegate string OnDataEventHandler(object sender, OnDataEventArgs e);
		public event OnDataEventHandler OnData;

		// OnConnect/Disconnect Event
		public class OnConnectDisconnectEventArgs : EventArgs {
			private string _RemoteEndPoint;
			public OnConnectDisconnectEventArgs(string RemoteEndPoint)
			{
				_RemoteEndPoint = RemoteEndPoint;
			}
			public string RemoteEndPoint
			{
				get { return _RemoteEndPoint; }
			}
		}
		public delegate void OnConnectEventHandler(object sender, OnConnectDisconnectEventArgs e);
		public event OnConnectEventHandler OnConnect;
		public delegate void OnDisconnectEventHandler(object sender, OnConnectDisconnectEventArgs e);
		public event OnDisconnectEventHandler OnDisconnect;
		
		// OnUser Event
		public class OnUserEventArgs : EventArgs {
			private string _Username;
			private string _Password;
			public OnUserEventArgs(string Username, string Password)
			{
				_Username = Username;
				_Password = Password;
			}
			public string Username
			{
				get { return _Username; }
			}
			public string Password
			{
				get { return _Password; }
			}
		}
		public delegate string OnUserEventHandler(object sender, OnUserEventArgs e);
		public event OnUserEventHandler OnUser;

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

			var OnDataEvent = OnData;              // Never use "Event" directly. The Handle can turn
			var OnConnectEvent = OnConnect;        // NULL betwenn != null check and execution.
			var OnDisconnectEvent = OnDisconnect;  // 
			var OnUserEvent = OnUser;              // 

			// Fire: OnConnect event
			if (OnConnectEvent != null) OnConnectEvent(
				this,
				new OnConnectDisconnectEventArgs(
				  ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()
				)
			);

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
			// (3 Lines Reached -> OnDataEvent fired within Server)
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
								// Fire: OnUser event
								if (OnUserEvent != null) AccessRight = OnUserEvent(
									this,
									new OnUserEventArgs(
										LineSplited.GetValue(1).ToString(),
										LineSplited.GetValue(2).ToString()
									)
								);
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

						// Fire OnData event -> Execute SQLite-Query
						if ((QueryDone) && (OnDataEvent != null))
						{
							string QueryResult = OnDataEvent(this, new OnDataEventArgs(Query, NoResult, AccessRight));

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

			// Fire OnDisconnect event
			try
			{
  				if (OnDisconnectEvent != null) OnDisconnectEvent(
					this,
					new OnConnectDisconnectEventArgs(
					((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()
					)
				);
				tcpClient.Close();
			}
			catch
			{
			}

		}
	}
}