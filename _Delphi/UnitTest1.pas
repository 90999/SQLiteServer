unit UnitTest1;

// -----------------------------------------------------------------------------
//
// Descrition: Create table and verify then drop table and verify
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

function TestCase1(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase1(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  SQL: String;
begin
  Result := TRUE;

  // Create random table name
  Table := 'Test1_Table_' + UnitToolsProject.RandomString(10, ['a'..'z','A'..'Z','0'..'9']);

  // CREATE
  SQL := 'CREATE TABLE ' + Table + ' (T TEXT, I INTEGER, R REAL, B BLOB)';
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

  // CHECK CREATE
  SQL := 'SELECT name FROM sqlite_master WHERE type="table" AND name="' + Table + '"';
  if ADebug then Log('SQL: ' + SQL);
  SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
  try
    if (SQLiteResult.Error) then begin
      if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
      Result := FALSE;
      exit;
    end else if (SQLiteResult.RowCount <> 1) or (SQLiteResult.FieldCount <> 1) or (SQLiteResult.Value[0,0] <> Table) then begin
      if ADebug then LogSQLResult('ERROR: Got wrong result', SQLiteResult);
      Result := FALSE;
    end;
  finally
    ASQLiteServerConnector.FreeResult(SQLiteResult);
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

  // CHECK DROP
  SQL := 'SELECT name FROM sqlite_master WHERE type="table" AND name="' + Table + '"';
  if ADebug then Log('SQL: ' + SQL);
  SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
  try
    if (SQLiteResult.Error) then begin
      if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
      Result := FALSE;
      exit;
    end else if (SQLiteResult.RowCount <> 0) then begin
      if ADebug then LogSQLResult('ERROR: Got wrong result', SQLiteResult);
      Result := FALSE;
    end;
  finally
    ASQLiteServerConnector.FreeResult(SQLiteResult);
  end;
end;

end.
