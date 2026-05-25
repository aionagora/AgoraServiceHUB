@echo off
echo ==========================================
echo  TEST DE CONEXION A SQL SERVER
echo ==========================================
echo.
echo Servidor: 192.168.88.14:56885
echo Base de Datos: db_AgoraERP_Core
echo.

powershell -Command "Test-NetConnection -ComputerName 192.168.88.14 -Port 56885"

echo.
echo ==========================================
pause
