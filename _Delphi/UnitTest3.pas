unit UnitTest3;

// -----------------------------------------------------------------------------
//
// Description: Write UTF-8 and read back
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

function TestCase3(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase3(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
const
  COUNT = 500;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  Field: String;
  Data: String;
  SQL: String;
  I: Integer;
  Rows: Array[1..COUNT] of String;
begin
  Result := TRUE;

  // Create random table name with umlauts
  Table := 'Test3_Table_' + UnitToolsProject.RandomString(10, ['ä','ö','ü','ß']);
  Field := 'Test3_Field_' + UnitToolsProject.RandomString(10, ['ä','ö','ü','ß']);
  Data := 'Test3_Data_' + UnitToolsProject.RandomString(10, ['ä','ö','ü','ß']);

  // CREATE
  SQL := 'CREATE TABLE "' + Table + '" ("' + Field + '" TEXT)';
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
  if (Result) then begin
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
  end;

  // INSERT
  if (Result = TRUE) then begin
    SQL := 'INSERT INTO ' + Table + ' ("' + Field + '") VALUES ("' + Data + '")';
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

  // SELECT
  if (Result) then begin
    SQL := 'SELECT "' + Field + '" FROM "' + Table + '"';
    if ADebug then Log('SQL: ' + SQL);
    SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
    try
      if (SQLiteResult.Error) then begin
        if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
        Result := FALSE;
      end else begin
        if (SQLiteResult.RowCount <> 1) then Result := FALSE;
        if (SQLiteResult.FieldCount <> 1) then Result := FALSE;
        if (Result) and (SQLiteResult.Names[0] <> Field) then Result := FALSE;
        if (Result) then begin
          if (SQLiteResult.Value[0,0] <> Data) then begin
            Result := FALSE;
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
  SQL := 'DROP TABLE "' + Table + '"';
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

