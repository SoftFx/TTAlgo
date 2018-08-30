@echo off

set NUGET=..\..\tools\nuget.exe

@echo ========================================
@echo packing %1 v. %2
@echo ========================================

if exist ..\%1.%2.nupkg del ..\%1.%2.nupkg
%NUGET% pack %1.nuspec -OutputDirectory ..\ -Version %2 -Verbosity detailed