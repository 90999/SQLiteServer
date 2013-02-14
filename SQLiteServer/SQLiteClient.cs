using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic; // List<T>
using Mono.Data.Sqlite;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

// Own
using Tools;

namespace SQLiteServer
{
	public class SQLiteClient
	{
		private static Dictionary<string, SqliteConnection> Connections = new Dictionary<string, SqliteConnection>();
		private static string DatabaseFile;
		private static string[] AccessRights = { "rw", "ro" };

		// Constructor
		public SQLiteClient(string ADatabaseFile)
		{
			DatabaseFile = ADatabaseFile;

            // Open Database
			Open();
        }

		// Destructor
        ~ SQLiteClient ()
		{
            // Close Database
            Close();
		}

		// Open
		private static void Open ()
		{
			if (! File.Exists (DatabaseFile)) {
				SqliteConnection.CreateFile (DatabaseFile);
			}

			foreach (string AccessRight in AccessRights) {
				Connections [AccessRight] = new SqliteConnection (
					"Data Source=" + DatabaseFile +
					";Version=3" +
					";encoding=UTF-8" +
					(AccessRight == "ro" ? ";Read Only=True" : "")
				);
				Connections [AccessRight].Open ();

				// Initial Connection Querys
				string[] InitSQL =  {};
				if (AccessRight == "ro") {
					InitSQL = new string[] {
						"PRAGMA encoding='UTF-8';"
					};
				} else if (AccessRight == "rw") {
					InitSQL = new string[] {
						"PRAGMA encoding='UTF-8';"
					};
				}
				foreach (string Query in InitSQL) {
					using (var Cmd = Connections[AccessRight].CreateCommand ()) {
						Cmd.CommandText = Query;
						Cmd.ExecuteNonQuery ();
					}
				}
			}
		}

		// Close
		private static void Close ()
		{
			foreach (string AccessRight in AccessRights) {
				Connections[AccessRight].Close ();
				Connections[AccessRight] = null;
			}
		}

		// ExecuteSQL
		public void ExecuteSQL (string ASQL, string AccessRight, ref string AResult, Boolean ANoResult = false)
		{
			try
			{
				AResult = String.Empty;

				// Check what SQLite connection to use (ro or rw or throw exception)
				if (! AccessRights.Contains(AccessRight)) {
					throw new System.UnauthorizedAccessException("Not authorized");
				}

				// Execute SQL Query
				using (var Cmd = Connections[AccessRight].CreateCommand ()) {

					// Query without result request by client
					if (ANoResult) {
						// Execute SQL-Query 
						Cmd.CommandText = ASQL;
						Cmd.ExecuteNonQuery();
						return;

					// Query WITH result request by client
					} else {
						// Execute SQL-Query 
						Cmd.CommandText = ASQL;
						var Reader = Cmd.ExecuteReader();

						// Create XML-Document
						XDocument XML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
						XElement XRoot = new XElement("Result");

						// Add Result
						int Row = 0;
						XElement XRows = new XElement("Rows");
						XElement XRow = null;
						XElement XField = null;
						XElement XFieldNames = new XElement("Fields");
						XElement XFieldName = null;
						bool first = true;
						while (Reader.Read ())
						{
							// On First element add Field/Column Information
							if (first) {
								first = false;
								for (int i = 0; i<Reader.FieldCount; i++) {
									XFieldName = new XElement("Col");
									XFieldName.Add(new XAttribute("No", i));
									XFieldName.Add(new XAttribute("Name", Reader.GetName(i)));
									XFieldNames.Add(XFieldName);
								}
							}

							// Add Row
							XRow = new XElement("Row");
							XRow.Add(new XAttribute("No", Row));
							for (int i = 0; i<Reader.FieldCount; i++) {
								XField = new XElement("Col");
								XField.Add(new XAttribute("No", i));
								XField.Add(new XAttribute("Type", Reader.GetFieldType(i).Name));

                                // BLOB
                                if (Reader.GetFieldType(i).Name.ToUpper() == "BYTE[]") {

                                    byte[] BlobBytes = (byte[])Reader[i];
                                    StringBuilder BlobStr = new StringBuilder(BlobBytes.Length * 2);
                                    foreach (byte B in BlobBytes) {
                                        BlobStr.AppendFormat("{0:X2}", B);
                                    }
                                    XField.Add(new XAttribute("Value",  "X'" + BlobStr.ToString() + "'"));

                                // Others
                                } else {
								  XField.Add(new XAttribute("Value",  Reader[i].ToString()));
                                }

								XRow.Add(XField);
							}
							XRows.Add(XRow);
							Row += 1;
						}
						XRoot.Add(XFieldNames);
						XRoot.Add(XRows);

						// Add Status/Error and Query Information
						XElement XStatus = new XElement("Status");
						XStatus.Add(new XAttribute("Error", false));
						XStatus.Add(new XAttribute("FieldCount", Reader.FieldCount.ToString()));
						XStatus.Add(new XAttribute("RowCount", Row.ToString()));
						XRoot.AddFirst(XStatus);

						// Return XML-Document as String
						XML.Add(XRoot);
						using(var MS = new MemoryStream())
						{
							using (StreamReader SR = new StreamReader(MS, System.Text.Encoding.UTF8)) {
								XML.Save(MS);
								MS.Seek(0, SeekOrigin.Begin);
								AResult = SR.ReadToEnd();
								return;
							}
						}
					}

				}
			}
			catch(Exception e)
			{
				// Create XML-Document
				XDocument XML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
				XElement XRoot = new XElement("Result");

				// Add Error Message to XML-Document
				XElement XStatus = new XElement("Status");
				XStatus.Add(new XAttribute("Error", true));
				XStatus.Add(new XAttribute("ErrorMessage", e.Message.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Trim()));
				XRoot.Add(XStatus);

				// Return XML-Document as String
				XML.Add(XRoot);
				using(var MS = new MemoryStream())
				{
					using (StreamReader SR = new StreamReader(MS, System.Text.Encoding.UTF8)) {
						XML.Save(MS);
						MS.Seek(0, SeekOrigin.Begin);
						AResult = SR.ReadToEnd();
						return;
					}
				}
			}


        }


    }
}

