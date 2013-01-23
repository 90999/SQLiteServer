// SQL-Verbindung und Rückgabe von Query-Antworten als XML-Dokument
//
// Änderungen:
// 23.01.2013 Tim David Saxen: Erste Version

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic; // List<T>
using Mono.Data.Sqlite;
using System.Xml;
using System.Xml.Linq;

// Eigene
using Tools;

namespace SQLite
{
	public class Client
	{
		private static SqliteConnection Connection;
		private static string DatabaseFile;

		// Constructor
		public Client(string ADatabaseFile)
		{
			// Variablen übernehmen
			DatabaseFile = ADatabaseFile;

			// SQL-Verbindung aufbauen
			Open();
		}

		// Destructor
		~ Client ()
		{
			Close();
		}

		// Open
		private static void Open ()
		{
			if (! File.Exists (DatabaseFile)) {
				SqliteConnection.CreateFile (DatabaseFile);
			}
			Connection = new SqliteConnection ("Data Source=" + DatabaseFile);
			Connection.Open();
		}

		// Close
		private static void Close ()
		{
			Connection.Close();
			Connection = null;
		}

		// ExecuteSQL
		public string ExecuteSQL (string ASQL)
		{
			try
			{
				using (var Cmd = Connection.CreateCommand ()) {
					// SQL-Query ausführen
					Cmd.CommandText = ASQL;
					var Reader = Cmd.ExecuteReader();

					// XML-Dokument erstellen
					XDocument XML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
					XElement XRoot = new XElement("Result");

					// Zeilen in XML-Dokument übernehmen
					int Row = 0;
					XElement XRows = new XElement("Rows");
					XElement XRow = null;
					XElement XField = null;
					XElement XFieldNames = new XElement("Fields");
					XElement XFieldName = null;
					bool first = true;
					while (Reader.Read ())
					{
						if (first) {
							first = false;
							for (int i = 0; i<Reader.FieldCount; i++) {
								XFieldName = new XElement("Col");
								XFieldName.Add(new XAttribute("No", i));
								XFieldName.Add(new XAttribute("Name", Reader.GetName(i)));
								XFieldNames.Add(XFieldName);
							}
						}

						XRow = new XElement("Row");
						XRow.Add(new XAttribute("No", Row));
						for (int i = 0; i<Reader.FieldCount; i++) {
							XField = new XElement("Col");
							XField.Add(new XAttribute("No", i));
							XField.Add(new XAttribute("Type", Reader.GetFieldType(i).Name));
							XField.Add(new XAttribute("Value",  Reader[i].ToString()));
							XRow.Add(XField);
						}
						XRows.Add(XRow);
						Row += 1;
					}
					XRoot.Add(XFieldNames);
					XRoot.Add(XRows);

					// Statusinformatuonen zum Query in XML-Dokument übernehmen
					XElement XStatus = new XElement("Status");
					XStatus.Add(new XAttribute("Error", false));
					XStatus.Add(new XAttribute("FieldCount", Reader.FieldCount.ToString()));
					XStatus.Add(new XAttribute("RowCount", Row.ToString()));
					XRoot.AddFirst(XStatus);

					// XML-Dokument in String wandeln und zurückgeben
					XML.Add(XRoot);
					return XML.Declaration.ToString() + Environment.NewLine + XML.ToString();
				}
			}
			catch(Exception e)
			{
				// XML-Fehler-Dokument erstellen
				XDocument XML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
				XElement XRoot = new XElement("Result");

				// Fehlermeldung in XML-Dokument schreiben
				XElement XStatus = new XElement("Status");
				XStatus.Add(new XAttribute("Error", true));
				XStatus.Add(new XAttribute("ErrorMessage", e.Message.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Trim()));
				XRoot.Add(XStatus);

				// XML-Dokument in String wandeln und zurückgeben
				XML.Add(XRoot);
				return XML.Declaration.ToString() + Environment.NewLine + XML.ToString();
			}

		}
	}
}

