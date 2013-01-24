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
			Console.WriteLine ("");

			// Init Variables
			string Host = "localhost";
			int Port = 11833;

			// Parse Commandline Parameters
			StringDictionary Parameters = Tools.System.ParseCommandlineArguments(args);
			if (Parameters["host"] != null) Host = Parameters["host"];
			if (Parameters["port"] != null) Port = Convert.ToInt32( Parameters["port"] );

			// Verbindung zum SQLiteServer initialisieren
			try {
				SQLiteServerConnector = new SQLiteServer.Connector (Host, Port);

				Console.WriteLine ("Commands:");
				Console.WriteLine ("");
				Console.WriteLine ("  .result={n}  Request query result (0=off 1=on, default=1)");
				Console.WriteLine ("  .quit        Exit this client application");
				Console.WriteLine ("");

				String Query = "";
				String Line = "";
				Boolean NoResult = false;
				SQLiteServer.SQLiteResult Result;

				// Benutzereingaben parsen
				while (true) {
					Console.Write ("Enter SQL Query or Command: ");
					Line = Console.ReadLine ();
					// Eingabe: .quit -> beenden!
					if (Line.Trim().ToLower() == ".quit") {
						Console.WriteLine("Bye!");
						Console.WriteLine("");
						break;
					// Result
					} else if (Line.Trim().ToLower() == ".result=0") {
						NoResult = true;
						Console.WriteLine("Result requests switched off.");
						Console.WriteLine("");
					} else if (Line.Trim().ToLower() == ".result=1") {
						NoResult = false;
						Console.WriteLine("Result requests switched on.");
						Console.WriteLine("");
						// Eingabe an Query anhängen
					} else {
						Query = Query + Line + Environment.NewLine;
					}
					// Wenn ein ; am ende der Zeile steht das Query an die Datenbank schicken
					if ((Line.Trim().Length > 0) && (Line.Trim() != "") && (Line.Trim().Substring(Line.Length-1, 1) == ";")) {
						Query = Query.TrimEnd('\r', '\n');;
						Result = SQLiteServerConnector.ExecSQL(Query, NoResult);
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
								Console.WriteLine( (NoResult ? "Ok. (Result requests disabled!)" : "Ok."));
								Console.WriteLine("");
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
			//
		}

	}
}
