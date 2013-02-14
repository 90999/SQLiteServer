object FrmMain: TFrmMain
  Left = 0
  Top = 0
  ClientHeight = 610
  ClientWidth = 937
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  DesignSize = (
    937
    610)
  PixelsPerInch = 96
  TextHeight = 13
  object GroupBox1: TGroupBox
    Left = 8
    Top = 8
    Width = 354
    Height = 161
    Caption = 'Connection Settings'
    TabOrder = 0
    DesignSize = (
      354
      161)
    object Label1: TLabel
      Left = 28
      Top = 37
      Width = 26
      Height = 13
      Caption = 'User:'
    end
    object Label2: TLabel
      Left = 28
      Top = 64
      Width = 26
      Height = 13
      Caption = 'Pass:'
    end
    object Label3: TLabel
      Left = 28
      Top = 91
      Width = 26
      Height = 13
      Caption = 'Host:'
    end
    object Label4: TLabel
      Left = 28
      Top = 118
      Width = 24
      Height = 13
      Caption = 'Port:'
    end
    object InpUser: TEdit
      Left = 76
      Top = 34
      Width = 253
      Height = 21
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 0
      Text = 'Admin'
    end
    object InpPass: TEdit
      Left = 76
      Top = 61
      Width = 253
      Height = 21
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 1
      Text = 'Admin'
    end
    object InpHost: TEdit
      Left = 76
      Top = 88
      Width = 253
      Height = 21
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 2
      Text = 'localhost'
    end
    object InpPort: TEdit
      Left = 76
      Top = 115
      Width = 253
      Height = 21
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
      Text = '11833'
    end
  end
  object GroupBox2: TGroupBox
    Left = 8
    Top = 175
    Width = 354
    Height = 117
    Caption = 'Tests'
    TabOrder = 1
    DesignSize = (
      354
      117)
    object Label5: TLabel
      Left = 28
      Top = 32
      Width = 30
      Height = 13
      Caption = 'Tests:'
    end
    object BtnRun: TButton
      Left = 254
      Top = 69
      Width = 75
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Run'
      TabOrder = 0
      OnClick = BtnRunClick
    end
    object CBDebug: TCheckBox
      Left = 76
      Top = 73
      Width = 57
      Height = 17
      Caption = 'Debug'
      Checked = True
      State = cbChecked
      TabOrder = 1
    end
    object CBTests: TComboBox
      Left = 76
      Top = 29
      Width = 253
      Height = 21
      Style = csDropDownList
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 2
    end
  end
  object GroupBox3: TGroupBox
    Left = 368
    Top = 8
    Width = 561
    Height = 594
    Anchors = [akLeft, akTop, akRight, akBottom]
    Caption = 'Log'
    TabOrder = 2
    ExplicitWidth = 566
    ExplicitHeight = 580
    DesignSize = (
      561
      594)
    object LBLog: TListBox
      Left = 16
      Top = 24
      Width = 529
      Height = 555
      Anchors = [akLeft, akTop, akRight, akBottom]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      ItemHeight = 13
      ParentFont = False
      TabOrder = 0
      ExplicitHeight = 601
    end
  end
end
