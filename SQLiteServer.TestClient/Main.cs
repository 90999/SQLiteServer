using System;
using System.Collections.Specialized; // StringDictionary
using System.Xml;
using System.IO; // MemoryStream

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
			// Init Console Output
			// Console.OutputEncoding = System.Text.Encoding.UTF8;
			// Console.InputEncoding = System.Text.Encoding.UTF8;

			// Show Welcom Message
			Console.WriteLine ("SQLiteServer Test Client v1.0");
			Console.WriteLine ("");

			// Init Variables
			string Host = "localhost";
			int Port = 11833;
			string Username = "Admin";
			string Password = "Admin";

			// Parse Commandline Parameters
			StringDictionary Parameters = Tools.System.ParseCommandlineArguments(args);
			if (Parameters["host"] != null) Host = Parameters["host"];
			if (Parameters["port"] != null) Port = Convert.ToInt32( Parameters["port"] );
			if (Parameters["user"] != null) Username = Parameters["user"];
			if (Parameters["pass"] != null) Password = Parameters["pass"];

			// Verbindung zum SQLiteServer initialisieren
			try {
				SQLiteServerConnector = new SQLiteServer.Connector (Host, Port, Username, Password);

				Console.WriteLine ("Commands:");
				Console.WriteLine ("");
				Console.WriteLine ("  .result={n}  Request query result (0=off 1=on, default=1)");
				Console.WriteLine ("  .raw={n}     Display raw XML result (0=off 1=on, default=0)");
				Console.WriteLine ("  .quit        Exit this client application");
				Console.WriteLine ("");

				String Query = "";
				String Line = "";
				Boolean NoResult = false;
				Boolean DisplayRaw = false;
				SQLiteServer.SQLiteResult Result;

				// Benutzereingaben parsen
				while (true) {
					Console.Write ("Enter SQL Query or Command: ");
					Line = Console.ReadLine ();
					// Option: .quit -> exit
					if (Line.Trim().ToLower() == ".quit") {
						Console.WriteLine("Bye!");
						Console.WriteLine("");
						break;
					// Option: .result
					} else if (Line.Trim().ToLower() == ".result=0") {
						NoResult = true;
						Console.WriteLine("Result requests switched off.");
						Console.WriteLine("");
					} else if (Line.Trim().ToLower() == ".result=1") {
						NoResult = false;
						Console.WriteLine("Result requests switched on.");
						Console.WriteLine("");
					// Option: .raw
					} else if (Line.Trim().ToLower() == ".raw=0") {
						DisplayRaw = false;
						Console.WriteLine("Raw XML output switched off.");
						Console.WriteLine("");
					} else if (Line.Trim().ToLower() == ".raw=1") {
						DisplayRaw = true;
						Console.WriteLine("Raw XML output switched on.");
						Console.WriteLine("");
					// Add Input to Query
					} else {
						Query = Query + Line + Environment.NewLine;
					}
					// If last Character in line is > ; < then assume that the query is complete
					if ((Line.Trim().Length > 0) && (Line.Trim() != "") && (Line.Trim().Substring(Line.Length-1, 1) == ";")) {
						Query = Query.TrimEnd('\r', '\n');;
						Result = SQLiteServerConnector.ExecSQL(Query, NoResult);
						// Error?
						if (Result.Error) {
							Console.WriteLine("ERROR: " + Result.ErrorMessage);
						// Ok
						} else {
							// Is minimum one line within the result set?
							if ((Result.RowCount > 0) && (Result.FieldCount > 0)) {
								// Raw XML result is enabled?
								if (DisplayRaw == true) {
									Console.WriteLine("");
									using(var MS = new MemoryStream())
									{
										using (StreamReader SR = new StreamReader(MS, System.Text.Encoding.UTF8)) {
											Result.XML.Save(MS);
											MS.Seek(0, SeekOrigin.Begin);
											Console.WriteLine(SR.ReadToEnd());
										}
									}
									Console.WriteLine("");
								} else {
									Console.WriteLine(
										ArrayPrinter.GetDataInTableFormat(Result.Names, Result.Value)
									);
								}

							// No lines received
							} else {
								Console.WriteLine( (NoResult ? "Ok. (Result requests disabled!)" : "Ok."));
								Console.WriteLine("");
							}
						}
						// Next query....
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
