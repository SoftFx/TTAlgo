;--------------------------
; Parameters

!define AGENT_NAME "BotAgent"
!define AGENT_DISPLAY_NAME "${BASE_NAME} ${AGENT_NAME}"
!define AGENT_BINDIR "..\TickTrader.BotAgent\bin\Release\net462\publish"
!define AGENT_EXE "TickTrader.BotAgent.exe"

!define SERVICE_NAME_BASE "_sfxBotAgent"
!define SERVICE_DISPLAY_NAME "_sfxBotAgent"

!define CONFIGURATOR_NAME "Configurator"
!define CONFIGURATOR_DISPLAY_NAME "${AGENT_NAME} config tool"
!define CONFIGURATOR_EXE "TickTrader.BotAgent.Configurator.exe"
!define CONFIGURATOR_LOCK_FILE "applock"

!define AGENT_LEGACY_REG_KEY "Software\TickTrader Bot Agent"
!define AGENT_LEGACY_SERVICE_NAME "_sfxBotAgent"


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
var Agent_LaunchService
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
    StrCpy $Agent_ServiceId "${SERVICE_NAME_BASE}_$Agent_Id"

    StrCpy $Agent_CoreSelected ${FALSE}

    StrCpy $Agent_Installed ${FALSE}

    StrCpy $Agent_LaunchService ${FALSE}
    StrCpy $Agent_ServiceCreated ${FALSE}
    StrCpy $Agent_ServiceFailed ${FALSE}
    StrCpy $Agent_ServiceError ${NO_ERR_MSG}

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

    Push $OUTDIR
    StrCpy $OUTDIR $Configurator_InstDir ; Working directory for CreateShortcut is taken from $OUTDIR

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

    Pop $OUTDIR

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
    ${Log} "Main registry keys location: $Agent_RegKey"
    ${Log} "Uninstall registry keys location: $Agent_UninstallRegKey"

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
    
    StrCpy $Agent_ServiceId "${SERVICE_NAME_BASE}_$Agent_Id"

!macroend


!define Agent_InitId '!insertmacro _InitAgentId'


;--------------------------
; Service management

!macro _CreateAgentService

    DetailPrint "Creating BotAgent service"
    ${Log} "Creating BotAgent service $Agent_ServiceId"
    ${InstallService} $Agent_ServiceId "${SERVICE_DISPLAY_NAME} ${PRODUCT_BUILD}" "16" "2" "$Agent_InstDir\${AGENT_EXE}" 80 $Agent_ServiceError
    ${If} $Agent_ServiceError == ${NO_ERR_MSG}
        ${ConfigureService} $Agent_ServiceId "${AGENT_DISPLAY_NAME} ${PRODUCT_BUILD} $Agent_InstDir" $Agent_ServiceError
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

    DetailPrint "Starting BotAgent service"
    ${Log} "Starting BotAgent service $Agent_ServiceId"
    ${StartService} $Agent_ServiceId 30 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        StrCpy $Agent_ServiceFailed ${TRUE}
        ${Log} $Agent_ServiceError
    ${Else}
        ${Log} "Started BotAgent service $Agent_ServiceId"
    ${EndIf}

!macroend

!macro _StopAgentService Retry Cancel

    DetailPrint "Stopping BotAgent service"
    ${Log} "Stopping BotAgent service $Agent_ServiceId"
    ${StopService} $Agent_ServiceId 80 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        ${Log} $Agent_ServiceError
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $Agent_ServiceError IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${Else}
        ${Log} "Stopped BotAgent service $Agent_ServiceId"
    ${EndIf}

!macroend

!macro _DeleteAgentService

    DetailPrint "Deleting BotAgent service"
    ${Log} "Deleting BotAgent service $Agent_ServiceId"
    ${UninstallService} $Agent_ServiceId 80 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        ${Log} $Agent_ServiceError
    ${Else}
        ${Log} "Deleted BotAgent service $Agent_ServiceId"
    ${EndIf}

!macroend

!macro _RememberAgentServiceState

    ${Log} "Checking if BotAgent service $Agent_ServiceId is running"
    ${IsServiceRunning} $Agent_ServiceId $Agent_LaunchService
    ${If} $Agent_LaunchService == ${TRUE}
        ${Log} "BotAgent service $Agent_ServiceId was running"
    ${Else}
        ${Log} "BotAgent service $Agent_ServiceId wasn't running"
    ${EndIf}

!macroend

!macro _StopLegacyAgentService Retry Cancel

    DetailPrint "Stopping BotAgent service"
    ${Log} "Stopping legacy BotAgent service ${AGENT_LEGACY_SERVICE_NAME}"
    ${StopService} ${AGENT_LEGACY_SERVICE_NAME} 80 $Agent_ServiceError
    ${If} $Agent_ServiceError != ${NO_ERR_MSG}
        ${Log} $Agent_ServiceError
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $Agent_ServiceError IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${Else}
        ${Log} "Stopped legacy BotAgent service $Agent_ServiceId"
    ${EndIf}

!macroend


!define Agent_CreateService '!insertmacro _CreateAgentService'
!define Agent_StartService '!insertmacro _StartAgentService'
!define Agent_StopService '!insertmacro _StopAgentService'
!define Agent_DeleteService '!insertmacro _DeleteAgentService'
!define Agent_RememberServiceState '!insertmacro _RememberAgentServiceState'
!define Agent_StopLegacyService '!insertmacro _StopLegacyAgentService'


;--------------------------
; Misc

!macro _CheckConfiguratorLock Msg Retry Cancel

    ${If} ${FileExists} "$Configurator_InstDir\*"
        ${GetFileLock} $3 "$Configurator_InstDir\${CONFIGURATOR_LOCK_FILE}"
        ${IF} $3 == ${FILE_LOCKED}
            ${Log} "Configurator is running ($Configurator_InstDir)"
            MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION ${Msg} IDRETRY ${Retry} IDCANCEL ${Cancel}
        ${EndIf}
    ${EndIf}

!macroend

!macro _CopyAgentConfig SrcDir DstDir

    CreateDirectory "${DstDir}\AlgoData"
    CreateDirectory "${DstDir}\AlgoRepository"
    CreateDirectory "${DstDir}\WebAdmin"

    CopyFiles "${SrcDir}\AlgoData\*.*" "${DstDir}\AlgoData"
    CopyFiles "${SrcDir}\AlgoRepository\*.ttalgo" "${DstDir}\AlgoRepository"
    CopyFiles "${SrcDir}\WebAdmin\appsettings.json" "${DstDir}\WebAdmin"
    CopyFiles "${SrcDir}\server.config.xml" "${DstDir}\server.config.xml"

!macroend

!define Configurator_CheckLock '!insertmacro _CheckConfiguratorLock'
!define Agent_CopyConfig '!insertmacro _CopyAgentConfig'