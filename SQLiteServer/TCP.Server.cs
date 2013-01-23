// Allgemeiner event basierter TCP-Server
//
// Änderungen:
// 23.01.2013 Tim David Saxen: Erste Version

using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

// Eigene
using Tools;

namespace TCP {

	public class Server {
		private TcpListener tcpListener;
		private Thread listenThread;

		// OnData Event
		public class OnDataEventArgs : EventArgs
		{
			private string _Data;
			public OnDataEventArgs(string Data)
			{
				_Data = Data;
			}
			public string Data
			{
				get { return _Data; }
			}
		}
		public delegate string OnDataEventHandler(object sender, OnDataEventArgs e);
		public event OnDataEventHandler OnData;

		// OnConnect/Disconnect
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

		// Constructor
		public Server (int APort)
		{
			this.tcpListener = new TcpListener(IPAddress.Any, APort);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
		}

		// Destructor
		~ Server ()
		{
			listenThread.Interrupt();
			listenThread = null;

			tcpListener.Stop();
			tcpListener = null;
		}
		
		// listenThread starten
		public void Start() {
			listenThread.Start();
		}
		
		// listenThread beenden
		public void Stop() {
			this.listenThread.Interrupt();		
		}

		// listenThread
		private void ListenForClients()
		{
			this.tcpListener.Start();
			while (true)
			{
				//blocks until a client has connected to the server
				TcpClient client = this.tcpListener.AcceptTcpClient();
				//create a thread to handle communication with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
				clientThread.Start(client);
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

			byte[] welcomeMsg = System.Text.Encoding.Unicode.GetBytes("SQLiteServer v1.0");

			var OnDataEvent = OnData;              // Niemals direkt mit dem Event arbeiten da ansonsten
			var OnConnectEvent = OnConnect;        // das Handle zwischen NULL-Abfrage und Ausführen
			var OnDisconnectEvent = OnDisconnect;  // verloren gehen kann.

			// OnConnect Event
			if (OnConnectEvent != null) OnConnectEvent(
				this,
				new OnConnectDisconnectEventArgs(
				  ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()
				)
			);

			// Sende Willkommens-Nachricht an den Client
			outStream.WriteLine("SQLiteServer v1.0");
			outStream.Flush();

			// Query initialisieren
			string Query = "";
			int Count = -1;
			bool QueryDone = false;

			// Kommunikation
			while (true)
			{
				try
				{
					if ((Line = inStream.ReadLine()) != null)
					{
						if (Line == "") {

						// REQUEST
						} else if (Line.Split(':').GetValue(0).ToString().ToUpper() == "REQUEST") {
							Count = Convert.ToInt32( Line.Split(':').GetValue(1).ToString() );
							QueryDone = false;
							Query = "";
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

						// OnData Event
						if ((QueryDone) && (OnDataEvent != null))
						{
							string QueryResult = OnDataEvent(this, new OnDataEventArgs(Query));
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

					Thread.Sleep(2);
				}
				catch
				{
					break;
				}
			}
			
			// OnDisconnect Event
			if (OnDisconnectEvent != null) OnDisconnectEvent(
				this,
				new OnConnectDisconnectEventArgs(
				((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()
				)
			);

			tcpClient.Close();
		}
	}
}