rem UF2.0前台编译
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

rem 这里指定Control的位置，需要自行调整: D6Home, TradeHome SettHome
set D6Home=D:\Program Files\Borland\Delphi6
set DCC6=%D6Home%\Bin\DCC32.EXE

set HsControl=E:\HSRef10\Sources\ClientCom\Control\;D:\Program Files\Raize\RC4\Lib\Delphi6
set D6UPath=%D6Home%\Lib;%D6Home%\Bin;%D6Home%\Imports;%D6Home%\Bpl;

rem 指定UnitOutPut目录
set DCUDir=C:\Obj

set DCC=%DCC6%
set UPath=%D6UPath%%HsControl%
set LUPackage=Hs08Controls
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
echo "%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"
"%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"

rem 如果编译失败，那么需要显示编译信息，以便用户确认来进行处理
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem 输出一行空行，且暂停
rem echo. & pause