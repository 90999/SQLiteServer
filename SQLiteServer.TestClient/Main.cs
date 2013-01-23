using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;

// Own
using SQLiteServer;

namespace TestClient
{
	class TestClient
	{
		private static SQLiteServer.Connector SQLiteServerConnector = null;

		// Constructor
		public static void Main (string[] args)
		{
			// Show Welcom Message
			Console.WriteLine ("SQLiteServer Test Client v1.0");

			// Init Variables
			string Host = "localhost";
			int Port = 11833;

			// Parse Commandline Parameters
			StringDictionary Parameters = Tools.System.ParseCommandlineArguments(args);
			if (Parameters["host"] != null) Host = Parameters["host"];
			if (Parameters["port"] != null) Port = Convert.ToInt32( Parameters["port"] );

			// Verbindung zum SQLiteServer initialisieren
			SQLiteServerConnector = new SQLiteServer.Connector ();
			try {
				String Query = "";
				String Line = "";
				SQLiteServer.SQLiteResult Result;

				// Verbindung zum SQLiteServer aufbauen
				if (! SQLiteServerConnector.Connect (Host, Port)) {
					throw new System.OperationCanceledException ("Cannot connect to SQLiteServer");
				}

				// Benutzereingaben parsen
				while (true) {
					Console.Write ("Enter SQL Query (or .quit): ");
					Line = Console.ReadLine ();
					// Eingabe: .quit -> beenden!
					if (Line.Trim().ToLower() == ".quit") {
						break;
					// Eingabe an Query anhängen
					} else {
						Query = Query + Line + Environment.NewLine;
					}
					// Wenn ein ; am ende der Zeile steht das Query an die Datenbank schicken
					if ((Line.Trim().Length > 0) && (Line.Trim() != "") && (Line.Trim().Substring(Line.Length-1, 1) == ";")) {
						Query = Query.TrimEnd('\r', '\n');;
						Result = SQLiteServerConnector.ExecSQL(Query);
						// Fehler?
						if (Result.Error) {
							Console.WriteLine("ERROR: " + Result.ErrorMessage);
						// Alles OK...
						} else {
							// Mindestens eine Zeile und eine Spalte empfangen
							if ((Result.RowCount > 0) && (Result.FieldCount > 0)) {
								Console.WriteLine(
									ArrayPrinter.GetDataInTableFormat(Result.Names, Result.Value)
								);
							// Keine Zeilen empfangen
							} else {
								Console.WriteLine("Ok.");
							}
						}
						// Nächstes query....
						Query = "";
					}
				};
			
			// Bei einem Fehler diesen ausgeben
			} catch (Exception e) {
				Console.WriteLine ("Error: " + e.Message);
				Console.WriteLine ("");
				Environment.Exit(99);
			}

		}

		// Destructor
		~ TestClient ()
		{
			// Ggf. Verbindung zum SQLite-Server trennen
			try {
				if (SQLiteServerConnector != null) SQLiteServerConnector.Disconnect ();
			} catch {
			}
		}

	}
}
