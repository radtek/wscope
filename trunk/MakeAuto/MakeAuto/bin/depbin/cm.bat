@echo off
rem
rem
rem ����ָ��Control��λ�ã���Ҫ���е�����ҲӦ���Ǹ�����
set Control=E:\VSS\HSTRADES11\Sources\ClientCom\Control;

rem ʹ��/d ǿ���л���������Դ����·��������Ӧ���Ǹ�����
cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF

rem ��ʼ�Զ�����
rem -B Build All
rem -E ָ�����·������Щͬѧ��һ������ D:\Febs2005\Trade\Biz ɶ��
rem -U ָ�������������ʹ�õĵ�Ԫ�ļ�
rem �����滻 C_CbpETFΪ����DLL
"D:\Program Files\Borland\Delphi6\Bin\DCC32.EXE" -B "C_CbpETF.dpr" -U"%Control%" -E"E:\MakeAuto\MakeAuto\bin\depbin"

rem ���һ�п��У�����ͣ
echo. & pause