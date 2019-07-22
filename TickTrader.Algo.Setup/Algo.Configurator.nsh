;--------------------------
; Parameters

!define CONFIGURATOR_NAME "BotAgent Configurator"
!define CONFIGURATOR_DISPLAY_NAME "${BASE_NAME} ${CONFIGURATOR_NAME}"
!define CONFIGURATOR_BINDIR "..\TickTrader.BotAgent.Configurator\bin\Release\"
!define CONFIGURATOR_EXE "TickTrader.BotAgent.Configurator.exe"
!define CONFIGURATOR_LOCK_FILE "applock"


;--------------------------
; Variables

var Configurator_Id
var Configurator_Size
var Configurator_CoreSelected
var Configurator_DesktopSelected
var Configurator_StartMenuSelected
var Configurator_InstDir
var Configurator_ShortcutName
var Configurator_RegKey
var Configurator_UninstallRegKey


;--------------------------
; Initialization

!macro _InitConfigurator

    StrCpy $Configurator_InstDir "${BASE_INSTDIR}\${CONFIGURATOR_NAME}"
    StrCpy $Configurator_ShortcutName "${CONFIGURATOR_NAME} ${PRODUCT_BUILD}"

    StrCpy $Configurator_RegKey "${REG_KEY_BASE}\${CONFIGURATOR_NAME}"
    StrCpy $Configurator_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${CONFIGURATOR_NAME}"

    StrCpy $Configurator_Id ${EMPTY_APPID}

    StrCpy $Configurator_CoreSelected ${FALSE}
    StrCpy $Configurator_DesktopSelected ${FALSE}
    StrCpy $Configurator_StartMenuSelected ${FALSE}

!macroend


!define Configurator_Init '!insertmacro _InitConfigurator'


;--------------------------
; Components files

!macro _UnpackConfigurator

    SetOutPath $Configurator_InstDir
!ifdef DEBUG
    Sleep 1500
    File "${CONFIGURATOR_BINDIR}\${CONFIGURATOR_EXE}"
!else
    File /r "${CONFIGURATOR_BINDIR}\*.*"
!endif
    File "${ICONS_DIR}\configurator.ico"
    
!macroend

!macro _DeleteConfiguratorFiles

    StrCpy $INSTDIR $Configurator_InstDir
!ifdef DEBUG
    Delete "$INSTDIR\${CONFIGURATOR_EXE}"
!else
    ; Remove installed files, but leave generated
    !include Configurator.Uninstall.nsi
!endif
    Delete "$INSTDIR\configurator.ico"

!macroend


!define Configurator_Unpack '!insertmacro _UnpackConfigurator'
!define Configurator_DeleteFiles 'insertmacro _DeleteConfiguratorFiles'


;--------------------------
; Shortcuts

!macro _CreateConfiguratorShortcuts

    ${If} $Configurator_DesktopSelected == ${TRUE}
        DetailPrint "Adding Desktop Shortcut"
        CreateShortcut "$DESKTOP\$Configurator_ShortcutName.lnk" "$Configurator_InstDir\${CONFIGURATOR_EXE}"
    ${EndIf}

    ${If} $Configurator_StartMenuSelected == ${TRUE}
        DetailPrint "Adding StartMenu Shortcut"
        CreateDirectory "$SMPROGRAMS\${BASE_NAME}\${CONFIGURATOR_NAME}\$Configurator_Id"
        CreateShortcut "$SMPROGRAMS\${BASE_NAME}\${CONFIGURATOR_NAME}\$Configurator_Id\$Configurator_ShortcutName.lnk" "$Configurator_InstDir\${CONFIGURATOR_EXE}"
    ${EndIf}

!macroend

!macro _DeleteConfiguratorShortcuts

    Delete "$SMPROGRAMS\${BASE_NAME}\${CONFIGURATOR_NAME}\$Configurator_Id\$Configurator_ShortcutName.lnk"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${CONFIGURATOR_NAME}\$Configurator_Id\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${CONFIGURATOR_NAME}\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\"

!macroend


!define Configurator_CreateShortcuts '!insertmacro _CreateConfiguratorShortcuts'
!define Configurator_DeleteShortcuts '!insertmacro _DeleteConfiguratorShortcuts'


;--------------------------
; Registry information

!macro _InitConfiguratorRegKeys

    StrCpy $Configurator_RegKey "${REG_KEY_BASE}\${CONFIGURATOR_NAME}\$Configurator_Id"
    StrCpy $Configurator_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${CONFIGURATOR_NAME} $Configurator_Id"

!macroend

!macro _RegReadConfigurator

    ReadRegStr $Configurator_ShortcutName HKLM "$Configurator_RegKey" "${REG_SHORTCUT_NAME}"

!macroend

!macro _RegWriteConfigurator

    WriteRegStr HKLM "$Configurator_RegKey" "${REG_PATH_KEY}" "$Configurator_InstDir"
    WriteRegStr HKLM "$Configurator_RegKey" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"
    WriteRegStr HKLM "$Configurator_RegKey" "${REG_SHORTCUT_NAME}" "$Configurator_ShortcutName"

    WriteRegStr HKLM "$Configurator_UninstallRegKey" "DisplayName" "${CONFIGURATOR_DISPLAY_NAME}"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "UninstallString" "$Configurator_InstDir\uninstall.exe"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "QuietUninstallString" '"$Configurator_InstDir\uninstall.exe" /S'

    WriteRegStr HKLM "$Configurator_UninstallRegKey" "InstallLocation" "$Configurator_InstDir"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "DisplayIcon" "$Configurator_InstDir\configurator.ico"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "$Configurator_UninstallRegKey" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "$Configurator_UninstallRegKey" "NoModify" "1"
    WriteRegDWORD HKLM "$Configurator_UninstallRegKey" "NoRepair" "1"
    WriteRegDWORD HKLM "$Configurator_UninstallRegKey" "EstimatedSize" "$Configurator_Size"

!macroend

!macro _RegDeleteConfigurator

    DeleteRegKey HKLM "$Configurator_RegKey"
    DeleteRegKey HKLM "$Configurator_UninstallRegKey"

!macroend


!define Configurator_InitRegKeys '!insertmacro _InitConfiguratorRegKeys'
!define Configurator_RegRead '!insertmacro _RegReadConfigurator'
!define Configurator_RegWrite '!insertmacro _RegWriteConfigurator'
!define Configurator_RegDelete '!insertmacro _RegDeleteConfigurator'


;--------------------------
; App id management

!macro _InitConfiguratorId Mode

    ${FindAppIdByPath} configurator_id $Configurator_Id "${REG_KEY_BASE}\${CONFIGURATOR_NAME}" ${REG_PATH_KEY} $Configurator_InstDir

    ${If} $Configurator_Id == ${EMPTY_APPID}
        ${If} ${Mode} == "Install"
            ${CreateAppId} $Configurator_Id
            ${Configurator_InitRegKeys}
        ${EndIf}
    ${Else}
        ${Configurator_InitRegKeys}
        ${Configurator_RegRead}
    ${EndIf}

!macroend


!define Configurator_InitId '!insertmacro _InitConfiguratorId'


;--------------------------
; Misc

!macro _CheckConfiguratorLock Msg Retry Cancel

    ${GetFileLock} $3 "$Configurator_InstDir\${CONFIGURATOR_LOCK_FILE}"
    ${IF} $3 == ${FILE_LOCKED}
        MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION ${Msg} IDRETRY ${Retry} IDCANCEL ${Cancel}
    ${EndIf}

!macroend

!define Configurator_CheckLock '!insertmacro _CheckConfiguratorLock'