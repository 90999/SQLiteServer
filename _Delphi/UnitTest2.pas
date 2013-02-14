unit UnitTest2;

// -----------------------------------------------------------------------------
//
// Description: Insert 500 random rows and verify
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

function TestCase2(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase2(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
const
  COUNT = 500;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  SQL: String;
  I: Integer;
  Rows: Array[1..COUNT] of String;
begin
  Result := TRUE;

  // Create random table name
  Table := 'Test2_Table_' + UnitToolsProject.RandomString(10, ['a'..'z','A'..'Z','0'..'9']);

  // CREATE
  SQL := 'CREATE TABLE ' + Table + ' (T TEXT)';
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

  // INSERT
  if (Result = TRUE) then begin
    SQL := 'INSERT INTO ' + Table + ' VALUES ';
    for I := 1 to COUNT do begin
      Rows[I] := RandomString(20, ['a'..'z','A'..'Z','0'..'9']);
      SQL := SQL + '("' + Rows[I] + '"),';
    end;
    Delete(SQL, Length(SQL), 1); // Remove trailing ,
    if ADebug then Log('SQL: INSERT INTO ' + Table + ' VALUES (".."), .., ("..")');
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

  // SELECT
  if (Result) then begin
    SQL := 'SELECT T FROM ' + Table;
    if ADebug then Log('SQL: ' + SQL);
    SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
    try
      if (SQLiteResult.Error) then begin
        if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
        Result := FALSE;
      end else begin
        if (SQLiteResult.RowCount <> COUNT) then Result := FALSE;
        if (SQLiteResult.FieldCount <> 1) then Result := FALSE;
        if (Result) and (SQLiteResult.Names[0] <> 'T') then Result := FALSE;
        if (Result) then begin
          for I := 1 to COUNT do begin
            if (SQLiteResult.Value[I-1,0] <> Rows[I]) then begin
              Result := FALSE;
              break;
            end;
          end;
        end;
        if (not Result) then begin
          if ADebug then LogSQLResult('ERROR: Got wrong result', SQLiteResult);
        end;
      end;
    finally
      ASQLiteServerConnector.FreeResult(SQLiteResult);
    end;
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

