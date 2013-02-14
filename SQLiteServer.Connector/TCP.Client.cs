using System;
using System.IO; // StreamReader, StreamWriter
using System.Xml;
using System.Xml.Linq;
using System.Net.Sockets; // TcpClient

namespace TCP
{
	internal class Client
	{
		private string Host = "localhost";
		private int Port = 11833;
		private string Username = "Admin";
		private string Password = "Admin";

		private TcpClient Connection = null;
		private NetworkStream Stream = null; 

		// Constructor
		public Client(string AHost, int APort, string AUsername, string APassword)
		{
			// Store Variables
			Host = AHost;
			Port = APort;
			Username = AUsername;
			Password = APassword;
		}
		
		// Destructor
		~ Client ()
		{
			Stream = null;
			Connection = null;
		}
		
		// Connect
		public Boolean Connect ()
		{
			// Connect
			if (Connection != null) return false;
			try {
				Connection = new TcpClient(Host, Port);
				Stream = Connection.GetStream();

				StreamReader inStream = new StreamReader (Stream, System.Text.Encoding.UTF8);
				StreamWriter outStream = new StreamWriter (Stream);

				Int64 StartTick = 0;
				string Line = "";

				// Wait for welcome Message from Server...
				StartTick = System.DateTime.Now.Ticks;
				Line = "";
				while (true) {
					if ((System.DateTime.Now.Ticks-StartTick > 100000000)) { // 10sec (timeout!)
						throw new System.TimeoutException("Welcome screen timed out");
					}
					if ((Line = inStream.ReadLine()) != null) {
						if (Line == "") {
							//
						} else if (Line == "SQLiteServer v1.0") {
							break;
						} else {
							throw new System.NotSupportedException("Wrong version detected: " + Line);
						}
					}
				}

				// Send Auth information
				outStream.WriteLine(
					"USER:" + Username + ":" + Password,
					System.Text.Encoding.UTF8
				);
				outStream.Flush();

				// Wait for Auth reply...
				StartTick = System.DateTime.Now.Ticks;
				Line = "";
				while (true) {
					if ((System.DateTime.Now.Ticks-StartTick > 100000000)) { // 10sec (timeout!)
						throw new System.TimeoutException("Login timed out");
					}
					if ((Line = inStream.ReadLine()) != null) {
						if (Line == "") {
							//
						} else if (Line == "RIGHTS:rw") {
							break;
						} else if (Line == "RIGHTS:ro") {
							break;
						} else {
							throw new System.NotSupportedException("Wrong result detected: " + Line);
						}
					}
				}

				return true;
			} catch {
				Connection = null;
				Stream = null;
				return false;
			}
		}
		
		// Connected
		public Boolean Connected ()
		{
			if (Connection == null) return false;

			return Connection.Connected;
		}
		
		// Disconnect
		public Boolean Disconnect ()
		{
			if (Connection != null) {
				try
				{
					Stream.Close ();
					Connection.Close ();   
					Connection = null;
				}
				catch
				{
					return false;
				}
			}

			// Rückgabe
			return true;
		}
		
		// ExecSQL
		public string ExecSQL (string ASQLQuery, Boolean ANoResult = false)
		{
			try
			{
				StreamReader inStream = new StreamReader (Stream, System.Text.Encoding.UTF8);
				StreamWriter outStream = new StreamWriter (Stream);

				// Request senden
				outStream.WriteLine(
					"REQUEST:" + ASQLQuery.Split('\n').Length + ":" + (ANoResult ? "1" : "0"),
					System.Text.Encoding.UTF8
				);
				foreach (string Line in ASQLQuery.Split('\n')) {
					outStream.WriteLine(
						"." + Line,
						System.Text.Encoding.UTF8
					);
				}
				outStream.Flush ();

				// Only wait if Result is desired!
				if (ANoResult == false) {
					// Wait on result
					string RecvStr = "";
					string RecvLine = "";
					int Count = -1;
					bool RecvDone = false;
					Int64 LastLine = System.DateTime.Now.Ticks;
					while (true) {
						if ((System.DateTime.Now.Ticks-LastLine > 10000000)) { // 1sec since last line (timeout!)
							throw new System.TimeoutException("SQL query timed out");
						}
						if ((RecvLine = inStream.ReadLine()) != null) {
							if (RecvLine != "") LastLine = System.DateTime.Now.Ticks;
							if (RecvLine == "") {
								//

							// RESULT
							} else if (RecvLine.Split(':').GetValue(0).ToString().ToUpper() == "RESULT") {
								Count = Convert.ToInt32( RecvLine.Split(':').GetValue(1).ToString() );
								RecvDone = false;
								// .
							} else if ((!RecvDone) && (Count>0) && (RecvLine.Substring(0,1) == ".")) {
								Count = Count - 1;
								RecvStr = RecvStr + RecvLine.Substring(1,RecvLine.Length-1) + Environment.NewLine;
								if (Count == 0) {
									RecvStr = RecvStr.TrimEnd('\r', '\n');
									RecvDone = true;
									Count = -1;
								}
							}
							if (RecvDone) {
								return RecvStr;
							}
						}
					}
				} else {
					return String.Empty;
				}

			} catch (Exception e) {
				Disconnect();

				// Create XML error Document
				XDocument XML = new XDocument (new XDeclaration ("1.0", "utf-8", "yes"));
				XElement XRoot = new XElement ("Result");
				
				// Add error message to XML Document
				XElement XStatus = new XElement ("Status");
				XStatus.Add (new XAttribute ("Error", true));
				XStatus.Add (new XAttribute ("ErrorMessage", "Client Exception: " + e.Message));
				XRoot.Add (XStatus);
				
				// Return XML-Document as String
				XML.Add (XRoot);
				using(var MS = new MemoryStream())
				{
					using (StreamReader SR = new StreamReader(MS, System.Text.Encoding.UTF8)) {
						XML.Save(MS);
						MS.Seek(0, SeekOrigin.Begin);
						return SR.ReadToEnd();
					}
				}
			}
		}

	}
}

