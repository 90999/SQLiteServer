using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace TCP
{
	internal class Client
	{
		private string Host = "";
		private int Port = 0;
		private TcpClient Connection = null;
		private NetworkStream Stream = null; 

		// Constructor
		public Client()
		{
		  //
		}
		
		// Destructor
		~ Client ()
		{
			Stream = null;
			Connection = null;
		}
		
		// Connect
		public Boolean Connect (string AHost, int APort)
		{
			// Variablen übernehmen
			Host = AHost;
			Port = APort;
			
			// Verbinden
			if (Connection != null) return false;
			try {
				Connection = new TcpClient(Host, Port);
				Stream = Connection.GetStream();

				StreamReader inStream = new StreamReader (Stream);
				StreamWriter outStream = new StreamWriter (Stream);

				string welcomeMsg = inStream.ReadLine();
				if (welcomeMsg != "SQLiteServer v1.0") {
					throw new System.NotSupportedException("Wrong version detected: " + welcomeMsg);
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
			if (Connection == null) {
				Stream.Close ();
				Connection.Close ();   
			}

			// Rückgabe
			return true;
		}
		
		// ExecSQL
		public string ExecSQL (string ASQLQuery)
		{
			if (Connection == null) {
				// XML-Fehler-Dokument erstellen
				XDocument XML = new XDocument (new XDeclaration ("1.0", "utf-8", "yes"));
				XElement XRoot = new XElement ("Result");
				
				// Fehlermeldung in XML-Dokument schreiben
				XElement XStatus = new XElement ("Status");
				XStatus.Add (new XAttribute ("Error", true));
				XStatus.Add (new XAttribute ("ErrorMessage", "Client Error: Not connected to SQLiteServer"));
				XRoot.Add (XStatus);

				// XML-Dokument in String wandeln und zurückgeben
				XML.Add (XRoot);
				return XML.Declaration.ToString () + Environment.NewLine + XML.ToString ();
			}

			StreamReader inStream = new StreamReader (Stream);
			StreamWriter outStream = new StreamWriter (Stream);

			try {
				// Request senden
				outStream.WriteLine("REQUEST:" + ASQLQuery.Split('\n').Length);
				foreach (string Line in ASQLQuery.Split('\n')) {
					outStream.WriteLine("." + Line);
				}
				outStream.Flush ();

				// Jetzt auf eine Antwort warten...
				string RecvStr = "";
				string RecvLine = "";
				int Count = -1;
				bool RecvDone = false;
				Int64 LastLine = System.DateTime.Now.Ticks;

				while (true) {
					if ((System.DateTime.Now.Ticks-LastLine > 10000000)) { // Letzte Zeile mehr als 1 sekunde her
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
			} catch (Exception e) {
				// XML-Fehler-Dokument erstellen
				XDocument XML = new XDocument (new XDeclaration ("1.0", "utf-8", "yes"));
				XElement XRoot = new XElement ("Result");
				
				// Fehlermeldung in XML-Dokument schreiben
				XElement XStatus = new XElement ("Status");
				XStatus.Add (new XAttribute ("Error", true));
				XStatus.Add (new XAttribute ("ErrorMessage", "Client Exception: " + e.Message));
				XRoot.Add (XStatus);
				
				// XML-Dokument in String wandeln und zurückgeben
				XML.Add (XRoot);
				return XML.Declaration.ToString () + Environment.NewLine + XML.ToString ();
			}
		}

	}
}

