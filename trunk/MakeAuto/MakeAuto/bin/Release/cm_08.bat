rem UF2.0ǰ̨����
@echo off

rem %1 ָ����һ��������ָ��ʹ��Delphi 5����Delphi 6 ������
rem %2 ָ������Ĺ�������
rem %3 ָ�������Ŀ¼
set DelphiVer=%1
set ProDir=%~dp2
set ProName=%~nx2
set OutPut=%3

rem ����DLL: 
rem cm 6 E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF\C_CbpETF.dpr C:\src
rem ����HsSettle: 
rem cm 6 E:\VSS\HsSettle\Sources\ClientCom\HsSettle\HsSettle.dpr C:\src
rem ����HsTools: 
rem cm 5 E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\TOOLS\HsTools.dpr C:\src

rem ����ָ��Control��λ�ã���Ҫ���е���: D6Home, TradeHome SettHome
set D6Home=D:\Program Files\Borland\Delphi6
set DCC6=%D6Home%\Bin\DCC32.EXE

set HsControl=E:\HSRef10\Sources\ClientCom\Control\;D:\Program Files\Raize\RC4\Lib\Delphi6
set D6UPath=%D6Home%\Lib;%D6Home%\Bin;%D6Home%\Imports;%D6Home%\Bpl;

rem ָ��UnitOutPutĿ¼
set DCUDir=C:\Obj

set DCC=%DCC6%
set UPath=%D6UPath%%HsControl%
set LUPackage=Hs08Controls
goto Make

:Make
rem ʹ��/d ǿ���л���������Դ����·��������Ӧ���Ǹ�����
rem cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF
rem ʹ�����Ҳ���� pushd E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\
pushd %ProDir%

rem ��ʼ�Զ�����
rem -B Build All
rem -E ָ�����·������Щͬѧ��һ������ D:\Febs2005\Trade\Biz ɶ��
rem -U ָ�������������ʹ�õĵ�Ԫ�ļ�
rem �����滻 C_CbpETFΪ����DLL
echo "%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"
"%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -LU"%LUPackage%"  -E"%OutPut%"

rem �������ʧ�ܣ���ô��Ҫ��ʾ������Ϣ���Ա��û�ȷ�������д���
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem ���һ�п��У�����ͣ
rem echo. & pause