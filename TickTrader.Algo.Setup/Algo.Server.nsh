;--------------------------
; Parameters

!define ALGOSERVER_NAME "AlgoServer"
!define ALGOSERVER_DISPLAY_NAME "${BASE_NAME} ${ALGOSERVER_NAME}"
!define ALGOSERVER_BINDIR "..\TickTrader.BotAgent\bin\Release\net472\publish"
!define ALGOSERVER_EXE "TickTrader.AlgoServer.exe"

!define SERVICE_NAME_BASE "_TTAlgoServer"
!define SERVICE_DISPLAY_NAME "_TTAlgoServer"

!define CONFIGURATOR_NAME "Configurator"
!define CONFIGURATOR_DISPLAY_NAME "${ALGOSERVER_NAME} config tool"
!define CONFIGURATOR_EXE "TickTrader.AlgoServer.Configurator.exe"
!define CONFIGURATOR_LOCK_FILE "applock"

!define ALGOSERVER_LEGACY_REG_KEY "Software\TickTrader Bot Agent"
!define ALGOSERVER_LEGACY_SERVICE_NAME "_sfxBotAgent"
!define ALGOSERVER_LEGACY_NAME "BotAgent"

;--------------------------
; Variables

var AlgoServer_Id
var AlgoServer_ServiceId
var AlgoServer_Size
var AlgoServer_CoreSelected
var AlgoServer_InstDir
var AlgoServer_RegKey
var AlgoServer_LegacyRegKey
var AlgoServer_UninstallRegKey
var AlgoServer_Installed
var AlgoServer_LaunchService
var AlgoServer_ServiceCreated
var AlgoServer_ServiceFailed
var AlgoServer_ServiceError

var Configurator_CoreSelected
var Configurator_DesktopSelected
var Configurator_StartMenuSelected
var Configurator_InstDir
var Configurator_ShortcutName
var Configurator_Installed


;--------------------------
; Initialization

!macro _InitAlgoServer

    StrCpy $AlgoServer_InstDir "${BASE_INSTDIR}\${ALGOSERVER_NAME}"

    StrCpy $AlgoServer_RegKey "${REG_KEY_BASE}\${ALGOSERVER_NAME}"
    StrCpy $AlgoServer_LegacyRegKey "${REG_KEY_BASE}\${ALGOSERVER_LEGACY_NAME}"
    StrCpy $AlgoServer_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${ALGOSERVER_NAME}"

    StrCpy $AlgoServer_Id ${EMPTY_APPID}
    StrCpy $AlgoServer_ServiceId "${SERVICE_NAME_BASE}_$AlgoServer_Id"

    StrCpy $AlgoServer_CoreSelected ${FALSE}

    StrCpy $AlgoServer_Installed ${FALSE}

    StrCpy $AlgoServer_LaunchService ${FALSE}
    StrCpy $AlgoServer_ServiceCreated ${FALSE}
    StrCpy $AlgoServer_ServiceFailed ${FALSE}
    StrCpy $AlgoServer_ServiceError ${NO_ERR_MSG}

    StrCpy $Configurator_InstDir "$AlgoServer_InstDir\${CONFIGURATOR_NAME}"
    StrCpy $Configurator_ShortcutName "${CONFIGURATOR_DISPLAY_NAME}"

    StrCpy $Configurator_CoreSelected ${FALSE}
    StrCpy $Configurator_DesktopSelected ${FALSE}
    StrCpy $Configurator_StartMenuSelected ${FALSE}

    StrCpy $Configurator_Installed ${FALSE}

!macroend


!define AlgoServer_Init '!insertmacro _InitAlgoServer'


;--------------------------
; Components files

!macro _UnpackAlgoServer

    ${Log} "Unpacking AlgoServer files to $AlgoServer_InstDir"
    SetOutPath $AlgoServer_InstDir
!ifdef DEBUG
    Sleep 5000
    File "${ALGOSERVER_BINDIR}\${ALGOSERVER_EXE}"
!else
    File /r "${ALGOSERVER_BINDIR}\*.*"
!endif
    File "${ICONS_DIR}\agent.ico"

!macroend

!macro _DeleteAlgoServerFiles

    ${Log} "Removing AlgoServer files from $AlgoServer_InstDir"
    StrCpy $INSTDIR $AlgoServer_InstDir
!ifdef DEBUG
    Delete "$INSTDIR\${ALGOSERVER_EXE}"
!else
    ; Remove installed files, but leave generated
    !include AlgoServer.Uninstall.nsi
!endif
    Delete "$INSTDIR\agent.ico"

!macroend


!define AlgoServer_Unpack '!insertmacro _UnpackAlgoServer'
!define AlgoServer_DeleteFiles '!insertmacro _DeleteAlgoServerFiles'


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
        CreateDirectory "$SMPROGRAMS\${BASE_NAME}\${ALGOSERVER_NAME}\$AlgoServer_Id"
        CreateShortcut "$SMPROGRAMS\${BASE_NAME}\${ALGOSERVER_NAME}\$AlgoServer_Id\$Configurator_ShortcutName.lnk" "$Configurator_InstDir\${CONFIGURATOR_EXE}"
    ${EndIf}

    Pop $OUTDIR

!macroend

!macro _DeleteConfiguratorShortcuts

    ${Log} "Deleting Configurator shortcuts with name: $Configurator_ShortcutName"
    Delete "$DESKTOP\$Configurator_ShortcutName.lnk"
    Delete "$SMPROGRAMS\${BASE_NAME}\${ALGOSERVER_NAME}\$AlgoServer_Id\$Configurator_ShortcutName.lnk"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${ALGOSERVER_NAME}\$AlgoServer_Id\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${ALGOSERVER_NAME}\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\"

!macroend


!define Configurator_CreateShortcuts '!insertmacro _CreateConfiguratorShortcuts'
!define Configurator_DeleteShortcuts '!insertmacro _DeleteConfiguratorShortcuts'


;--------------------------
; Registry information

!macro _InitAlgoServerRegKeys

    StrCpy $AlgoServer_RegKey "${REG_KEY_BASE}\${ALGOSERVER_NAME}\$AlgoServer_Id"
    StrCpy $AlgoServer_LegacyRegKey "${REG_KEY_BASE}\${ALGOSERVER_LEGACY_NAME}\$AlgoServer_Id"

    StrCpy $AlgoServer_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${ALGOSERVER_NAME} $AlgoServer_Id"

!macroend

!macro _RegReadAlgoServer

    ReadRegStr $Configurator_ShortcutName HKLM "$AlgoServer_RegKey" "${REG_SHORTCUT_NAME}"

    ${Log} "Configurator Icon Name $Configurator_ShortcutName"

    ${If} $Configurator_ShortcutName == ""
        ReadRegStr $Configurator_ShortcutName HKLM "$AlgoServer_LegacyRegKey" "${REG_SHORTCUT_NAME}"
        ${Log} "Configurator Icon Name Legacy $Configurator_ShortcutName"
    ${EndIf}

!macroend

!macro _RegWriteAlgoServer

    ${Log} "Writing registry keys"
    ${Log} "Main registry keys location: $AlgoServer_RegKey"
    ${Log} "Uninstall registry keys location: $AlgoServer_UninstallRegKey"

    WriteRegStr HKLM "$AlgoServer_RegKey" "${REG_PATH_KEY}" "$AlgoServer_InstDir"
    WriteRegStr HKLM "$AlgoServer_RegKey" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"
    WriteRegStr HKLM "$AlgoServer_RegKey" "${REG_SERVICE_ID}" "$AlgoServer_ServiceId"
    WriteRegStr HKLM "$AlgoServer_RegKey" "${REG_SHORTCUT_NAME}" "$Configurator_ShortcutName"

    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "DisplayName" "${ALGOSERVER_DISPLAY_NAME}"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "UninstallString" "$AlgoServer_InstDir\uninstall.exe"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "QuietUninstallString" '"$AlgoServer_InstDir\uninstall.exe" \S'

    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "InstallLocation" "$AlgoServer_InstDir"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "DisplayIcon" "$AlgoServer_InstDir\agent.ico"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "$AlgoServer_UninstallRegKey" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "$AlgoServer_UninstallRegKey" "NoModify" "1"
    WriteRegDWORD HKLM "$AlgoServer_UninstallRegKey" "NoRepair" "1"
    WriteRegDWORD HKLM "$AlgoServer_UninstallRegKey" "EstimatedSize" "$AlgoServer_Size"

!macroend

!macro _RegDeleteAlgoServer

    ${Log} "Deleting registry keys"

    DeleteRegKey HKLM "$AlgoServer_RegKey"
    DeleteRegKey HKLM "$AlgoServer_UninstallRegKey"

!macroend


!define AlgoServer_InitRegKeys '!insertmacro _InitAlgoServerRegKeys'
!define AlgoServer_RegRead '!insertmacro _RegReadAlgoServer'
!define AlgoServer_RegWrite '!insertmacro _RegWriteAlgoServer'
!define AlgoServer_RegDelete '!insertmacro _RegDeleteAlgoServer'


;--------------------------
; App id management

!macro _InitAlgoServerId Mode

    ${FindAppIdByPath} algoServer_id $AlgoServer_Id "${REG_KEY_BASE}\${ALGOSERVER_NAME}" ${REG_PATH_KEY} $AlgoServer_InstDir

    ${If} $AlgoServer_Id == ${EMPTY_APPID}
        ${FindAppIdByPath} algoServer_legacy_id $AlgoServer_Id "${REG_KEY_BASE}\${ALGOSERVER_LEGACY_NAME}" ${REG_PATH_KEY} $AlgoServer_InstDir
    ${EndIf}

    ${If} $AlgoServer_Id == ${EMPTY_APPID}
        ${If} ${Mode} == "Install"
            ${CreateAppId} $AlgoServer_Id
            ${AlgoServer_InitRegKeys}
            ${Log} "Created AlgoServer Id: $AlgoServer_Id"
        ${EndIf}
    ${Else}
        ${Log} "Found AlgoServer Id: $AlgoServer_Id"
        ${AlgoServer_InitRegKeys}
        ${AlgoServer_RegRead}
    ${EndIf}

    StrCpy $AlgoServer_ServiceId "${SERVICE_NAME_BASE}_$AlgoServer_Id"

!macroend


!define AlgoServer_InitId '!insertmacro _InitAlgoServerId'


;--------------------------
; Service management

!macro _CreateAlgoServerService

    DetailPrint "Creating AlgoServer service"
    ${Log} "Creating AlgoServer service $AlgoServer_ServiceId"
    ${InstallService} $AlgoServer_ServiceId "${SERVICE_DISPLAY_NAME}" "16" "2" "$AlgoServer_InstDir\${ALGOSERVER_EXE}" 80 $AlgoServer_ServiceError
    ${If} $AlgoServer_ServiceError == ${NO_ERR_MSG}
        ${ConfigureService} $AlgoServer_ServiceId "${ALGOSERVER_DISPLAY_NAME} ${PRODUCT_BUILD} $AlgoServer_InstDir" $AlgoServer_ServiceError
        ${If} $AlgoServer_ServiceError == ${NO_ERR_MSG}
            ${Log} "Created AlgoServer service"
            StrCpy $AlgoServer_ServiceCreated ${TRUE}
        ${Else}
            ${Log} $AlgoServer_ServiceError
        ${EndIf}
    ${Else}
        ${Log} $AlgoServer_ServiceError
    ${EndIf}

!macroend

!macro _StartAlgoServerService

    DetailPrint "Starting AlgoServer service"
    ${Log} "Starting AlgoServer service $AlgoServer_ServiceId"
    ${StartService} $AlgoServer_ServiceId 30 $AlgoServer_ServiceError
    ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}
        StrCpy $AlgoServer_ServiceFailed ${TRUE}
        ${Log} $AlgoServer_ServiceError
    ${Else}
        ${Log} "Started AlgoServer service $AlgoServer_ServiceId"
    ${EndIf}

!macroend

!macro _StopAlgoServerService Retry Cancel

    DetailPrint "Stopping AlgoServer service"
    ${Log} "Stopping AlgoServer service $AlgoServer_ServiceId"
    ${StopService} $AlgoServer_ServiceId 80 $AlgoServer_ServiceError
    ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}
        ${Log} $AlgoServer_ServiceError
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $AlgoServer_ServiceError IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${Else}
        ${Log} "Stopped AlgoServer service $AlgoServer_ServiceId"
    ${EndIf}

!macroend

!macro _DeleteAlgoServerService

    DetailPrint "Deleting AlgoServer service"
    ${Log} "Deleting AlgoServer service $AlgoServer_ServiceId"
    ${UninstallService} $AlgoServer_ServiceId 80 $AlgoServer_ServiceError
    ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}
        ${Log} $AlgoServer_ServiceError
    ${Else}
        ${Log} "Deleted AlgoServer service $AlgoServer_ServiceId"
    ${EndIf}

!macroend

!macro _RememberAlgoServerServiceState

    ${Log} "Checking if AlgoServer service $AlgoServer_ServiceId is running"
    ${IsServiceRunning} $AlgoServer_ServiceId $AlgoServer_LaunchService
    ${If} $AlgoServer_LaunchService == ${TRUE}
        ${Log} "AlgoServer service $AlgoServer_ServiceId was running"
    ${Else}
        ${Log} "AlgoServer service $AlgoServer_ServiceId wasn't running"
    ${EndIf}

!macroend

!macro _StopLegacyAlgoServerService Retry Cancel

    DetailPrint "Stopping AlgoServer service"
    ${Log} "Stopping legacy AlgoServer service ${ALGOSERVER_LEGACY_SERVICE_NAME}"
    ${StopService} ${ALGOSERVER_LEGACY_SERVICE_NAME} 80 $AlgoServer_ServiceError
    ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}
        ${Log} $AlgoServer_ServiceError
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $AlgoServer_ServiceError IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${Else}
        ${Log} "Stopped legacy AlgoServer service $AlgoServer_ServiceId"
    ${EndIf}

!macroend


!define AlgoServer_CreateService '!insertmacro _CreateAlgoServerService'
!define AlgoServer_StartService '!insertmacro _StartAlgoServerService'
!define AlgoServer_StopService '!insertmacro _StopAlgoServerService'
!define AlgoServer_DeleteService '!insertmacro _DeleteAlgoServerService'
!define AlgoServer_RememberServiceState '!insertmacro _RememberAlgoServerServiceState'
!define AlgoServer_StopLegacyService '!insertmacro _StopLegacyAlgoServerService'


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

!macro _CopyAlgoServerConfig SrcDir DstDir

    CreateDirectory "${DstDir}\AlgoData"
    CreateDirectory "${DstDir}\AlgoRepository"
    CreateDirectory "${DstDir}\WebAdmin"

    CopyFiles "${SrcDir}\AlgoData\*.*" "${DstDir}\AlgoData"
    CopyFiles "${SrcDir}\AlgoRepository\*.ttalgo" "${DstDir}\AlgoRepository"
    CopyFiles "${SrcDir}\WebAdmin\appsettings.json" "${DstDir}\WebAdmin"
    CopyFiles "${SrcDir}\server.config.xml" "${DstDir}\server.config.xml"

!macroend

!define Configurator_CheckLock '!insertmacro _CheckConfiguratorLock'
!define AlgoServer_CopyConfig '!insertmacro _CopyAlgoServerConfig'