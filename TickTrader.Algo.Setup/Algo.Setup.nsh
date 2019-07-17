;--------------------------
; Parameters

!ifndef PRODUCT_NAME
    !define PRODUCT_NAME "TickTrader Algo"
!endif

!ifndef PRODUCT_PUBLISHER
    !define PRODUCT_PUBLISHER "SoftFX"
!endif

!ifndef PRODUCT_BUILD
    !define PRODUCT_BUILD "1.0"
!endif

!ifndef LICENSE_FILE
    !define LICENSE_FILE "license.txt"
!endif

!ifndef SETUP_FILENAME
    !define SETUP_FILENAME "Algo ${PRODUCT_BUILD}.Setup.exe"
!endif

!ifndef PRODUCT_URL
    !define PRODUCT_URL "https://www.soft-fx.com/"
!endif

!define BASE_NAME "TickTrader"
!define TERMINAL_NAME "BotTerminal"
!define AGENT_NAME "BotAgent"
!define CONFIGURATOR_NAME "Configurator"

!define REPOSITORY_DIR "AlgoRepository"
!define OUTPUT_DIR "..\build.ouput"
!define ICONS_DIR "..\icons"

!define TERMINAL_BINDIR "..\TickTrader.BotTerminal\bin\Release\"
!define AGENT_BINDIR "..\TickTrader.BotAgent\bin\Release\net462\publish"
!define CONFIGURATOR_BINDIR "..\TickTrader.BotAgent.Configurator\bin\Release\"

!ifndef TERMINAL_SIZE
    !define TERMINAL_SIZE 100
!endif

!ifndef AGENT_SIZE
    !define AGENT_SIZE 100
!endif

!ifndef CONFIGURATOR_SIZE
    !define CONFIGURATOR_SIZE 100
!endif

!define TERMINAL_EXE "TickTrader.BotTerminal.exe"
!define AGENT_EXE "TickTrader.BotAgent.exe"
!define CONFIGURATOR_EXE "TickTrader.BotAgent.Configurator.exe"

!define SERVICE_NAME "_sfxBotAgent"
!define SERVICE_DISPLAY_NAME "_sfxBotAgent"

!define BASE_INSTDIR "$PROGRAMFILES64\${BASE_NAME}"

;--------------------------
; Variables

var TerminalId
var AgentId
var AgentServiceId

var TerminalCoreInstalled
var TerminalDesktopInstalled
var TerminalStartMenuInstalled
var TerminalTestCollectionInstalled

var AgentCoreInstalled
var ConfiguratorCoreInstalled
var ConfiguratorDesktopInstalled
var ConfiguratorStartMenuInstalled

;--------------------------
; Components files

!macro UnpackTerminal

    File /r "${TERMINAL_BINDIR}\*.*"
    ;Sleep 3000

!macroend

!macro UnpackTestCollection

    ;File /r "${OUTPUT_DIR}\TickTrader.Algo.TestCollection.ttalgo"
    ;File /r "${OUTPUT_DIR}\TickTrader.Algo.VersionTest.ttalgo"
    Sleep 1500
    
!macroend

!macro UnpackAgent

    File /r "${AGENT_BINDIR}\*.*"
    File "${ICONS_DIR}\agent.ico"
    ;Sleep 5000

!macroend

!macro UnpackConfigurator

    ;File /r "${CONFIGURATOR_BINDIR}\*.*"
    Sleep 1000
    
!macroend

;--------------------------
; Registry information

!define REG_KEY_BASE "Software\${BASE_NAME}"
!define REG_UNINSTALL_KEY_BASE "Software\Microsoft\Windows\CurrentVersion\Uninstall"

!define REG_TERMINAL_KEY "${REG_KEY_BASE}\${TERMINAL_NAME}"
!define REG_TERMINAL_UNINSTALL_KEY "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${TERMINAL_NAME}"

!define REG_AGENT_KEY "${REG_KEY_BASE}\${AGENT_NAME}"
!define REG_AGENT_UNINSTALL_KEY "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${AGENT_NAME}"

!define REG_PATH_KEY "Path"
!define REG_VERSION_KEY "Version"


!macro RegWriteTerminal

    WriteRegStr HKLM "${REG_TERMINAL_KEY}\$TerminalId" "${REG_PATH_KEY}" "$INSTDIR\${TERMINAL_NAME}"
    WriteRegStr HKLM "${REG_TERMINAL_KEY}\$TerminalId" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"

    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "DisplayName" "${BASE_NAME} ${TERMINAL_NAME}"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "UninstallString" "$INSTDIR\${TERMINAL_NAME}\uninstall.exe"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "QuietUninstallString" '"$INSTDIR\${TERMINAL_NAME}\uninstall.exe" /S'

    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "InstallLocation" "$INSTDIR\${TERMINAL_NAME}"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "DisplayIcon" "$INSTDIR\${TERMINAL_NAME}\terminal.ico"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "NoModify" "1"
    WriteRegDWORD HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "NoRepair" "1"
    WriteRegDWORD HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId" "EstimatedSize" "${TERMINAL_SIZE}"

!macroend

!macro RegDeleteTerminal

    DeleteRegKey HKLM "${REG_TERMINAL_KEY}\$TerminalId"
    DeleteRegKey HKLM "${REG_TERMINAL_UNINSTALL_KEY} $TerminalId"

!macroend

!macro RegWriteAgent

    WriteRegStr HKLM "${REG_AGENT_KEY}\$AgentId" "${REG_PATH_KEY}" "$INSTDIR\${AGENT_NAME}"
    WriteRegStr HKLM "${REG_AGENT_KEY}\$AgentId" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"

    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "DisplayName" "${BASE_NAME} ${AGENT_NAME}"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "UninstallString" "$INSTDIR\${AGENT_NAME}\uninstall.exe"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "QuietUninstallString" '"$INSTDIR\${AGENT_NAME}\uninstall.exe" \S'

    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "InstallLocation" "$INSTDIR\${AGENT_NAME}"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "DisplayIcon" "$INSTDIR\${AGENT_NAME}\agent.ico"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "NoModify" "1"
    WriteRegDWORD HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "NoRepair" "1"
    WriteRegDWORD HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId" "EstimatedSize" "${AGENT_SIZE}"

!macroend

!macro RegDeleteAgent

    DeleteRegKey HKLM "${REG_AGENT_KEY}\$AgentId"
    DeleteRegKey HKLM "${REG_AGENT_UNINSTALL_KEY} $AgentId"

!macroend

;--------------------------
; App id management

!define FindTerminalId `!insertmacro FindAppIdByPath terminal_id $TerminalId ${REG_TERMINAL_KEY} ${REG_PATH_KEY}`
!define FindAgentId `!insertmacro FindAppIdByPath agent_id $AgentId ${REG_AGENT_KEY} ${REG_PATH_KEY}`


!macro InitAgentServiceId

    StrCpy $AgentServiceId "${SERVICE_NAME}_$AgentId"

!macroend

;--------------------------
; Common macros

!macro UninstallApp Path
    
    ; Copy previous uninstaller to temp location
    CreateDirectory "${Path}\tmp"
    CopyFiles /SILENT /FILESONLY "${Path}\uninstall.exe" "${Path}\tmp"
    ; Run uninstaller of previous version
    ExecWait '"${Path}\tmp\uninstall.exe" /S _?=${Path}'
    RMDir /r "${Path}\tmp"

!macroend
