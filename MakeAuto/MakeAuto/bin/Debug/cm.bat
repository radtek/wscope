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

rem ����ָ��Control��λ�ã���Ҫ���е���
set DCC5=D:\Program Files\Borland\Delphi5\Bin\DCC32.EXE
set DCC6=D:\Program Files\Borland\Delphi6\Bin\DCC32.EXE
set HSControls=E:\VSS\HSTRADES11\Sources\ClientCom\Control;
set Platform10=E:\VSS\HSTRADES11\Sources\ClientCom\Control\PLATFORM10;
set Lzrw1_5=E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\TOOLS\CONTROL\LZRW1;
set Lzrw1_6=E:\VSS\HsSettle\Sources\ClientCom\Control\LZRW1;
set DOA5=D:\Program Files\Borland\Delphi5\DOA;
set DOA6=D:\Program Files\Borland\Delphi6\DOA;

rem ָ��UnitOutPutĿ¼
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
rem ʹ��/d ǿ���л���������Դ����·��������Ӧ���Ǹ�����
rem cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF
rem ʹ�����Ҳ���� pushd E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\
pushd %ProDir%

rem ��ʼ�Զ�����
rem -B Build All
rem -E ָ�����·������Щͬѧ��һ������ D:\Febs2005\Trade\Biz ɶ��
rem -U ָ�������������ʹ�õĵ�Ԫ�ļ�
rem �����滻 C_CbpETFΪ����DLL
rem echo "%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -E"%OutPut%"
"%DCC%" -B "%ProName%" -U"%UPath%" -N"%DCUDir%" -E"%OutPut%"

rem �������ʧ�ܣ���ô��Ҫ��ʾ������Ϣ���Ա��û�ȷ�������д���
if %ERRORLEVEL%==0 (echo "Complile Success") else (echo "Complile Failed")

popd
rem ���һ�п��У�����ͣ
rem echo. & pause