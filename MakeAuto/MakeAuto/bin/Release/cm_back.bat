@echo off

rem �����������ƣ�һ��Ҫ��HDTĿ¼�µ���eclipse���򣬷������ǻᱨ��JAVA�Ĵ������ͨ����������룬���������Ƚ�Ŀ¼�����л���
rem %1 ���̿ռ�
rem %2 ������
rem %3 ҵ��ģ��
rem %4 ����ļ�
set BSpace=%1
set BProject=%2
set BBiz=%3
set BOut=%4

set HDTDIR=D:\HDT
set HDT=%HDTDIR%\eclipsec.exe
set APP=com.hundsun.hdt.compile.HDTCompileApplication

pushd %HDTDIR%

if "%BBiz:~0,12%"=="functionlist"  goto FuncList

:Make
echo %HDT% -noSplash -application %APP% -data %BSPace% -hsproject %BProject% -hsbizmodule %BBiz% -output %BOut%
%HDT% -noSplash -application %APP% -data %BSPace% -hsproject %BProject% -hsbizmodule %BBiz% -output %BOut%
goto END

:FuncList
if exist %BOut%\%BBiz% (del /F %BOut%\%BBiz%)

echo %HDT% -noSplash -application %APP% -data %BSPace% -hsproject %BProject% -functionlist functionlist -output %BOut%
%HDT% -noSplash -application %APP% -data %BSPace% -hsproject %BProject% -functionlist functionlist -output %BOut%

if "%BBiz%"=="functionlist" (echo ok) else (ren %BOut%\functionlist.xml %BBiz%)

goto END

:END
popd

rem ���һ�п��У�����ͣ
rem echo. & pause