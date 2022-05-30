@cls
@echo ********************************************************
@echo *******      ESTADO REPLICACIÓN NODO LOCAL       *******
@echo ********************************************************
@echo off
SET estado=0
"C:\Program Files\MySQL\MySQL Server 5.6\bin\mysql" -uroot -pcd40 -f -n < ".\replication_status.sql" > estado.txt
if "%ERRORLEVEL%"=="0" goto onLocal
goto end

:onLocal
SET estado=2
goto end

:remoto
@echo ********************************************************
@echo *******      ESTADO REPLICACIÓN NODO REMOTO       ******
@echo ********************************************************
"C:\Program Files\MySQL\MySQL Server 5.6\bin\mysql" -h%1 -uroot -pcd40 -f -n < ".\replication_status.sql" >> estado.txt
if "%ERRORLEVEL%"=="0" goto on
goto end

:on
SET /a estado="%estado%"+1 

:end
echo *****************************************************************
echo "  ESTADO  | ESTADO SERVIDOR LOCAL | ESTADO SERVIDOR REMOTO"
echo --------------------------------------------------------------
echo 	0		OFF			OFF
echo 	1		OFF			ON
echo 	2		ON			OFF
echo 	3		ON			ON
echo *****************************************************************
echo %estado%
echo %estado% >> estado.txt
EXIT /B %estado%

rem pause

