unit UnitFrmMain;

interface

uses
  // Own
  UnitToolsProject,
  // Test cases
  UnitTest1,
  UnitTest2,
  UnitTest3,
  UnitTest4,
  UnitTest5,
  UnitTest6,
  UnitTest7,
  // SQL Connector
  SQLiteServer.Connector,
  // Delphi
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants,
  System.Classes, Vcl.Graphics, Vcl.Controls, Vcl.Forms, Vcl.Dialogs,
  Vcl.StdCtrls;

type
  TFrmMain = class(TForm)
    GroupBox1: TGroupBox;
    Label1: TLabel;
    InpUser: TEdit;
    Label2: TLabel;
    InpPass: TEdit;
    InpHost: TEdit;
    Label3: TLabel;
    InpPort: TEdit;
    Label4: TLabel;
    GroupBox2: TGroupBox;
    BtnRun: TButton;
    CBDebug: TCheckBox;
    Label5: TLabel;
    CBTests: TComboBox;
    GroupBox3: TGroupBox;
    LBLog: TListBox;
    procedure FormCreate(Sender: TObject);
    procedure BtnRunClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    SQLiteServerConnector: TSQLiteServerConnector;
    TestCaseArr: TTestCaseArr;
  end;

var
  FrmMain: TFrmMain;

implementation

{$R *.dfm}

// -----------------------------------------------------------------------------
// Create/Destroy
// -----------------------------------------------------------------------------

procedure TFrmMain.FormCreate(Sender: TObject);
var
  TestCase: TTestCase;
begin
  UnitToolsProject.LogList := LBLog;

  // Add Test cases to array
  SetLength(TestCaseArr, 0);
  AddTestCaseToArr(TestCaseArr, 'Test1', TestCase1);
  AddTestCaseToArr(TestCaseArr, 'Test2', TestCase2);
  AddTestCaseToArr(TestCaseArr, 'Test3', TestCase3);
  AddTestCaseToArr(TestCaseArr, 'Test4', TestCase4);
  AddTestCaseToArr(TestCaseArr, 'Test5', TestCase5);
  AddTestCaseToArr(TestCaseArr, 'Test6', TestCase6);
  AddTestCaseToArr(TestCaseArr, 'Test7', TestCase7);

  // Tests to ComboBox
  CBTests.Clear;
  CBTests.Items.Add('(all)');
  for TestCase in TestCaseArr do begin
    CBTests.Items.Add(TestCase.Name);
  end;
  CBTests.ItemIndex := 0;
end;

procedure TFrmMain.FormDestroy(Sender: TObject);
begin
  // Clear Test Cases
  SetLength(TestCaseArr, 0);
end;

// -----------------------------------------------------------------------------
// Form
// -----------------------------------------------------------------------------

procedure TFrmMain.BtnRunClick(Sender: TObject);
var
  I: Integer;
  SQL: String;
  ConnectionData: TConnectionData;
  IsDebug: Boolean;
  TestCase: TTestCase;
  Res: Boolean;
  RunTests: String;
begin
  // Init
  LBLog.Clear;
  RunTests := CBTests.Items.Strings[CBTests.ItemIndex];

  // Parse connection data
  try
    IsDebug := CBDebug.Checked;
    with ConnectionData do begin
      User := InpUser.Text;
      Pass := InpPass.Text;
      Host := InpHost.Text;
      Port := StrToInt(InpPort.Text);
    end;
  except
    on E: Exception do begin
      Log('Exception: ' + E.Message);
      exit;
    end;
  end;

  // Connect and exec Test cases
  SQLiteServerConnector := TSQLiteServerConnector.Create(
    ConnectionData.Host,
    ConnectionData.Port,
    ConnectionData.User,
    ConnectionData.Pass
  );
  try

    // Run tests
    for TestCase in TestCaseArr do begin
      if (RunTests = '(all)') or (RunTests = TestCase.Name) then begin
        if (IsDebug) then Log('Running Test: ' + TestCase.Name);
        LBLog.Items.BeginUpdate;
        try
          Res := TestCase.Proc(SQLiteServerConnector, IsDebug);
          Log(TestCase.Name + ' Result: ' + ResultToStr(Res));
          if (IsDebug) then Log('');
        finally
          LBLog.Items.EndUpdate;
          LBLog.Invalidate;
        end;
      end;
    end;

  except
    on E: Exception do begin
      Log('Exception: ' + E.Message);
    end;
  end;

  // Disconnect or Free
  SQLiteServerConnector.Free;
end;

end.
