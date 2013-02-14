unit UnitToolsProject;

interface

uses
  // SQL Connector
  SQLiteServer.Connector,
  // Delphi
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants,
  System.Classes, Vcl.Graphics, Vcl.Controls, Vcl.Forms, Vcl.Dialogs,
  Vcl.StdCtrls;

type
  TSetOfChar = set of WideChar;

  TTestCaseProc = function(ASQLiteServerConnector: TSQLiteServerConnector; ADebug: Boolean): Boolean;
  TTestCase = record
    Name: String;
    Proc: TTestCaseProc;
  end;
  TTestCaseArr = array of TTestCase;

  TConnectionData = record
    User: String;
    Pass: String;
    Host: String;
    Port: Word;
  end;

var
  LogList: TListBox = nil;


procedure Log(ALine: String);
function RandomString(ALen: Integer; const AChars: TSetOfChar): string;
// procedure Query(var ASQLiteServerConnector: TSQLiteServerConnector; ASQL: String; ANoResult: Boolean);
function ResultToStr(AValue: Boolean): String;
procedure AddTestCaseToArr(var ATestCaseArr: TTestCaseArr; AName: String; AProc: TTestCaseProc);
procedure LogSQLResult(AHeader: String; var ASQLiteResult: TSQLiteServerResult);

implementation

procedure Log(ALine: String);
begin
  if LogList = nil then exit;

  LogList.Items.Add(ALine);
  LogList.ItemIndex := LogList.Items.Count-1;
end;

function RandomString(ALen: Integer; const AChars: TSetOfChar): string;
var
  I: Integer;
  C: WideChar;
  A: Array of WideChar;
begin
  Result := '';
  SetLength(A, 0);
  try
    for C in AChars do begin
      SetLength(A, Length(A)+1);
      A[Length(A)-1] := C;
    end;
    for I := 1 to ALen do Result := Result + A[Random(Length(A))];
  finally
    SetLength(A, 0);
  end;
end;

(*
procedure Query(var ASQLiteServerConnector: TSQLiteServerConnector; ASQL: String; ANoResult: Boolean);
var
  SQLiteResult: TSQLiteServerResult;
  R, C: Integer;
begin
  Log('SQL: ' + ASQL);
  SQLiteResult := SQLiteServerConnector.ExecSQL(ASQL, ANoResult);
  if (SQLiteResult.Error) then begin
    Log('ERROR: ' + SQLiteResult.ErrorMessage);
  end else begin
    if ANoResult then begin
      Log('Ok. (No result requested!)');
    end else begin
      Log('Status');
      Log('  * Rows: ' + IntToStr(SQLiteResult.RowCount));
      Log('  * Cols: ' + IntToStr(SQLiteResult.FieldCount));
      for R := 0 to SQLiteResult.RowCount-1 do begin
        Log('Row: ' + IntToStr(R));
        for C := 0 to SQLiteResult.FieldCount-1 do begin
          Log(
            '  * ' +
            'Col: ' + IntToStr(C) + ' ' +
            'Name: ' + SQLiteResult.Names[C] + ' ' +
            'Type: ' + SQLiteResult.ValueType[R][C] + ' ' +
            'Value: ' + SQLiteResult.Value[R][C]
          );
        end;
      end;
    end;
  end;
  SQLiteServerConnector.FreeResult(SQLiteResult);
end;
*)

function ResultToStr(AValue: Boolean): String;
begin
  if AValue then Result := 'OK' else Result := 'FAILED';
end;

procedure AddTestCaseToArr(var ATestCaseArr: TTestCaseArr; AName: String; AProc: TTestCaseProc);
begin
  SetLength(ATestCaseArr, Length(ATestCaseArr)+1);
  with ATestCaseArr[Length(ATestCaseArr)-1] do begin
    Name := AName;
    Proc := AProc;
  end;
end;

procedure LogSQLResult(AHeader: String; var ASQLiteResult: TSQLiteServerResult);
var
  R, C: Integer;
begin
  Log(AHeader);
  Log('  * Rows: ' + IntToStr(R));
  Log('  * Cols: ' + IntToStr(C));
  for R := 0 to ASQLiteResult.RowCount-1 do begin
    Log('Row: ' + IntToStr(R));
    for C := 0 to ASQLiteResult.FieldCount-1 do begin
      Log(
        '  * ' +
        'Col: ' + IntToStr(C) + ' ' +
        'Name: ' + ASQLiteResult.Names[C] + ' ' +
        'Type: ' + ASQLiteResult.ValueType[R][C] + ' ' +
        'Value: ' + ASQLiteResult.Value[R][C]
      );
    end;
  end;
end;

end.
