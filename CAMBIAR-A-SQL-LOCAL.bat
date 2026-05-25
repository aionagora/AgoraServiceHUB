@echo off
echo ════════════════════════════════════════════════════════════════════════
echo  CAMBIAR A SQL SERVER LOCAL
echo ════════════════════════════════════════════════════════════════════════
echo.
echo Este script cambiara la configuracion para usar SQL Server Local
echo en lugar del servidor remoto 192.168.88.14:56885
echo.
echo ANTES:  Server=192.168.88.14,56885
echo AHORA:  Server=(localdb)\mssqllocaldb
echo.
pause

echo.
echo Haciendo backup de appsettings.json...
copy src\AgoraHub360.ERP.Api\appsettings.json src\AgoraHub360.ERP.Api\appsettings.json.backup

echo.
echo Actualizando configuracion...

powershell -Command "(Get-Content src\AgoraHub360.ERP.Api\appsettings.json) -replace 'Server=192.168.88.14,56885;Database=db_AgoraERP_Core;User Id=usagora;Password=Sinnada123.\*\*;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false', 'Server=(localdb)\\mssqllocaldb;Database=db_AgoraERP_Core;Trusted_Connection=true;TrustServerCertificate=true' | Set-Content src\AgoraHub360.ERP.Api\appsettings.json"

echo.
echo ════════════════════════════════════════════════════════════════════════
echo  ✓ CONFIGURACION ACTUALIZADA
echo ════════════════════════════════════════════════════════════════════════
echo.
echo Backup guardado en: src\AgoraHub360.ERP.Api\appsettings.json.backup
echo.
echo Ahora necesitas crear la base de datos local:
echo.
echo    cd src\AgoraHub360.ERP.Api
echo    dotnet ef database update
echo.
echo Luego ejecuta:
echo    1-iniciar-api.bat
echo.
pause
