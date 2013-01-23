using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
	}

	public class Connector
	{
		private static TCP.Client TCPClient = null;

		// Connect
		public Boolean Connect (string AHost, int APort)
		{
			if (TCPClient != null) {
				TCPClient.Disconnect ();
			}
			TCPClient = new TCP.Client();
			return TCPClient.Connect (AHost, APort);
		}
		
		// Connected
		public Boolean Connected ()
		{
			if (TCPClient == null) return false;
			return TCPClient.Connected ();
		}
		
		// Disconnect
		public Boolean Disconnect ()
		{
			if (TCPClient == null) return false;
			Boolean res = TCPClient.Disconnect ();
			TCPClient = null;
			return res;
		}
		
		// ExecSQL
		public SQLiteResult ExecSQL (string ASQLQuery)
		{
			if (! Connected ()) {
				throw new System.InvalidOperationException ("Not Connected");
			}

			SQLiteResult Res = new SQLiteResult ();

			string ExecResult = TCPClient.ExecSQL (ASQLQuery);
			Res.XML = XDocument.Parse (ExecResult);
			ParseSQLiteResult (ref Res);
			//Res.Error = false;
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

