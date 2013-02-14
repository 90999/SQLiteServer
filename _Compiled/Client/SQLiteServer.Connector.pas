unit SQLiteServer.Connector;

interface

uses
  // Indy
  IdBaseComponent, IdComponent, IdTCPConnection, IdTCPClient, IdGlobal,
  // Delphi
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants,
  System.Classes, Vcl.Graphics, Vcl.Controls, Vcl.Forms, Vcl.Dialogs,
  Winapi.MSXML, System.StrUtils, System.Types;

type
  TSQLiteServerResult = record
		XML: IXMLDOMDocument;
		Error: Boolean;
		ErrorMessage: String;
		RowCount: Int64;
		FieldCount: Int64;
		Names: Array of String;
		Value: Array of Array of String;
		ValueType: Array of Array of String;
  end;

  TSQLiteServerConnector = class
  private
    Host: String;
    Port: Word;
    User: String;
    Pass: String;
    Socket: TIdTCPClient;

    function Connect: Boolean;
    procedure Disconnect;
    function Connected: Boolean;
    function Send(ASQLQuery: String; ANoResult: Boolean): IXMLDOMDocument;
    procedure ParseSQLiteResult(var AResult: TSQLiteServerResult);
  public
    constructor Create(AHost: String; APort: Word; AUser: String; APAss: String);
    destructor Destroy; override;
    function ExecSQL(ASQLQuery: String; const ANoResult: Boolean = FALSE): TSQLiteServerResult;
    procedure FreeResult(var AResult: TSQLiteServerResult);
    function EscapeAndQuote(AValue: String): String; overload;
    function EscapeAndQuote(AValue: Single): String; overload;
    function EscapeAndQuote(AValue: Int64): String; overload;
    function EscapeAndQuote(AValue: Integer): String; overload;
    function EscapeAndQuote(AValue: Boolean): String; overload;
    function EscapeAndQuote(AValue: TBytes): String; overload;
    function Escape(AValue: String): String; overload;
    function Escape(AValue: Single): String; overload;
    function Escape(AValue: Int64): String; overload;
    function Escape(AValue: Integer): String; overload;
    function Escape(AValue: Boolean): String; overload;
  end;

implementation

// Constructor
constructor TSQLiteServerConnector.Create(AHost: String; APort: Word; AUser: String; APAss: String);
begin
  inherited Create;

  Host := AHost;
  Port := APort;
  User := AUser;
  Pass := APass;

  Socket := nil;
end;

// Destructor
destructor TSQLiteServerConnector.Destroy;
begin
  Socket.Free;

  inherited Destroy;
end;

// Connect
function TSQLiteServerConnector.Connect: Boolean;
var
  Line: String;
  StartTime: Cardinal;
begin
  Result := FALSE;

  if Socket <> nil then begin
    Disconnect;
  end;

  Socket := TIdTCPClient.Create(nil);
  Socket.Port := Port;
  Socket.Host := Host;
  Socket.ConnectTimeout := 10000;
  Socket.ReadTimeout := 5000;
  try

    Socket.Connect;
    if not Connected then begin
      raise Exception.Create('Cannot connect to remote host');
    end;

    // Set Default Encoding
    Socket.IOHandler.DefStringEncoding := TIdTextEncoding.UTF8;

    // Wait on Greeting from Server
    StartTime := WinApi.Windows.GetTickCount;
    while (TRUE)  do begin
      Line := Socket.IOHandler.ReadLnWait;
      if (not Socket.IOHandler.ReadLnTimedOut) then begin
        if (Line <> 'SQLiteServer v1.0') then begin
          raise Exception.Create('Wrong Server version detected: ' + Line);
        end else begin
          break;
        end;
      end else if (WinApi.Windows.GetTickCount-StartTime>10000) then begin
        raise Exception.Create('Read request timed out');
      end;
    end;

    // Send Login
    Socket.IOHandler.WriteLn('USER:' + User + ':' + Pass);

    // Wait on login response
    StartTime := WinApi.Windows.GetTickCount;
    while (TRUE)  do begin
      Line := Socket.IOHandler.ReadLnWait();
      if (not Socket.IOHandler.ReadLnTimedOut) then begin
        if (Line <> 'RIGHTS:ro') and (Line <> 'RIGHTS:rw') then begin
          raise Exception.Create('Wrong permissions detected: ' + Line);
        end else begin
          break;
        end;
      end else if (WinApi.Windows.GetTickCount-StartTime>10000) then begin
        raise Exception.Create('Read request timed out');
      end;
    end;

    Result := TRUE;
  except
    on E: Exception do begin
      Disconnect;
    end;
  end;

end;

// Disconnect
procedure TSQLiteServerConnector.Disconnect;
begin
  if (Socket = nil) then exit;

  try
    Socket.DisconnectNotifyPeer;
  except
  end;

  FreeAndNil(Socket);
end;

// Connected
function TSQLiteServerConnector.Connected: Boolean;
begin
  Result := FALSE;

  if Socket = nil then exit;

  Result := Socket.Connected;
end;

// Send -> TCP Server
function TSQLiteServerConnector.Send(ASQLQuery: String; ANoResult: Boolean): IXMLDOMDocument;
var
  SL: TStringList;
  I: Integer;
  Line: String;
  //
	Count: Integer;
  RecvStr: String;
  RecvLine: String;
  DynArray: TStringDynArray;
  //
  XmlRoot: IXMLDOMElement;
  XmlStatus: IXMLDOMElement;
  //
  LastLine: Cardinal;
begin
  Result := CoDOMDocument30.Create;
  Result.appendChild(Result.createProcessingInstruction('xml', 'version="1.0" encoding="UTF-8" standalone="yes"'));

  try
    if (not Connected) then begin
      if (not Connect) then begin
        raise Exception.Create('Cannot connect to remote host');
      end;
    end;

  	// Send REQUEST
    SL := TStringList.Create;
    try
      SL.Text := ASQLQuery;

      Line := 'REQUEST:' + IntToStr(SL.Count) + ':';
      if ANoResult then Line := Line + '1' else Line := Line + '0';
      Socket.IOHandler.WriteLn(Line);

      for I := 0 to SL.Count-1 do begin
        Socket.IOHandler.WriteLn('.' + SL.Strings[I]);
      end;
    finally
      SL.Free;
    end;

  	if ANoResult then exit;

  	// Read
		Count := -1;
		RecvStr := '';
		RecvLine := '';

    LastLine := 0;
    while (TRUE) do begin

      RecvLine := Socket.IOHandler.ReadLnWait;
      if (not Socket.IOHandler.ReadLnTimedout) then begin
        DynArray := SplitString(RecvLine, ':');
        try
          if ((Length(DynArray)>=2) and (UpperCase(DynArray[0]) = 'RESULT')) then begin
            LastLine := GetTickCount;
            Count := StrToInt(Trim(DynArray[1]));
          end else if ((Count>0) and (Copy(RecvLine,1,1)='.')) then begin
            LastLine := GetTickCount;
            Count := Count -1;
            RecvStr := RecvStr + Copy(RecvLine,2,Length(RecvLine));
            if (Count > 0) then begin
              RecvStr := RecvStr + #10;
            end else if (count = 0) then begin
              Result.loadXML(RecvStr);
              Count := -1;
              break;
            end;
          end;

        finally
          SetLength(DynArray, 0);
        end;

      end else if (LastLine > 0) and (GetTickCount-LastLine>1000) then begin
        raise Exception.Create('Read request timed out');
      end;
    end;
  except
    on E: Exception do begin
      XmlRoot := Result.createElement('Result');

      XmlStatus := Result.createElement('Status');
      XmlStatus.setAttribute('Error', 'true');
      XmlStatus.setAttribute('ErrorMessage', E.Message);
      XmlRoot.appendChild(XmlStatus);

      Result.appendChild(XmlRoot);

      Disconnect;
    end;
  end;
end;


procedure TSQLiteServerConnector.ParseSQLiteResult(var AResult: TSQLiteServerResult);
var
  XmlStatus, XmlFields: IXMLDOMNode;
  FieldArr, ColArr, RowArr: IXMLDOMNodeList;
  I,N: Integer;
  R,C: Int64;
begin
  // Error flag
  try
    XmlStatus := AResult.XML.getElementsByTagName('Status').item[0];
    AResult.Error := XmlStatus.attributes.getNamedItem('Error').text <> 'false';
    if (AResult.Error) then begin
      AResult.ErrorMessage := XmlStatus.attributes.getNamedItem('ErrorMessage').text;
    end;
  except
    AResult.Error := TRUE;
    AResult.ErrorMessage := 'Client Error: Cannot parse error';
  end;
  if (AResult.Error) then exit;

  // RowCount/FieldCount
  try
    XmlStatus := AResult.XML.getElementsByTagName('Status').item[0];
    AResult.RowCount := StrToInt64(XmlStatus.attributes.getNamedItem('RowCount').text);
    AResult.FieldCount := StrToInt64(XmlStatus.attributes.getNamedItem('FieldCount').text);
  except
    AResult.Error := TRUE;
    AResult.ErrorMessage := 'Client Error: Cannot parse row/field count';
  end;
  if (AResult.Error) then exit;

  // Field Names
  try
    XmlFields := AResult.XML.getElementsByTagName('Fields').item[0];
    FieldArr := XmlFields.childNodes;
    for I := 0 to FieldArr.length-1 do begin
      if FieldArr.item[I].nodeName = 'Col' then begin
        SetLength(AResult.Names, Length(AResult.Names)+1);
        AResult.Names[Length(AResult.Names)-1] := FieldArr.item[I].attributes.getNamedItem('Name').text;
      end;
    end;
  except
    AResult.Error := TRUE;
    AResult.ErrorMessage := 'Client Error: Cannot parse row/field count';
  end;
  if (AResult.Error) then exit;

  // Value/Type Names
  try
    SetLength(AResult.ValueType, AResult.RowCount, AResult.FieldCount);
    SetLength(AResult.Value, AResult.RowCount, AResult.FieldCount);

    RowArr := AResult.XML.getElementsByTagName('Row');
    R := -1;
    for N := 0 to RowArr.length-1 do begin
      if RowArr.item[N].nodeName = 'Row' then begin
        R := R + 1;
        C := -1;
        ColArr := RowArr.item[N].childNodes;
        for I := 0 to ColArr.length-1 do begin
          if ColArr.item[I].nodeName = 'Col' then begin
            C := C + 1;
            AResult.Value[R][C] := ColArr.item[I].attributes.getNamedItem('Value').text;
            AResult.ValueType[R][C] := ColArr.item[I].attributes.getNamedItem('Type').text;
          end;
        end;
      end;
    end;
  except
    AResult.Error := TRUE;
    AResult.ErrorMessage := 'Client Error: Cannot parse row/field count';
  end;
  if (AResult.Error) then exit;
end;

function TSQLiteServerConnector.ExecSQL(ASQLQuery: String; const ANoResult: Boolean = FALSE): TSQLiteServerResult;
begin
  // Init Result
	Result.XML := nil;
	Result.Error := TRUE;
	Result.ErrorMessage := 'Result set is init value';
	Result.RowCount := 0;
	Result.FieldCount := 0;
	SetLength(Result.Names, 0);
	SetLength(Result.Value, 0, 0);
	SetLength(Result.ValueType, 0, 0);

  Result.XML := Send(ASQLQuery, ANoResult);
  if (ANoResult = TRUE) then begin
		Result.Error := FALSE;
		Result.ErrorMessage := '';
	end else begin
		ParseSQLiteResult(Result);
  end;
end;

procedure TSQLiteServerConnector.FreeResult(var AResult: TSQLiteServerResult);
begin
  AResult.Xml := nil; // decrement reference counter
	AResult.Error := FALSE;
	AResult.ErrorMessage := '';
	SetLength(AResult.Names, 0);
	SetLength(AResult.Value, 0, 0);
	SetLength(AResult.ValueType, 0, 0);
end;

// -----------------------------------------------------------------------------
// SQL (MySQL)
// -----------------------------------------------------------------------------

function TSQLiteServerConnector.EscapeAndQuote(AValue: String): String;
begin
  Result := '"' + Escape(AValue) + '"';
end;

function TSQLiteServerConnector.EscapeAndQuote(AValue: Single): String;
begin
  Result := '"' + Escape(AValue) + '"';
end;

function TSQLiteServerConnector.EscapeAndQuote(AValue: Int64): String;
begin
  Result := '"' + Escape(AValue) + '"';
end;

function TSQLiteServerConnector.EscapeAndQuote(AValue: Integer): String;
begin
  Result := '"' + Escape(AValue) + '"';
end;

function TSQLiteServerConnector.EscapeAndQuote(AValue: Boolean): String;
begin
  Result := '"' + Escape(AValue) + '"';
end;

function TSQLiteServerConnector.EscapeAndQuote(AValue: TBytes): String;
var
  B: Byte;
begin
  Result := 'X''';
  for B in AValue do begin
    Result := Result + IntToHex(B,2);
  end;
  Result := Result + '''';
end;

function TSQLiteServerConnector.Escape(AValue: String): String;
var
  I: Integer;
begin
  for I := length(AValue) downto 1 do begin
    if Copy(AValue,I,1) = #0 then begin
      AValue[i] := '0';
      Insert ('\', AValue, I);
    end;
    if Copy(AValue,I,1) = '\' then begin
      Insert ('\', AValue, I);
    end;
    if Copy(AValue,I,1) = '''' then begin
      Insert ('\', AValue, I);
    end;
    if Copy(AValue,I,1) = '"' then begin
      Insert ('\', AValue, I);
    end;
  end;
  Result := AValue
end;

function TSQLiteServerConnector.Escape(AValue: Single): String;
var
  DS: Char;
begin
  DS := FormatSettings.DecimalSeparator;
  try
    FormatSettings.DecimalSeparator := '.';
    Result := Escape(FloatToStr(AValue));
  except
  end;
  FormatSettings.DecimalSeparator := DS;
end;

function TSQLiteServerConnector.Escape(AValue: Int64): String;
begin
  Result := Escape(IntToStr(AValue));
end;

function TSQLiteServerConnector.Escape(AValue: Integer): String;
begin
  Result := Escape(IntToStr(AValue));
end;

function TSQLiteServerConnector.Escape(AValue: Boolean): String;
begin
  if AValue then Result := Escape('1') else Result := Escape('0');
end;

(*

  Example Implementation:

  procedure Query;
  var
    SQLiteResult: TSQLiteServerResult;
    R, C: Integer;
  begin
    SQLiteServerConnector := TSQLiteServerConnector.Create('localhost', 11833, 'Admin', 'Admin');

    SQLiteResult := SQLiteServerConnector.ExecSQL('SELECT * FROM test', FALSE);
    if (SQLiteResult.Error) then begin
      Log('ERROR: ' + SQLiteResult.ErrorMessage);
    end else begin
      for R := 0 to SQLiteResult.RowCount-1 do begin
        Log('Row: ' + IntToStr(R));
        for C := 0 to SQLiteResult.FieldCount-1 do begin
          Log(
            '  * Col: ' + IntToStr(C) + ' Name: ' + SQLiteResult.Names[C] + ' Type: ' + SQLiteResult.ValueType[R][C] + ' Value: ' + SQLiteResult.Value[R][C]
          );
        end;
      end;
    end;
    SQLiteServerConnector.FreeResult(SQLiteResult);

    SQLiteServerConnector.Free;
  end;

*)

end.
