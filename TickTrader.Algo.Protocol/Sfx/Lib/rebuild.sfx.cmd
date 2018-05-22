@echo off

set COMPILER=%~dp0..\..\..\tools\rsc.exe

if exist BotAgent.cs del BotAgent.cs

if exist %COMPILER% (
	%COMPILER%
	%COMPILER% -t cs -o BotAgent.cs BotAgent.net
) else (
	echo Compiler not found!
)

pause