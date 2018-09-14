@echo off

set COMPILER=%~dp0..\..\..\packages\Grpc.Tools.1.14.1\tools\windows_x64\protoc.exe
set GRPC_PLUGIN=%~dp0..\..\..\packages\Grpc.Tools.1.14.1\tools\windows_x64\grpc_csharp_plugin.exe

if exist Descriptors.cs del Descriptors.cs
if exist Config.cs del Config.cs
if exist Metadata.cs del Metadata.cs
if exist BotAgent.cs del BotAgent.cs
if exist BotAgentGrpc.cs del BotAgentGrpc.cs

if exist %COMPILER% (
	%COMPILER% --version
	%COMPILER% --proto_path=./ --csharp_out=./  descriptors.proto config.proto metadata.proto keys.proto
	%COMPILER% --proto_path=./ --csharp_out=./  --grpc_out=./ --plugin=protoc-gen-grpc=%GRPC_PLUGIN% bot_agent.proto
) else (
	echo Compiler not found!
)

pause