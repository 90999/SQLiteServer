unit UnitTest5;

// -----------------------------------------------------------------------------
//
// Description: Check Transaction and Rollback
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

function TestCase5(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase5(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  SQL: String;
  I: Integer;
begin
  Result := TRUE;

  // Create random table name
  Table := 'Test5_Table_' + UnitToolsProject.RandomString(10, ['a'..'z','A'..'Z','0'..'9']);

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

  // INSERT (WITH COMMIT)
  if (Result = TRUE) then begin
    SQL := 'BEGIN TRANSACTION;' +
           'INSERT INTO ' + Table + ' (I) VALUES (1);' +
           'INSERT INTO ' + Table + ' (I) VALUES (2);' +
           'COMMIT;';
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

  // INSERT (WITH ROLLBACK)
  if (Result = TRUE) then begin
    SQL := 'BEGIN TRANSACTION;' +
           'INSERT INTO ' + Table + ' (I) VALUES (3);' +
           'ROLLBACK;';
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
    SQL := 'SELECT I FROM ' + Table;
    if ADebug then Log('SQL: ' + SQL);
    SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
    try
      if (SQLiteResult.Error) then begin
        if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
        Result := FALSE;
      end else begin
        if (SQLiteResult.RowCount <> 2) then Result := FALSE;
        if (SQLiteResult.FieldCount <> 1) then Result := FALSE;
        if (Result) then begin
          if (SQLiteResult.Value[0,0] <> '1') then begin
            Result := FALSE;
          end;
          if (SQLiteResult.Value[1,0] <> '2') then begin
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

