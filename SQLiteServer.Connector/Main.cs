using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic; // IEnumerate
using System.Linq; // IEnumerate

// Own
using TCP;

namespace SQLiteServer
{
	// Result structure
	public struct SQLiteResult {
		public XDocument XML;			// Raw XML Result
		// Parsed by ParseSQLiteResult:
		public Boolean Error;			// If "true" then an error accoured
		public String ErrorMessage;		// Only valid if Error is "true"
		public int RowCount;			// Number of Rows within XML file
		public int FieldCount;			// Number of Columns/Fields within XML file
		public string[] Names;			// List of Column-Names
		public string[,] Value;			// Values stored as [row,col] array
		public string[,] Type;			// Types stored as [row,col] array
	};

	public class Connector
	{
		private static TCP.Client TCPClient = null;

		// Constructor
		public Connector (string AHost, int APort, string AUsername, string APassword)
		{
			TCPClient = new TCP.Client (AHost, APort, AUsername, APassword);

			// Initial Connect to check if connection to server is possible.
			if (! TCPClient.Connect ()) {
				throw new System.InvalidOperationException ("Cannot connect to Server");
			}
		}

		// Destructor
		~Connector ()
		{
			if (TCPClient != null) TCPClient.Disconnect ();
		}

		// ExecSQL
		public SQLiteResult ExecSQL (string ASQLQuery, Boolean ANoResult = false)
		{
			// Initialize Result
			SQLiteResult Res = new SQLiteResult ();
			Res.XML = new XDocument();
			Res.Error = true;
			Res.ErrorMessage = "Result set is init value";
			Res.FieldCount = 0;
			Res.RowCount = 0;
			Res.Names = new string[0];
			Res.Type = new string[0,0];
			Res.Value = new string[0,0];

			// Send Query to TCP-Client
			try
			{
				// SQL Query -> TCP Client
				if (! TCPClient.Connected ()) {
					if (! TCPClient.Connect()) throw new System.SystemException("Client exception: Cannot connect to Server");
				}
				string ExecResult = TCPClient.ExecSQL (ASQLQuery, ANoResult);

				// Handle Result
				if (ANoResult) {
					Res.Error = false;
					Res.ErrorMessage = "";
				} else {
					Res.XML = XDocument.Parse (ExecResult);
					ParseSQLiteResult (ref Res);
				}

			} catch(Exception e) {
				Res.Error = true;
				Res.ErrorMessage = "Client Exception1: " + e.Message;
			}

			// Return result
			return Res;
		}

		public void ParseSQLiteResult (ref SQLiteResult AResult)
		{
			// Fehlerflag
			try {
				XElement Status = AResult.XML.Root.Element ("Status");
				AResult.Error = Convert.ToBoolean (Status.Attribute ("Error").Value);
				if (AResult.Error) AResult.ErrorMessage = Status.Attribute ("ErrorMessage").Value;
			} catch {
				AResult.Error = true;
				AResult.ErrorMessage = "Client Error: Cannot parse error";
			}
			if (AResult.Error) return;

			// RowCount/FieldCount
			try {
				XElement Status = AResult.XML.Root.Element ("Status");
				AResult.RowCount = Convert.ToInt32 (Status.Attribute ("RowCount").Value);
				AResult.FieldCount = Convert.ToInt32 (Status.Attribute ("FieldCount").Value);
			} catch {
				AResult.Error = true;
				AResult.ErrorMessage = "Client Error: Cannot parse row/field count";
			}
			if (AResult.Error) return;

			// Field Names
			try {
				XElement Fields = AResult.XML.Root.Element("Fields");
				IEnumerable<XElement> Field = from Element in Fields.Elements() 
					select Element;
				
				AResult.Names = new string[AResult.FieldCount];
				for (int i = 0; i<AResult.FieldCount; i++) {
					AResult.Names[i] = Field.ElementAt(i).Attribute("Name").Value;
				}
			} catch {
				AResult.Error = true;
				AResult.ErrorMessage = "Client Error: Cannot parse row/field count";
			}
			if (AResult.Error) return;
			
			// Value/Type Names
			try {
				XElement Rows = AResult.XML.Root.Element("Rows");
				IEnumerable<XElement> Row = from Element in Rows.Elements() 
					select Element;
				IEnumerable<XElement> Cols;

				AResult.Value = new string[AResult.RowCount, AResult.FieldCount];
				AResult.Type = new string[AResult.RowCount, AResult.FieldCount];
				for (int r = 0; r<AResult.RowCount; r++) {
					Cols = from Element in Row.ElementAt(r).Elements() 
						select Element;
					for (int c = 0; c<AResult.FieldCount; c++) {
						AResult.Value[r,c] = Cols.ElementAt(c).Attribute("Value").Value;
						AResult.Type[r,c] = Cols.ElementAt(c).Attribute("Type").Value;
					}
				}
			} catch {
				AResult.Error = true;
				AResult.ErrorMessage = "Client Error: Cannot parse row/field count";
			}
			if (AResult.Error) return;

		}

	}
}

