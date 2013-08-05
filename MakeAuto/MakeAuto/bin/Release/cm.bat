@echo off

rem %1 ָ����һ��������ָ��ʹ��Delphi 5����Delphi 6 ������
rem %2 ָ������Ĺ�������
rem %3 ָ�������Ŀ¼
set DelphiVer=%1
set ProDir=%~dp2
set ProName=%~nx2
set OutPut=%3

rem ����DLL: 
rem cm.bat 6 E:\06trade\HSTRADES11\trunk\Sources\ClientCom\Subsys\Secu\CbpETF\C_CbpETF.dpr C:\src
rem ����HsSettle: 
rem cm.bat 6 E:\06trade\HsSettle\trunk\Sources\ClientCom\HsSettle\HsSettle.dpr C:\src
rem ����HsTools: 
rem cm.bat 5 E:\06trade\HSTRADES11\trunk\Sources\ClientCom\Subsys\TOOLS\HsTools.dpr C:\src

rem ����ָ��Control��λ�ã���Ҫ���е���: D5Home, D6Home, TradeHome SettHome
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

rem ָ��UnitOutPutĿ¼
set DCUDir=%TradeHome%\Sources\ClientCom\Obj

if "%DelphiVer%"=="6" goto D6
:D5
set DCC=%DCC5%
set UPath=%D5UPath%%Lzrw1_5%%Platform10%
rem Platform���������룬��������� ȥ�� LUPackage=Platform10 �Ĵ�������
set LUPackage=
goto Make

:D6
set DCC=%DCC6%
set UPath=%D6UPath%%Lzrw1_6%%HsControls%
set LUPackage=HsControls
goto Make

:Make
rem ʹ��/d ǿ���л���������Դ����·��������Ӧ���Ǹ�����
rem cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF
rem ʹ�����Ҳ���� pushd E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\
pushd %ProDir%

rem ��ʼ�Զ�����
rem -Q -W- -H- -Q����ģʽ���������; -W- ��������棻-H- �����Hint������������л�������
rem -B Build All
rem -E ָ�����·������Щͬѧ��һ������ D:\Febs2005\Trade\Biz ɶ��
rem -U ָ�������������ʹ�õĵ�Ԫ�ļ�

rem �����滻 C_CbpETFΪ����DLL
echo "%DCC%" -Q -W- -H- -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"
"%DCC%" -Q -W- -H- -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"

rem �������ʧ�ܣ���ô��Ҫ��ʾ������Ϣ���Ա��û�ȷ�������д���
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem ���һ�п��У�����ͣ
rem echo. & pause