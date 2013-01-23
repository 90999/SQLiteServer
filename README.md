SQLiteServer (1.0 BETA RELEASE - License: GPL)
==============================================

Currently still experimental TCP Server for SQLite accesss.

Commandline Parameters
----------------------

[--host HOSTNAME]     listen on Host or IP (default: localhost)
[--port PORT]         listen on Port (default: 11833)


SQLiteServer.Connector (License: LGPL)
======================================

C# Client Access Library to connect applications to the SQLiteServer easily.

Usage example
-------------

	using SQLiteServer;
	namespace Test
	{
	    class TestClass
    	{
	        public void Main (string[] args)
    	    {
        	    SQLiteServerConnector = new SQLiteServer.Connector ();
				SQLiteServer.SQLiteResult Result = SQLiteServerConnector.ExecSQL("SELECT 1");
	            // bool             Result.Error
    	        // string           Result.ErrorMessage
        	    // XDocument        Result.XML
	            // string [col]     Result.Names
    	        // string [row,col] Result.Value
        	    // string [row,col] Result.Type
	            // int              Result.FieldCount
    	        // int              Result.RowCount
	        }
	    }
	}


SQLiteServer.TestClient (License: GPL)
======================================

Example implementation of a Client unsing C# Client Access Library "SQLiteServer.Connector.dll".

Commandline Parameters
----------------------

[--host HOSTNAME]     listen on Host or IP (default: localhost)
[--port PORT]         listen on Port (default: 11833)
