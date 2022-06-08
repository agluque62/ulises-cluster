@echo off
powershell -ExecutionPolicy bypass -File msiversion.ps1 ".\bin\Release\UlisesClusterSetup.msi"
pause
@echo on

