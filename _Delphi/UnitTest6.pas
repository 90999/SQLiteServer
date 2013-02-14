unit UnitTest6;

// -----------------------------------------------------------------------------
//
// Descrition: Measure INSERTS with RESULT (~ 10sec)
//
// Changes:
// 14.02.2012 Tim David Saxen: First Release
//
// -----------------------------------------------------------------------------

interface

uses
  // Own
  UnitToolsProject,
  // SQL Connector
  SQLiteServer.Connector,
  // Delphi
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants,
  System.Classes, Vcl.Graphics, Vcl.Controls, Vcl.Forms, Vcl.Dialogs,
  Vcl.StdCtrls;

function TestCase6(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase6(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  SQL: String;
  I: Integer;
  StartTime: Cardinal;
  Count: Integer;
begin
  Result := TRUE;

  // Create random table name
  Table := 'Test6_Table_' + UnitToolsProject.RandomString(10, ['a'..'z','A'..'Z','0'..'9']);

  // CREATE
  SQL := 'CREATE TABLE ' + Table + ' (I INTEGER)';
  if ADebug then Log('SQL: ' + SQL);
  SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
  try
    if (SQLiteResult.Error) then begin
      if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
      Result := FALSE;
    end;
  finally
    ASQLiteServerConnector.FreeResult(SQLiteResult);
  end;

  // INSERT (WITH RESULT)
  StartTime := WinApi.Windows.GetTickCount;
  Count := 0;
  while (Result = TRUE) and (WinApi.Windows.GetTickCount-StartTime<10000) do begin
    SQL := 'INSERT INTO ' + Table + ' (I) VALUES (' + IntToStr(Count) + ')';
    if ADebug then Log('SQL: ' + SQL);
    SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
    try
      if (SQLiteResult.Error) then begin
        if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
        Result := FALSE;
      end else begin
        Count := Count + 1;
      end;
    finally
      ASQLiteServerConnector.FreeResult(SQLiteResult);
    end;
  end;
  if (Count > 0) then begin
    if ADebug then Log('' + IntToStr(Count) + ' INSERTs with result within ' + IntToStr(WinApi.Windows.GetTickCount-StartTime) + 'ms.');
  end;

  // DROP
  SQL := 'DROP TABLE ' + Table;
  if ADebug then Log('SQL: ' + SQL);
  SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
  try
    if (SQLiteResult.Error) then begin
      if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
      Result := FALSE;
    end;
  finally
    ASQLiteServerConnector.FreeResult(SQLiteResult);
  end;
end;

end.

