@echo off

rem %1 指定第一个参数，指定使用Delphi 5还是Delphi 6 来编译
rem %2 指定编译的工程名称
rem %3 指定输出的目录
set DelphiVer=%1
set ProDir=%~dp2
set ProName=%~nx2
set OutPut=%3

rem 编译DLL: 
rem cm 6 E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF\C_CbpETF.dpr C:\src
rem 编译HsSettle: 
rem cm 6 E:\VSS\HsSettle\Sources\ClientCom\HsSettle\HsSettle.dpr C:\src
rem 编译HsTools: 
rem cm 5 E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\TOOLS\HsTools.dpr C:\src

rem 这里指定Control的位置，需要自行调整
set DCC5=D:\Program Files\Borland\Delphi5\Bin\DCC32.EXE
set DCC6=D:\Program Files\Borland\Delphi6\Bin\DCC32.EXE
set HSControls=E:\VSS\HSTRADES11\Sources\ClientCom\Control;
set Platform10=E:\VSS\HSTRADES11\Sources\ClientCom\Control\PLATFORM10;
set Lzrw1_5=E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\TOOLS\CONTROL\LZRW1;
set Lzrw1_6=E:\VSS\HsSettle\Sources\ClientCom\Control\LZRW1;
set DOA5=D:\Program Files\Borland\Delphi5\DOA;
set DOA6=D:\Program Files\Borland\Delphi6\DOA;

rem 指定UnitOutPut目录
set DCUDir=E:\06trade\HSTRADES11\trunk\Sources\ClientCom\Obj

if "%DelphiVer%"=="6" goto D6
:D5
set DCC=%DCC5%
set UPath=%Platform10%%Lzrw1_5%%DOA5%
goto Make

:D6
set DCC=%DCC6%
set UPath=%HsControls%%Lzrw1_6%%DOA6%
goto Make

:Make
rem 使用/d 强制切换驱动器到源代码路径，这里应该是个参数
rem cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF
rem 使用这个也可以 pushd E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\
pushd %ProDir%

rem 开始自动编译
rem -B Build All
rem -E 指定输出路径，有些同学不一定都在 D:\Febs2005\Trade\Biz 啥的
rem -U 指定还从哪里查找使用的单元文件
rem 这里替换 C_CbpETF为其他DLL
rem echo "%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -E"%OutPut%"
"%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -E"%OutPut%"

rem 如果编译失败，那么需要显示编译信息，以便用户确认来进行处理
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem 输出一行空行，且暂停
rem echo. & pause