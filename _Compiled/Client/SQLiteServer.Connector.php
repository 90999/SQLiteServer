<?php

// SQLiteServer Connector Class for PHP

class SQLiteServerConnector {

	private $host = "localhost";
	private $port = 11833;
	private $user = "Admin";
	private $pass = "Admin";

	private $socket = NULL;

	// Constructor
	public function __construct($host, $port) {
		$this->host = $host;
		$this->port = $port;
	}

	//  Destructor
	function __destruct() {
		//
	}

	// Connect
	private function Connect() {
		try {

			// Create socket
			$this->socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
			if ($this->socket === false) {
				throw new Exception("Cannot create socket");
			}
			socket_set_option($this->socket, SOL_SOCKET, SO_RCVTIMEO, array("sec" => 5, "usec" =>0 ));
      
			$result = socket_connect($this->socket, $this->host, $this->port);
			if ($result === false) {
				throw new Exception("Cannot connect to remote host");
			}

			// Wait for greeting
			$result = socket_read($this->socket,2048);
			if ($result === false) {
				throw new Exception("Cannot read remote greeting");
			} else {
				$result = rtrim($result, " \r\n");
				if ($result != "SQLiteServer v1.0") {
					throw new Exception("Wrong Server version detected: " . $result);
				}
			}

			// Send login
			$AuthStr = "USER:" . $this->user . ":" . $this->pass . "\n";
			socket_write($this->socket, $AuthStr, strlen($AuthStr));

			// Waiting for login response
			$result = socket_read($this->socket,2048);
			if ($result === false) {
				throw new Exception("Cannot write to remote server");
			} else {
				$result = rtrim($result, " \r\n");
				if (($result != "RIGHTS:ro") && ($result != "RIGHTS:rw")) {
					throw new Exception("Wrong permissions detected");
				}
			}

			return true;
		} catch (Exception $e) {
			$this->Disconnect();
			return false;
		}
	}

	// Disconnect
	private function Disconnect() {
		if (! is_null($this->socket)) {
			try {
				socket_shutdown($this->socket, 2); // 2=in+out
				socket_close($this->socket);
			} catch (Exception $e) {
			}
			$this->socket = NULL;
		}
	}

	// Connected
	private function Connected() {
		if ($this->socket == NULL) {
			return false;
		} else {
			return true;
		}
	}

	// ExecSQL
	private function Send($ASQLQuery, $ANoResult = false) {
		try {
			if (! $this->Connected()) {
				if (! $this->Connect()) {
					throw new Exception("Cannot connect to remote host");
				}
			}

			// Send REQUEST
			$SQLQueryArr = preg_split("/(\r\n|\n|\r)/", $ASQLQuery);
			$AuthStr = "REQUEST:" . count($SQLQueryArr) . ":" . ($ANoResult == true ? "1" : "0") . "\n";
			$sent = socket_write($this->socket, $AuthStr, strlen($AuthStr));
			if ($sent === false) {
				throw new Exception("Cannot write to socket");
			}
			for ($i = 0; $i<count($SQLQueryArr); $i++) {
				$SQLQueryArr[$i] = "." . $SQLQueryArr[$i] . "\n";
				$sent = socket_write($this->socket, $SQLQueryArr[$i], strlen($SQLQueryArr[$i]));
				if ($sent === false) {
					throw new Exception("Cannot write to socket");
				}
			}
			$SQLQueryArr = [];

			// Read
			if ($ANoResult == false) {
				$Buffer = "";
				$Count = -1;
				$RecvRaw = "";
				$RecvStr = "";
				$RecvLine = "";
				$RecvDone = false;
				while (true) {

					$RecvRaw = socket_read($this->socket, 2048);
					if ($RecvRaw === false) {
						throw new Exception("Cannot read from remote server"); // leave while(true)
					} else {
						$Buffer .= $RecvRaw;
						while (($PosNewLine = strpos($Buffer, "\n")) !== false) {
							$RecvLine = rtrim(substr($Buffer, 0, $PosNewLine), "\r\n");
							$Buffer = substr($Buffer, $PosNewLine+1, strlen($Buffer));
              
							if (trim($RecvLine) == "") {
								// Nothing to do
							} else {
								$RecvLineArr = explode(":", $RecvLine);

								// RESULT
								if ((count($RecvLineArr)>=2) && (strtoupper($RecvLineArr[0]) == "RESULT")) {
									$Count = $RecvLineArr[1]+0;
									$RecvDone = false;
								// .
								} else if (($RecvDone == false) && ($Count>0) && (substr($RecvLine,0,1) == ".")) {
									$Count -= 1;
									$RecvStr = $RecvStr . substr($RecvLine, 1, strlen($RecvLine)) . "\n";
									if ($Count == 0) {
										$RecvStr = rtrim($RecvStr, "\r\n");
										$RecvDone = true;
										$Count = -1;
									}
								}

								if ($RecvDone == true) {
									$doc = new DomDocument("1.0", "UTF-8");
									$doc->loadXML($RecvStr);
									return $doc; // leave while(true)
								}
              
							}
						}
					}
				}
			}
      
			return "";
      
		} catch (Exception $e) {
			$this->Disconnect();

			$doc = new DomDocument("1.0", "UTF-8");
			$doc->xmlStandalone = true;
			$doc->preserveWhiteSpace = false;
			$doc->formatOutput = true;
			$result = $doc->createElement("Result");
  
			$status = $doc->createElement("Status");
			$status->setAttribute("Error", "true");
			$status->setAttribute("ErrorMessage", $e->getMessage());
			$result->appendChild($status);
  
			$doc->appendChild($result);

			return $doc;
		}
	}

	public function ParseSQLiteResult(&$AResult) {
		// Error flag
		try {
			$Status = $AResult->XML->getElementsByTagName("Status")->item(0);
			$AResult->Error = $Status->getAttribute("Error") !== "false";
			if ($AResult->Error) {
				$AResult->ErrorMessage = $Status->getAttribute("ErrorMessage");
			}
		} catch(Exception $e) {
			$AResult->Error = true;
			$AResult->ErrorMessage = "Client Error: Cannot parse error";
		}
		if ($AResult->Error) return;

		// RowCount/FieldCount
		try {
			$Status = $AResult->XML->getElementsByTagName("Status")->item(0);
			$AResult->RowCount = $Status->getAttribute("RowCount");
			$AResult->FieldCount = $Status->getAttribute("FieldCount");
		} catch(Exception $e) {
			$AResult->Error = true;
			$AResult->ErrorMessage = "Client Error: Cannot parse row/field count";
		}
		if ($AResult->Error) return;

		// Field Names
		try {
			$Fields = $AResult->XML->getElementsByTagName("Fields")->item(0);
			$FieldArr = $Fields->getElementsByTagName("Col");
			foreach ($FieldArr as $Field) {
				array_push($AResult->Names, $Field->getAttribute("Name"));
			}
		} catch(Exception $e) {
			$AResult->Error = true;
			$AResult->ErrorMessage = "Client Error: Cannot parse row/field count";
		}
		if ($AResult->Error) return;
			
		// Value/Type Names
		try {
			$Rows = $AResult->XML->getElementsByTagName("Rows")->item(0);
			$RowArr = $Rows->getElementsByTagName("Row");
			$r = -1;
			foreach ($RowArr as $Row) {
				$r += 1;
				$c = -1;
				$ColArr = $Row->getElementsByTagName("Col");
				foreach ($ColArr as $Col) {
					$c += 1;
					$AResult->Type[$r][$c] = $Col->getAttribute("Type");
					$AResult->Value[$r][$c] = $Col->getAttribute("Value");
				}
			}
		} catch(Exception $e) {
			$AResult->Error = true;
			$AResult->ErrorMessage = "Client Error: Cannot parse row/field count";
		}
		if ($AResult->Error) return;
	}

	public function ExecSQL($ASQLQuery, $ANoResult = false) {
		$OldEncoding = mb_internal_encoding(); 
		mb_internal_encoding('UTF-8'); 

		// Init result
		$Result = new stdClass;
		$Result->XML = new DomDocument("1.0", "UTF-8");;
		$Result->Error = true;
		$Result->ErrorMessage = "Result set is init value";
		$Result->RowCount = 0;
		$Result->FieldCount = 0;
		$Result->Names = array();
		$Result->Value = array(array());;
		$Result->Type = array(array());;

		$Result->XML = $this->Send($ASQLQuery, $ANoResult);
		if ($ANoResult == true) {
			$Result->Error = false;
			$Result->ErrorMessage = "";
		} else {
			$this->ParseSQLiteResult($Result);
		}

		mb_internal_encoding($OldEncoding); 
		return $Result;
	}
}

/*

	// Example implementation for testing purposes

	$Connector = new SQLiteServerConnector(
		"localhost",
		11833,
		"Admin",
		"Admin"
	);
	$Result = $Connector->ExecSQL(
		"SELECT 1;",
		false
	);

	if ($Result->Error) {
		print "ERROR: $Result->ErrorMessage\n";
	} else {
		print "Rows: $Result->RowCount\n";
		print "Fields: $Result->FieldCount\n";
		foreach ($Result->Names as $name) {
			print "  * Field: $name\n";
		}
		for ($r = 0; $r<$Result->RowCount; $r++) {
			print "Row: $r\n";
			for ($c = 0; $c<$Result->FieldCount; $c++) {
				print	"  * Row: $r " .
					"Col: $c " .
					"Name: " . $Result->Names[$c] . " " .
					"Type: " . ($Result->Type[$r][$c]) . " " .
					"Value: " . ($Result->Value[$r][$c]) . "\n";
			}
		}
	}

	$Connector = NULL;

*/

?>