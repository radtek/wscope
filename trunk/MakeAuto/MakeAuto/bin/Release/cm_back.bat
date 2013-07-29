@echo off

rem 开发工具限制，一定要在HDT目录下调用eclipse程序，否则总是会报下JAVA的错误，因此通过批处理编译，批处理中先将目录进行切换。
rem %1 工程空间
rem %2 工程名
rem %3 业务模块
rem %4 输出文件
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

rem 输出一行空行，且暂停
rem echo. & pause