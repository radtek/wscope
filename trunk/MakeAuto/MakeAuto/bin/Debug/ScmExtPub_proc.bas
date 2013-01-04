Attribute VB_Name = "scmextpub_proc"
Sub getTime()
    Sheets(1).Cells(1, 1) = "����"
End Sub

Sub getTime2(title As String)
    Sheets(1).Cells(2, 1) = title & " ����������" & Now
End Sub

Function getTime3(title As String)
    getTime3 = title & " : " & Now
End Function

Function CreateAs3CodePub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
'  Dim iBeginNo As Integer, iEndNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
  Set fs = CreateObject("Scripting.FileSystemObject")
  If fs.FolderExists(sFileDir) Then
    If Right(Trim(sFileDir), 1) = "\" Then
      sFileDictionary = Trim(sFileDir)
    Else
      sFileDictionary = Trim(sFileDir) & "\"
    End If
  
    sVersionName = "ORACLE"
    
    iProCVersion = 0
  
    sCPPVersion = "GCC"
    
    
    If Sheets(1).obCreateProcedure.Value = True Then
      iCreateType = 0
'    ElseIf obCreateServer.Value = True Then
'      iCreateType = 1
    ElseIf Sheets(1).obCreateFunction.Value = True Then
      iCreateType = 2
    End If
    
    If Sheets(1).opMakeObjectNo.Value = True Then
      iMakeSvrType = 0
    Else
      iMakeSvrType = 1
    End If
    
    '����X��־�д����־
'    If cbTest.Value = True Then
'      If MsgBox("��ѡ���˲�����X��־�Ĵ��룡ȷ��Ҫ����� ", vbYesNo) = vbYes Then
'        iTestCodeFlag = 1
'      Else
'        iTestCodeFlag = 0
'      End If
'    Else
      iTestCodeFlag = 0
'    End If
    
'    If obHsPlat3.Value = True Then
'      sCodeVerName = "HsPlat3"
'    ElseIf obHsFEBS.Value = True Then
      sCodeVerName = "HsFEBS"
'    End If
    
    'ѡ���ģ��ҳ����
    If Sheets(1).obSvrSelect.Value = True Then
      sSelectPageName = "SvrPage"
    ElseIf Sheets(1).obFuncSelect.Value = True Then
      sSelectPageName = "FuncPage"
    ElseIf Sheets(1).obProcSelect.Value = True Then
      sSelectPageName = "ProcPage"
    End If
    
'    sStartListName = Trim(Sheets(1).cbxStartPage.List(Sheets(1).cbxStartPage.ListIndex))
'    sEndListName = Trim(Sheets(1).cbxEndPage.List(Sheets(1).cbxEndPage.ListIndex))
    
'    iBeginNo = Val(Left(sStartListName, InStr(sStartListName, " ") - 1))
'    iEndNo = Val(Left(sEndListName, InStr(sEndListName, " ") - 1))

     ' �궨����ֵ
    CreateAs3CodePub = -1
  
    If iBeginNo > iEndNo Then
      MsgBox ("��ʼҳ�治���ڽ���ҳ��֮��������ѡ��")
    Else
      'If MsgBox("��ѡ����" & sVersionName & "�棬���Ƿ��Ѿ�ִ�й����ȱʡ�������̣� ", vbYesNo) = vbYes Then
        '��ʼ����������
        'λ1 ģ�鶨�� 2 ��׼�ֶ�Ŀ¼ 3 �����ֶ��б� 4 ���޸�˳��Ŀ¼ 5 ����ʵ��ȱʡ���� 6 ����ʵ��ȱʡ����
        '7 ��̨����ȱʡ���� 8 �궨�� 9 ���������滻����
        Call SetPublicVarValue("111000011")
        
        Call CreateServerCodeAs3(iBeginNo, iEndNo)
      '  MsgBox (sVersionName & "��(AS2)CPP�ļ�������" & sFileDictionary & "�£�")
        CreateAs3CodePub = 0
      'Else
      '  MsgBox ("��ѡ������м������˴������ɵĲ���ȡ��")
      'End If
    End If
  
    'Sheets(1).Select
  Else
    MsgBox ("���ɵ��ļ�·������ȷ������·��")
  End If
End Function
Function CreateSQLCodePub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  'Dim iBeginNo As Integer, iEndNo As Integer, iBeginFuNo As Integer, iEndFuNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
'  sFileDictionary = InputBox("������·��", "�����ļ�", "c:\")

  
  Set fs = CreateObject("Scripting.FileSystemObject")
  If fs.FolderExists(sFileDir) Then
    If Right(Trim(sFileDir), 1) = "\" Then
      sFileDictionary = Trim(sFileDir)
    Else
      sFileDictionary = Trim(sFileDir) & "\"
    End If
  
    sVersionName = "ORACLE"
    
    '����ʵʱ���ݴ����־
'    If cbBackupCode.Value = True Then
'      iBackUpCodeFlag = 1
'    Else
      iBackUpCodeFlag = 0
'    End If
    
    '����X��־�д����־
'    If cbTest.Value = True Then
'      If MsgBox("��ѡ���˲�����X��־�Ĵ��룡ȷ��Ҫ����� ", vbYesNo) = vbYes Then
'        iTestCodeFlag = 1
'      Else
'        iTestCodeFlag = 0
'      End If
'    Else
      iTestCodeFlag = 0
'    End If
    
'    If obHsPlat3.Value = True Then
'      sCodeVerName = "HsPlat3"
'    ElseIf obHsFEBS.Value = True Then
      sCodeVerName = "HsFEBS"
'    End If
    
    'ѡ���ģ��ҳ����
    If Sheets(1).obSvrSelect.Value = True Then
      sSelectPageName = "SvrPage"
    ElseIf Sheets(1).obFuncSelect.Value = True Then
      sSelectPageName = "FuncPage"
    ElseIf Sheets(1).obProcSelect.Value = True Then
      sSelectPageName = "ProcPage"
    End If
    
    Set fs = CreateObject("Scripting.FileSystemObject")
    
    'sStartListName = Trim(cbxStartPage.List(cbxStartPage.ListIndex))
    'sEndListName = Trim(cbxEndPage.List(cbxEndPage.ListIndex))
    
    'iBeginNo = Val(Left(sStartListName, InStr(sStartListName, " ") - 1))
    'iEndNo = Val(Left(sEndListName, InStr(sEndListName, " ") - 1))
  
    ' �궨����ֵ
    CreateSQLCodePub = -1
    
    If iBeginNo > iEndNo Then
      MsgBox ("��ʼҳ�治���ڽ���ҳ��֮��������ѡ��")
    Else
        'If MsgBox("��ѡ����" & sVersionName & "�棬ȷ������ӹ�ȱʡ�������Ѷ�������������ģ���ļ��� ", vbYesNo) = vbYes Then
            '��ʼ����������
            'λ1 ģ�鶨�� 2 ��׼�ֶ�Ŀ¼ 3 �����ֶ��б� 4 ���޸�˳��Ŀ¼ 5 ����ʵ��ȱʡ���� 6 ����ʵ��ȱʡ����
            '7 ��̨����ȱʡ���� 8 �궨�� 9 ���������滻����
            Call SetPublicVarValue("110100011")
            
            Call CreateSQLSRC(iBeginNo, iEndNo)
            'MsgBox ("������ϣ��ļ�������" & sFileDictionary & "�£�")
            CreateSQLCodePub = 0
        'Else
        '  MsgBox ("����ȡ����")
        'End If
    End If

    'Sheets(1).Select
  Else
    MsgBox ("���ɵ��ļ�·������ȷ������·��")
  End If
End Function
Function DocHyberLinkPub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  'Dim iBeginNo As Integer, iEndNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
  'ѡ���ģ��ҳ����
  If Sheets(1).obSvrSelect.Value = True Then
    sSelectPageName = "SvrPage"
  ElseIf Sheets(1).obFuncSelect.Value = True Then
    sSelectPageName = "FuncPage"
  ElseIf Sheets(1).obProcSelect.Value = True Then
    sSelectPageName = "ProcPage"
  End If
  
  'sStartListName = Trim(cbxStartPage.List(cbxStartPage.ListIndex))
  'sEndListName = Trim(cbxEndPage.List(cbxEndPage.ListIndex))
  
  'iBeginNo = Val(Left(sStartListName, InStr(sStartListName, " ") - 1))
  'iEndNo = Val(Left(sEndListName, InStr(sEndListName, " ") - 1))
  
  DocHyberLinkPub = -1

  If iBeginNo > iEndNo Then
    MsgBox ("��ʼҳ�治���ڽ���ҳ��֮��������ѡ��")
  Else
    Set fs = CreateObject("Scripting.FileSystemObject")
    If fs.FolderExists(Trim(Sheets(1).tbFileDir.Text)) Then
        'If Right(Trim(tbFileDir.Text), 1) = "\" Then
        '  sFileDictionary = Trim(tbFileDir.Text)
        'Else
        '  sFileDictionary = Trim(tbFileDir.Text) & "\"
        'End If
          
        sFileDictionary = sFileDir
        
        'If MsgBox("�㽫��ѡ�����ĵ����е��õĳ����Ӵ���ȷ����Ҫ��ô����", vbYesNo) = vbYes Then
            '��ʼ����������
            'λ1 ģ�鶨�� 2 ��׼�ֶ�Ŀ¼ 3 �����ֶ��б� 4 ���޸�˳��Ŀ¼ 5 ����ʵ��ȱʡ���� 6 ����ʵ��ȱʡ����
            '7 ��̨����ȱʡ���� 8 �궨�� 9 ���������滻����
            Call SetPublicVarValue("100000010")
            
            Call CreateDocHyberLink(iBeginNo, iEndNo)
            'MsgBox ("ѡ�����ĵ������Ӵ�����ɣ�")
            DocHyberLinkPub = 0
        'Else
        '  MsgBox ("��ѡ������ĵ��ĳ����Ӵ���Ĳ���ȡ����")
        'End If
    Else
      MsgBox ("���ɵ��ļ�·������ȷ������·��")
    End If
  End If

  'Sheets(1).Select
End Function
Function FunctionListXMLPub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  'Dim iBeginNo As Integer, iEndNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
  Set fs = CreateObject("Scripting.FileSystemObject")
  If fs.FolderExists(Trim(sFileDir)) Then
    If Right(Trim(sFileDir), 1) = "\" Then
      sFileDictionary = Trim(sFileDir)
    Else
      sFileDictionary = Trim(sFileDir) & "\"
    End If
  
'    If obHsPlat3.Value = True Then
'      sCodeVerName = "HsPlat3"
'    ElseIf obHsFEBS.Value = True Then
      sCodeVerName = "HsFEBS"
'    End If
    
    'sStartListName = Trim(cbxStartPage.List(cbxStartPage.ListIndex))
    'sEndListName = Trim(cbxEndPage.List(cbxEndPage.ListIndex))
    
    'iBeginNo = Val(Left(sStartListName, InStr(sStartListName, " ") - 1))
    'iEndNo = Val(Left(sEndListName, InStr(sEndListName, " ") - 1))
  
    FunctionListXMLPub = -1
    'If iBeginNo > iEndNo Then
    '  MsgBox ("��ʼҳ�治���ڽ���ҳ��֮��������ѡ��")
    'Else
        'If MsgBox("ϵͳ������AS2��FunctionList.XML�ļ�����ȷ��...", vbYesNo) = vbYes Then
            'λ1 ģ�鶨�� 2 ��׼�ֶ�Ŀ¼ 3 �����ֶ��б� 4 ���޸�˳��Ŀ¼ 5 ����ʵ��ȱʡ���� 6 ����ʵ��ȱʡ����
            '7 ��̨����ȱʡ���� 8 �궨�� 9 ���������滻����
            Call SetPublicVarValue("100000000")
            
            Call CreateFunctionListXML(iBeginNo, iEndNo)
            
            FunctionListXMLPub = 0
            'MsgBox ("�ļ�������" & sFileDictionary & "�£�")
        'Else
        '  MsgBox ("��ȡ���˴����ͻ��˹�����AS2���FunctionList.XML�ļ��Ĳ�����")
        'End If
    'End If

    'Sheets(1).Select
  Else
    MsgBox ("���ɵ��ļ�·������ȷ������·��")
  End If
End Function
 
Function ScmExtPub(OperType As Integer, sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  ExtPub = -1
  Select Case OperType
    Case 1
      Call CreateAs3CodePub(sFileDir, iBeginNo, iEndNo)  '1 Proc SOԴ�ļ�
      ExtPub = 0
    Case 2
      Call CreateSQLCodePub(sFileDir, iBeginNo, iEndNo)  ' 2 SQL����
      ExtPub = 0
    Case 3
      Call FunctionListXMLPub(sFileDir, iBeginNo, iEndNo) '3 ���� xml �ļ�
      ExtPub = 0
    Case 4
      Call DocHyberLinkPub(sFileDir, iBeginNo, iEndNo)  '4 ����������
      ExtPub = 0
    Case Else
      ExtPub = 99
  End Select
End Function

'���Եĺ�
Sub ExtPub1()
  Call ScmExtPub(1, "C:\src", 13, 13)
End Sub
