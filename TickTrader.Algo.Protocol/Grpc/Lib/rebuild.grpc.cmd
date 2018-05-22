@echo off

set COMPILER=%~dp0..\..\..\packages\Grpc.Tools.1.12.0\tools\windows_x64\protoc.exe
set GRPC_PLUGIN=%~dp0..\..\..\packages\Grpc.Tools.1.12.0\tools\windows_x64\grpc_csharp_plugin.exe

if exist BotAgent.cs del BotAgent.cs
if exist BotAgentGrpc.cs del BotAgentGrpc.cs

if exist %COMPILER% (
	%COMPILER% --version
	%COMPILER% --csharp_out=./ bot_agent.proto --grpc_out=./ --plugin=protoc-gen-grpc=%GRPC_PLUGIN%
) else (
	echo Compiler not found!
)

pause