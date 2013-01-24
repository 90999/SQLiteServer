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

**Users are stored within "users.txt" with the following format:**

```text
rw:Username1:Password1
ro:Username2:Password2
```

*ro* describes an read-only user
*rw* describes an read-write user


C# SQLiteServer.Connector (License: LGPLv3)
===========================================

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
       	    SQLiteServerConnector = new SQLiteServer.Connector (
       	    	"localhost",	// Remote Host
       	    	11833,			// Remote Port
       	    	"Admin",		// Username
       	    	"Admin"			// Password
       	    );
			SQLiteServer.SQLiteResult Result = SQLiteServerConnector.ExecSQL(
				"SELECT 1",		// Query
				false			// NoResult = true
			);
       	    // XDocument        Result.XML (raw data from server)
			// bool             Result.Error
            // string           Result.ErrorMessage
			// string [col]     Result.Names
			// string [row,col] Result.Value
			// string [row,col] Result.Type
			// int              Result.FieldCount
			// int              Result.RowCount
		}
	}
}
```


C# SQLiteServer.Connector (License: LGPLv3)
===========================================

PHP Client Access Class to connect applications to the SQLiteServer easily.

**Usage example:**

```PHP
  require_once("SQLiteServer.Connector.php");

  $obj = new SQLiteServerConnector(
  	"localhost",	// remote hostname
  	11833,			// remote port
  	"Admin",		// username
  	"Admin"			// password
  );
  $Result = $obj->ExecSQL(
  	"SELECT 1;",	// query
  	false			// false = with result
  );

  // DomDocument	$Result->XML 
  // bool			$Result->Error 
  // string			$Result->ErrorMessage 
  // int			$Result->RowCount 
  // int			$Result->FieldCount 
  // array()		$Result->Names 
  // array(array())	$Result->Value 
  // array(array())	$Result->Type 
```

**Requipments:**

The following PHP extension must be enabled to use this class: .

```Text
extension=php_mbstring.dll
extension=php_sockets.dll
```

SQLiteServer.TestClient (License: GPLv3)
========================================

Example implementation of a Client unsing C# Client Access Library "SQLiteServer.Connector.dll".

**Commandline Parameters:**

```Shell
SQLiteServer.TestClient.exe
[--host=HOSTNAME]     connect to Host or IP (default: localhost)
[--port=PORT]         connect to Port (default: 11833)
[--user=USER]         connection Username (default: Admin)
[--pass=PASS]         connection Password (default: Admin)
```
