unit UnitTest4;

// -----------------------------------------------------------------------------
//
// Description: Check Datatypes: BLOB, REAL, INTEGER and TEXT
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

function TestCase4(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;

implementation

function TestCase4(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
var
  SQLiteResult: TSQLiteServerResult;
  Table: String;
  SQL: String;
  I: Integer;
  Blob: TBytes;
  BlobEscaped: String;
  VarInt: Int64;
  VarReal: Single;
  VarString: String;
begin
  Result := TRUE;

  // Create random table name
  Table := 'Test4_Table_' + UnitToolsProject.RandomString(10, ['a'..'z','A'..'Z','0'..'9']);

  // Create string values from Variables
  SetLength(Blob, 0);
  try
    for I := 0 to 10 do begin
      SetLength(Blob, Length(Blob)+1);
      Blob[Length(Blob)-1] := I;
    end;
    BlobEscaped := ASQLiteServerConnector.EscapeAndQuote(Blob);
  finally
    SetLength(Blob, 0);
  end;

  VarInt := 1234567890;
  VarReal := 0.1234567;
  VarString := 'This is a test String';

  // CREATE
  SQL := 'CREATE TABLE ' + Table + ' (I INTEGER, R REAL, T TEXT, B BLOB)';
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
    SQL := 'INSERT INTO ' + Table + ' (I, B, R, T) VALUES (' +
      ASQLiteServerConnector.EscapeAndQuote(VarInt) + ',' +
      BlobEscaped + ',' +
      ASQLiteServerConnector.EscapeAndQuote(VarReal) + ',' +
      ASQLiteServerConnector.EscapeAndQuote(VarString) +
    ')';
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
    SQL := 'SELECT I, B, R, T FROM ' + Table;
    if ADebug then Log('SQL: ' + SQL);
    SQLiteResult := ASQLiteServerConnector.ExecSQL(SQL, FALSE);
    try
      if (SQLiteResult.Error) then begin
        if ADebug then Log('SQL ERROR: ' + SQLiteResult.ErrorMessage);
        Result := FALSE;
      end else begin
        if (SQLiteResult.RowCount <> 1) then Result := FALSE;
        if (SQLiteResult.FieldCount <> 4) then Result := FALSE;
        if (Result) then begin
          if (SQLiteResult.Value[0,0] <> '1234567890') then begin
            Result := FALSE;
          end;
          if (SQLiteResult.Value[0,1] <> BlobEscaped) then begin
            Result := FALSE;
          end;
          if (SQLiteResult.Value[0,2] <> '0.1234567') then begin
            Result := FALSE;
          end;
          if (SQLiteResult.Value[0,3] <> VarString) then begin
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

