program Test;

uses
  Vcl.Forms,
  UnitFrmMain in 'UnitFrmMain.pas' {FrmMain},
  UnitTest1 in 'UnitTest1.pas',
  UnitTest2 in 'UnitTest2.pas',
  UnitTest3 in 'UnitTest3.pas',
  UnitToolsProject in 'UnitToolsProject.pas',
  UnitTest4 in 'UnitTest4.pas',
  UnitTest5 in 'UnitTest5.pas',
  UnitTest6 in 'UnitTest6.pas',
  UnitTest7 in 'UnitTest7.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.MainFormOnTaskbar := True;
  Application.CreateForm(TFrmMain, FrmMain);
  Application.Run;
end.
