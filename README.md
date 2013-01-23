SQLiteServer (License: GPLv3)
=============================

TCP Server for remote or local network accesss to a SQLite Database.

**Commandline Parameters:**

```Shell
SQLiteServer.exe
[--dbfile=FILENAME]   filename of Database to serve (default: database.db3)
[--host=HOSTNAME]     listen on Host or IP (default: localhost)
[--port=PORT]         listen on Port (default: 11833)
```


SQLiteServer.Connector (License: LGPLv3)
========================================

C# Client Access Library to connect applications to the SQLiteServer easily.

**Usage example:**

```C#
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
```


SQLiteServer.TestClient (License: GPLv3)
========================================

Example implementation of a Client unsing C# Client Access Library "SQLiteServer.Connector.dll".

**Commandline Parameters:**

```Shell
SQLiteServer.TestClient.exe
[--host=HOSTNAME]     connect to Host or IP (default: localhost)
[--port=PORT]         connect to Port (default: 11833)
```
