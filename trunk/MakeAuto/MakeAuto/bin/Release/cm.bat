@echo off

rem %1 指定第一个参数，指定使用Delphi 5还是Delphi 6 来编译
rem %2 指定编译的工程名称
rem %3 指定输出的目录
set DelphiVer=%1
set ProDir=%~dp2
set ProName=%~nx2
set OutPut=%3

rem 编译DLL: 
rem cm.bat 6 E:\06trade\HSTRADES11\trunk\Sources\ClientCom\Subsys\Secu\CbpETF\C_CbpETF.dpr C:\src
rem 编译HsSettle: 
rem cm.bat 6 E:\06trade\HsSettle\trunk\Sources\ClientCom\HsSettle\HsSettle.dpr C:\src
rem 编译HsTools: 
rem cm.bat 5 E:\06trade\HSTRADES11\trunk\Sources\ClientCom\Subsys\TOOLS\HsTools.dpr C:\src

rem 这里指定Control的位置，需要自行调整: D5Home, D6Home, TradeHome SettHome
set D5Home=D:\Program Files\Borland\Delphi5
set D6Home=D:\Program Files\Borland\Delphi6
set TradeHome=E:\06trade\HSTRADES11\trunk
set SettHome=E:\06trade\HsSettle\trunk\

set HsControls=%TradeHome%\Sources\ClientCom\Control;
set Platform10=%TradeHome%\Sources\ClientCom\Control\PLATFORM10;
set Lzrw1_5=%TradeHome%\Sources\ClientCom\Subsys\TOOLS\CONTROL\LZRW1;
set Lzrw1_6=%SettHome%\Sources\ClientCom\Control\LZRW1;

set DCC5=%D5Home%\Bin\DCC32.EXE
set DCC6=%D6Home%\Bin\DCC32.EXE
set D5UPath=%D5Home%\Lib;%D5Home%\Bin;%D5Home%\Imports;%D5Home%\Bpl;%D5Home%\DOA;%TradeHome%\Sources\ClientCom\Control;%TradeHome%\Sources\ClientCom\Control\PLATFORM10;
set D6UPath=%D6Home%\Lib;%D6Home%\Bin;%D6Home%\Imports;%D6Home%\Bpl;%D6Home%\DOA;

rem 指定UnitOutPut目录
set DCUDir=%TradeHome%\Sources\ClientCom\Obj

if "%DelphiVer%"=="6" goto D6
:D5
set DCC=%DCC5%
set UPath=%D5UPath%%Lzrw1_5%%Platform10%
rem Platform不带包编译，打包进程序 去掉 LUPackage=Platform10 的带包编译
set LUPackage=
goto Make

:D6
set DCC=%DCC6%
set UPath=%D6UPath%%Lzrw1_6%%HsControls%
set LUPackage=HsControls
goto Make

:Make
rem 使用/d 强制切换驱动器到源代码路径，这里应该是个参数
rem cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF
rem 使用这个也可以 pushd E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\
pushd %ProDir%

rem 开始自动编译
rem -Q -W- -H- -Q安静模式，减少输出; -W- 不输出警告；-H- 不输出Hint；这样输出的行会大幅减少
rem -B Build All
rem -E 指定输出路径，有些同学不一定都在 D:\Febs2005\Trade\Biz 啥的
rem -U 指定还从哪里查找使用的单元文件

rem 这里替换 C_CbpETF为其他DLL
echo "%DCC%" -Q -W- -H- -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"
"%DCC%" -Q -W- -H- -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"

rem 如果编译失败，那么需要显示编译信息，以便用户确认来进行处理
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem 输出一行空行，且暂停
rem echo. & pause