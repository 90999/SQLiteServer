Download: [ZIP Archive](SQLiteServer/zipball/master)

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


C#/.NET SQLiteServer.Connector.dll (License: LGPLv3)
====================================================

C#/.NET Client Access Library to connect applications to the SQLiteServer easily.

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


PHP SQLiteServer.Connector.php (License: LGPLv3)
================================================

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

The following PHP extension must be enabled to use this class: 

```Text
extension=php_mbstring.dll
extension=php_sockets.dll
```


Perl SQLiteServerConnector.pm (License: LGPLv3)
==============================================

Perl Client Access Module to connect applications to the SQLiteServer easily.

**Usage example:**

```Perl
	use SQLiteServerConnector;

	my $SQLiteServerConnector = new SQLiteServerConnector(
		host => "localhost",	// remote host
		port => 11833,			// remote port
		user => "Admin",		// username
		pass => "Admin"			// password
	);

	$Result = $SQLiteServerConnector->ExecSQL(
		"SELECT 1",				// query
		false					// false = with result
	);

	// XML::LibXML::Document	$Result->{XML}
	// $Result->{Error}				int 0 = false | 1 = true/error
	// $Result->{ErrorMessage} 		string
	// $Result->{RowCount}			int
	// $Result->{FieldCount}		int
	// $Result->{Names}[col]		array
	// $Result->{Value}[row,col]	array
	// $Result->{Type}[row,col]		array
```

**Requipments:**

The following Perl packages must be installed to use this module:

```Text
IO::Socket
XML::LibXML
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
