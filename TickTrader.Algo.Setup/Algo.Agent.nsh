;--------------------------
; Parameters

!define AGENT_NAME "BotAgent"
!define AGENT_DISPLAY_NAME "${BASE_NAME} ${AGENT_NAME}"
!define AGENT_BINDIR "..\TickTrader.BotAgent\bin\Release\net462\publish"
!define AGENT_EXE "TickTrader.BotAgent.exe"

!define SERVICE_NAME "_sfxBotAgent"
!define SERVICE_DISPLAY_NAME "_sfxBotAgent"

!define CONFIGURATOR_NAME "Configurator"
!define CONFIGURATOR_DISPLAY_NAME "${AGENT_NAME} config tool"
!define CONFIGURATOR_EXE "TickTrader.BotAgent.Configurator.exe"
!define CONFIGURATOR_LOCK_FILE "applock"


;--------------------------
; Variables

var Agent_Id
var Agent_ServiceId
var Agent_Size
var Agent_CoreSelected
var Agent_InstDir
var Agent_RegKey
var Agent_UninstallRegKey
var Agent_Installed
var Agent_StartService
var Agent_ServiceCreated
var Agent_ServiceFailed
var Agent_ServiceError

var Configurator_CoreSelected
var Configurator_DesktopSelected
var Configurator_StartMenuSelected
var Configurator_InstDir
var Configurator_ShortcutName
var Configurator_Installed


;--------------------------
; Initialization

!macro _InitAgent

    StrCpy $Agent_InstDir "${BASE_INSTDIR}\${AGENT_NAME}"

    StrCpy $Agent_RegKey "${REG_KEY_BASE}\${AGENT_NAME}"
    StrCpy $Agent_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${AGENT_NAME}"

    StrCpy $Agent_Id ${EMPTY_APPID}
    StrCpy $Agent_ServiceId "${SERVICE_NAME}_$Agent_Id"

    StrCpy $Agent_CoreSelected ${FALSE}

    StrCpy $Agent_Installed ${FALSE}

    StrCpy $Agent_StartService ${FALSE}
    StrCpy $Agent_ServiceCreated ${FALSE}
    StrCpy $Agent_ServiceFailed ${FALSE}
    StrCpy $Agent_ServiceError ${FALSE}

    StrCpy $Configurator_InstDir "$Agent_InstDir\${CONFIGURATOR_NAME}"
    StrCpy $Configurator_ShortcutName "${CONFIGURATOR_DISPLAY_NAME}"

    StrCpy $Configurator_CoreSelected ${FALSE}
    StrCpy $Configurator_DesktopSelected ${FALSE}
    StrCpy $Configurator_StartMenuSelected ${FALSE}

    StrCpy $Configurator_Installed ${FALSE}

!macroend


!define Agent_Init '!insertmacro _InitAgent'


;--------------------------
; Components files

!macro _UnpackAgent

    ${Log} "Unpacking BotAgent files to $Agent_InstDir"
    SetOutPath $Agent_InstDir
!ifdef DEBUG
    Sleep 5000
    File "${AGENT_BINDIR}\${AGENT_EXE}"
!else
    File /r "${AGENT_BINDIR}\*.*"
!endif
    File "${ICONS_DIR}\agent.ico"

!macroend

!macro _DeleteAgentFiles

    ${Log} "Removing BotAgent files from $Agent_InstDir"
    StrCpy $INSTDIR $Agent_InstDir
!ifdef DEBUG
    Delete "$INSTDIR\${AGENT_EXE}"
!else
    ; Remove installed files, but leave generated
    !include Agent.Uninstall.nsi
!endif
    Delete "$INSTDIR\agent.ico"

!macroend


!define Agent_Unpack '!insertmacro _UnpackAgent'
!define Agent_DeleteFiles '!insertmacro _DeleteAgentFiles'


;--------------------------
; Shortcuts

!macro _CreateConfiguratorShortcuts

    ${Log} "Shortcut name: $Configurator_ShortcutName"
    ${If} $Configurator_DesktopSelected == ${TRUE}
        ${Print} "Adding Desktop Shortcut"
        CreateShortcut "$DESKTOP\$Configurator_ShortcutName.lnk" "$Configurator_InstDir\${CONFIGURATOR_EXE}"
    ${EndIf}

    ${If} $Configurator_StartMenuSelected == ${TRUE}
        ${Print} "Adding StartMenu Shortcut"
        CreateDirectory "$SMPROGRAMS\${BASE_NAME}\${AGENT_NAME}\$Agent_Id"
        CreateShortcut "$SMPROGRAMS\${BASE_NAME}\${AGENT_NAME}\$Agent_Id\$Configurator_ShortcutName.lnk" "$Configurator_InstDir\${CONFIGURATOR_EXE}"
    ${EndIf}

!macroend

!macro _DeleteConfiguratorShortcuts
    
    ${Log} "Deleting Configurator shortcuts with name: $Configurator_ShortcutName"
    Delete "$DESKTOP\$Configurator_ShortcutName.lnk"
    Delete "$SMPROGRAMS\${BASE_NAME}\${AGENT_NAME}\$Agent_Id\$Configurator_ShortcutName.lnk"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${AGENT_NAME}\$Agent_Id\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${AGENT_NAME}\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\"

!macroend


!define Configurator_CreateShortcuts '!insertmacro _CreateConfiguratorShortcuts'
!define Configurator_DeleteShortcuts '!insertmacro _DeleteConfiguratorShortcuts'


;--------------------------
; Registry information

!macro _InitAgentRegKeys

    StrCpy $Agent_RegKey "${REG_KEY_BASE}\${AGENT_NAME}\$Agent_Id"
    StrCpy $Agent_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${AGENT_NAME} $Agent_Id"

!macroend

!macro _RegReadAgent

    ReadRegStr $Configurator_ShortcutName HKLM "$Agent_RegKey" "${REG_SHORTCUT_NAME}"

!macroend

!macro _RegWriteAgent

    ${Log} "Writing registry keys"

    WriteRegStr HKLM "$Agent_RegKey" "${REG_PATH_KEY}" "$Agent_InstDir"
    WriteRegStr HKLM "$Agent_RegKey" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"
    WriteRegStr HKLM "$Agent_RegKey" "${REG_SERVICE_ID}" "$Agent_ServiceId"
    WriteRegStr HKLM "$Agent_RegKey" "${REG_SHORTCUT_NAME}" "$Configurator_ShortcutName"

    WriteRegStr HKLM "$Agent_UninstallRegKey" "DisplayName" "${AGENT_DISPLAY_NAME}"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "UninstallString" "$Agent_InstDir\uninstall.exe"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "QuietUninstallString" '"$Agent_InstDir\uninstall.exe" \S'

    WriteRegStr HKLM "$Agent_UninstallRegKey" "InstallLocation" "$Agent_InstDir"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "DisplayIcon" "$Agent_InstDir\agent.ico"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "$Agent_UninstallRegKey" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "$Agent_UninstallRegKey" "NoModify" "1"
    WriteRegDWORD HKLM "$Agent_UninstallRegKey" "NoRepair" "1"
    WriteRegDWORD HKLM "$Agent_UninstallRegKey" "EstimatedSize" "$Agent_Size"

!macroend

!macro _RegDeleteAgent

    ${Log} "Deleting registry keys"

    DeleteRegKey HKLM "$Agent_RegKey"
    DeleteRegKey HKLM "$Agent_UninstallRegKey"

!macroend


!define Agent_InitRegKeys '!insertmacro _InitAgentRegKeys'
!define Agent_RegRead '!insertmacro _RegReadAgent'
!define Agent_RegWrite '!insertmacro _RegWriteAgent'
!define Agent_RegDelete '!insertmacro _RegDeleteAgent'


;--------------------------
; App id management

!macro _InitAgentId Mode

    ${FindAppIdByPath} agent_id $Agent_Id "${REG_KEY_BASE}\${AGENT_NAME}" ${REG_PATH_KEY} $Agent_InstDir

    ${If} $Agent_Id == ${EMPTY_APPID}
        ${If} ${Mode} == "Install"
            ${CreateAppId} $Agent_Id
            ${Agent_InitRegKeys}
            ${Log} "Created BotAgent Id: $Agent_Id"
        ${EndIf}
    ${Else}
        ${Log} "Found BotAgent Id: $Agent_Id"
        ${Agent_InitRegKeys}
        ${Agent_RegRead}
    ${EndIf}
    
    StrCpy $Agent_ServiceId "${SERVICE_NAME}_$Agent_Id"

!macroend


!define Agent_InitId '!insertmacro _InitAgentId'


;--------------------------
; Service management

!macro _CreateAgentService

    ${Print} "Creating BotAgent service"
    ${InstallService} $Agent_ServiceId "${SERVICE_DISPLAY_NAME}" "16" "2" "$Agent_InstDir\${AGENT_EXE}" 80 $Agent_ServiceError
    ${If} $Agent_ServiceError == ${NO_ERR_MSG}
        ${ConfigureService} $Agent_ServiceId $Agent_ServiceError
        ${If} $Agent_ServiceError == ${NO_ERR_MSG}
            ${Log} "Created BotAgent service"
            StrCpy $Agent_ServiceCreated ${TRUE}
        ${Else}
            ${Log} $Agent_ServiceError
        ${EndIf}
    ${Else}
        ${Log} $Agent_ServiceError
    ${EndIf}

!macroend

!macro _StartAgentService

    ${Print} "Starting BotAgent service"
    ${StartService} $Agent_ServiceId 30 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        StrCpy $Agent_ServiceFailed ${TRUE}
        ${Log} $Agent_ServiceError
    ${Else}
        ${Log} "Started BotAgent service"
    ${EndIf}

!macroend

!macro _StopAgentService Retry Cancel

    ${Print} "Stopping BotAgent service"
    ${StopService} $Agent_ServiceId 80 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        ${Log} $Agent_ServiceError
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $Agent_ServiceError IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${Else}
        ${Log} "Stopped BotAgent service"
    ${EndIf}

!macroend

!macro _DeleteAgentService

    ${Print} "Deleting BotAgent service"
    ${UninstallService} $Agent_ServiceId 80 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        ${Log} $Agent_ServiceError
    ${Else}
        ${Log} "Deleted BotAgent service"
    ${EndIf}

!macroend


!define Agent_CreateService '!insertmacro _CreateAgentService'
!define Agent_StartService '!insertmacro _StartAgentService'
!define Agent_StopService '!insertmacro _StopAgentService'
!define Agent_DeleteService '!insertmacro _DeleteAgentService'


;--------------------------
; Misc

!macro _CheckConfiguratorLock Msg Retry Cancel

    ${If} ${FileExists} "$Configurator_InstDir\*"
        ${Log} "Configurator is running"
        ${GetFileLock} $3 "$Configurator_InstDir\${CONFIGURATOR_LOCK_FILE}"
        ${IF} $3 == ${FILE_LOCKED}
            MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION ${Msg} IDRETRY ${Retry} IDCANCEL ${Cancel}
        ${EndIf}
    ${EndIf}

!macroend

!define Configurator_CheckLock '!insertmacro _CheckConfiguratorLock'