@echo off
rem
rem
rem 这里指定Control的位置，需要自行调整，也应该是个参数
set Control=E:\VSS\HSTRADES11\Sources\ClientCom\Control;

rem 使用/d 强制切换驱动器到源代码路径，这里应该是个参数
cd /d E:\VSS\HSTRADES11\Sources\ClientCom\Subsys\Secu\CbpETF

rem 开始自动编译
rem -B Build All
rem -E 指定输出路径，有些同学不一定都在 D:\Febs2005\Trade\Biz 啥的
rem -U 指定还从哪里查找使用的单元文件
rem 这里替换 C_CbpETF为其他DLL
"D:\Program Files\Borland\Delphi6\Bin\DCC32.EXE" -B "C_CbpETF.dpr" -U"%Control%" -E"E:\MakeAuto\MakeAuto\bin\depbin"

rem 输出一行空行，且暂停
echo. & pause