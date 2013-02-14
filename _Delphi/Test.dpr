program Test;

uses
  Vcl.Forms,
  UnitFrmMain in 'UnitFrmMain.pas' {FrmMain},
  UnitTest1 in 'UnitTest1.pas',
  UnitTest2 in 'UnitTest2.pas',
  UnitTest3 in 'UnitTest3.pas',
  UnitToolsProject in 'UnitToolsProject.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.MainFormOnTaskbar := True;
  Application.CreateForm(TFrmMain, FrmMain);
  Application.Run;
end.
