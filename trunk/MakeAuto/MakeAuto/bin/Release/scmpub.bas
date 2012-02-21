Attribute VB_Name = "timepub"
Sub getTime()

    Sheets(1).Cells(1, 1) = "我们"

End Sub


Sub getTime2(title As String)

    Sheets(1).Cells(2, 1) = title & " 我们所有人" & Now

End Sub

Function getTime3(title As String)

    getTime3 = title & " : " & Now

End Function

Function CreateAs3CodePub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
'  Dim iBeginNo As Integer, iEndNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
  
  Set fs = CreateObject("Scripting.FileSystemObject")
  If fs.FolderExists(Trim(Sheets(1).tbFileDir.Text)) Then
'    If Right(Trim(tbFileDir.Text), 1) = "\" Then
'      sFileDictionary = Trim(tbFileDir.Text)
'    Else
'      sFileDictionary = Trim(tbFileDir.Text) & "\"
'    End If
     
    sFileDictionary = sFileDir
  
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
    
    '生成X标志行代码标志
'    If cbTest.Value = True Then
'      If MsgBox("你选择了不生成X标志的代码！确认要如此吗？ ", vbYesNo) = vbYes Then
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
    
    '选择的模块页面名
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

     ' 标定返回值
    CreateAs3CodePub = -1
  
    If iBeginNo > iEndNo Then
      MsgBox ("起始页面不能在结束页面之后！请重新选择")
    Else
      'If MsgBox("你选择了" & sVersionName & "版，你是否已经执行过添加缺省参数过程？ ", vbYesNo) = vbYes Then
        '初始化公共变量
        '位1 模块定义 2 标准字段目录 3 特殊字段列表 4 表修改顺序目录 5 服务实现缺省参数 6 函数实现缺省参数
        '7 后台过程缺省参数 8 宏定义 9 调换功能替换参数
        Call SetPublicVarValue("111000011")
        
        Call CreateServerCodeAs3(iBeginNo, iEndNo)
      '  MsgBox (sVersionName & "版(AS2)CPP文件生成在" & sFileDictionary & "下！")
        CreateAs3CodePub = 0
      'Else
      '  MsgBox ("你选择进行中间件服务端代码生成的操作取消")
      'End If
    End If
  
    'Sheets(1).Select
  Else
    MsgBox ("生成的文件路径不正确！请检查路径")
  End If
End Function
Function CreateSQLCodePub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  'Dim iBeginNo As Integer, iEndNo As Integer, iBeginFuNo As Integer, iEndFuNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
'  sFileDictionary = InputBox("请输入路径", "生成文件", "c:\")

  
  Set fs = CreateObject("Scripting.FileSystemObject")
  If fs.FolderExists(Trim(Sheets(1).tbFileDir.Text)) Then
    'If Right(Trim(tbFileDir.Text), 1) = "\" Then
    '  sFileDictionary = Trim(tbFileDir.Text)
    'Else
    '  sFileDictionary = Trim(tbFileDir.Text) & "\"
    'End If
    
    sFileDictionary = sFileDir
  
    sVersionName = "ORACLE"
    
    '生成实时备份代码标志
'    If cbBackupCode.Value = True Then
'      iBackUpCodeFlag = 1
'    Else
      iBackUpCodeFlag = 0
'    End If
    
    '生成X标志行代码标志
'    If cbTest.Value = True Then
'      If MsgBox("你选择了不生成X标志的代码！确认要如此吗？ ", vbYesNo) = vbYes Then
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
    
    '选择的模块页面名
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
  
    ' 标定返回值
    CreateSQLCodePub = -1
    
    If iBeginNo > iEndNo Then
      MsgBox ("起始页面不能在结束页面之后！请重新选择")
    Else
        'If MsgBox("你选择了" & sVersionName & "版，确认已添加过缺省参数且已读入其他被调用模块文件？ ", vbYesNo) = vbYes Then
            '初始化公共变量
            '位1 模块定义 2 标准字段目录 3 特殊字段列表 4 表修改顺序目录 5 服务实现缺省参数 6 函数实现缺省参数
            '7 后台过程缺省参数 8 宏定义 9 调换功能替换参数
            Call SetPublicVarValue("110100011")
            
            Call CreateSQLSRC(iBeginNo, iEndNo)
            'MsgBox ("生成完毕！文件生成在" & sFileDictionary & "下！")
            CreateSQLCodePub = 0
        'Else
        '  MsgBox ("操作取消！")
        'End If
    End If

    'Sheets(1).Select
  Else
    MsgBox ("生成的文件路径不正确！请检查路径")
  End If
End Function
Function DocHyberLinkPub(sFileDir As String, iBeginNo As Integer, iEndNo As Integer) As Integer
  'Dim iBeginNo As Integer, iEndNo As Integer
  'Dim sStartListName As String, sEndListName As String
  
  '选择的模块页面名
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
    MsgBox ("起始页面不能在结束页面之后！请重新选择")
  Else
    Set fs = CreateObject("Scripting.FileSystemObject")
    If fs.FolderExists(Trim(Sheets(1).tbFileDir.Text)) Then
        'If Right(Trim(tbFileDir.Text), 1) = "\" Then
        '  sFileDictionary = Trim(tbFileDir.Text)
        'Else
        '  sFileDictionary = Trim(tbFileDir.Text) & "\"
        'End If
          
        sFileDictionary = sFileDir
        
        'If MsgBox("你将对选定的文档进行调用的超链接处理，确认需要这么做吗？", vbYesNo) = vbYes Then
            '初始化公共变量
            '位1 模块定义 2 标准字段目录 3 特殊字段列表 4 表修改顺序目录 5 服务实现缺省参数 6 函数实现缺省参数
            '7 后台过程缺省参数 8 宏定义 9 调换功能替换参数
            Call SetPublicVarValue("100000010")
            
            Call CreateDocHyberLink(iBeginNo, iEndNo)
            'MsgBox ("选定的文档超链接处理完成！")
            DocHyberLinkPub = 0
        'Else
        '  MsgBox ("你选择进行文档的超链接处理的操作取消！")
        'End If
    Else
      MsgBox ("生成的文件路径不正确！请检查路径")
    End If
  End If

  'Sheets(1).Select
End Function
