;--------------------------
; Parameters

!define TERMINAL_NAME "BotTerminal"
!define TERMINAL_DISPLAY_NAME "${BASE_NAME} ${TERMINAL_NAME}"
!define TERMINAL_BINDIR "..\TickTrader.BotTerminal\bin\Release\"
!define TERMINAL_EXE "TickTrader.BotTerminal.exe"
!define TERMINAL_LOCK_FILE "applock"

!define REPOSITORY_DIR "AlgoRepository"


;--------------------------
; Variables

var Terminal_Id
var Terminal_Size
var Terminal_CoreSelected
var Terminal_DesktopSelected
var Terminal_StartMenuSelected
var Terminal_InstDir
var Terminal_ShortcutName
var Terminal_RegKey
var Terminal_UninstallRegKey
var Terminal_Installed

var TestCollection_Selected


;--------------------------
; Initialization

!macro _InitTerminal

    StrCpy $Terminal_InstDir "${BASE_INSTDIR}\${TERMINAL_NAME}"
    StrCpy $Terminal_ShortcutName "${TERMINAL_DISPLAY_NAME}"

    StrCpy $Terminal_RegKey "${REG_KEY_BASE}\${TERMINAL_NAME}"
    StrCpy $Terminal_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${TERMINAL_NAME}"

    StrCpy $Terminal_Id ${EMPTY_APPID}

    StrCpy $Terminal_CoreSelected ${FALSE}
    StrCpy $Terminal_DesktopSelected ${FALSE}
    StrCpy $Terminal_StartMenuSelected ${FALSE}

    StrCpy $Terminal_Installed ${FALSE}

!macroend


!define Terminal_Init '!insertmacro _InitTerminal'


;--------------------------
; Component files

!macro _UnpackTerminal

    ${Log} "Unpacking BotTerminal files to $Terminal_InstDir"
    SetOutPath $Terminal_InstDir
!ifdef DEBUG
    Sleep 3000
    File "${TERMINAL_BINDIR}\${TERMINAL_EXE}"
!else
    File /r "${TERMINAL_BINDIR}\*.*"
!endif
    File "${ICONS_DIR}\terminal.ico"

!macroend

!macro _DeleteTerminalFiles

    ${Log} "Removing BotTerminal files to $Terminal_InstDir"
    StrCpy $INSTDIR $Terminal_InstDir
!ifdef DEBUG
    Delete "$INSTDIR\${TERMINAL_EXE}"
!else
    ; Remove installed files, but leave generated
    !include Terminal.Uninstall.nsi
!endif
    Delete "$INSTDIR\terminal.ico"

!macroend

!macro _UnpackTestCollection

    ${Log} "Unpacking TestCollection files to $Terminal_InstDir"
    SetOutPath "$Terminal_InstDir\${REPOSITORY_DIR}"
    File "${OUTPUT_DIR}\TickTrader.Algo.TestCollection.ttalgo"
    File "${OUTPUT_DIR}\TickTrader.Algo.VersionTest.ttalgo"
    
!macroend

!macro _DeleteTestCollectionFiles

    Delete "$Terminal_InstDir\${REPOSITORY_DIR}\TickTrader.Algo.TestCollection.ttalgo"
    Delete "$Terminal_InstDir\{REPOSITORY_DIR}\TickTrader.Algo.VersionTest.ttalgo"

!macroend


!define Terminal_Unpack '!insertmacro _UnpackTerminal'
!define Terminal_DeleteFiles '!insertmacro _DeleteTerminalFiles'
!define TestCollection_Unpack '!insertmacro _UnpackTestCollection'
!define TestCollection_DeleteFiles '!insertmacro _DeleteTestCollectionFiles'


;--------------------------
; Shortcuts

!macro _CreateTerminalShortcuts

    ${Log} "Shortcut name: $Terminal_ShortcutName"
    ${If} $Terminal_DesktopSelected == ${TRUE}
        ${Print} "Adding Desktop Shortcut"
        CreateShortcut "$DESKTOP\$Terminal_ShortcutName.lnk" "$Terminal_InstDir\${TERMINAL_EXE}"
    ${EndIf}

    ${If} $Terminal_StartMenuSelected == ${TRUE}
        ${Print} "Adding StartMenu Shortcut"
        CreateDirectory "$SMPROGRAMS\${BASE_NAME}\${TERMINAL_NAME}\$Terminal_Id\"
        CreateShortcut "$SMPROGRAMS\${BASE_NAME}\${TERMINAL_NAME}\$Terminal_Id\$Terminal_ShortcutName.lnk" "$Terminal_InstDir\${TERMINAL_EXE}"
    ${EndIf}

!macroend

!macro _DeleteTerminalShortcuts

    ${Log} "Deleting BotTerminal shortcuts with name: $Terminal_ShortcutName"
    Delete "$DESKTOP\$Terminal_ShortcutName.lnk"
    Delete "$SMPROGRAMS\${BASE_NAME}\${TERMINAL_NAME}\$Terminal_Id\$Terminal_ShortcutName.lnk"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${TERMINAL_NAME}\$Terminal_Id\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\${TERMINAL_NAME}\"
    RMDir "$SMPROGRAMS\${BASE_NAME}\"

!macroend


!define Terminal_CreateShortcuts '!insertmacro _CreateTerminalShortcuts'
!define Terminal_DeleteShortcuts '!insertmacro _DeleteTerminalShortcuts'


;--------------------------
; Registry information

!macro _InitTerminalRegKeys

    StrCpy $Terminal_RegKey "${REG_KEY_BASE}\${TERMINAL_NAME}\$Terminal_Id"
    StrCpy $Terminal_UninstallRegKey "${REG_UNINSTALL_KEY_BASE}\${BASE_NAME} ${TERMINAL_NAME} $Terminal_Id"

!macroend

!macro _RegReadTerminal

    ReadRegStr $Terminal_ShortcutName HKLM "$Terminal_RegKey" "${REG_SHORTCUT_NAME}"

!macroend

!macro _RegWriteTerminal

    ${Log} "Writing registry keys"

    WriteRegStr HKLM "$Terminal_RegKey" "${REG_PATH_KEY}" "$Terminal_InstDir"
    WriteRegStr HKLM "$Terminal_RegKey" "${REG_VERSION_KEY}" "${PRODUCT_BUILD}"
    WriteRegStr HKLM "$Terminal_RegKey" "${REG_SHORTCUT_NAME}" "$Terminal_ShortcutName"

    WriteRegStr HKLM "$Terminal_UninstallRegKey" "DisplayName" "${TERMINAL_DISPLAY_NAME}"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "UninstallString" "$Terminal_InstDir\uninstall.exe"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "QuietUninstallString" '"$Terminal_InstDir\uninstall.exe" /S'

    WriteRegStr HKLM "$Terminal_UninstallRegKey" "InstallLocation" "$Terminal_InstDir"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "DisplayIcon" "$Terminal_InstDir\terminal.ico"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "URLInfoAbout" "${PRODUCT_URL}"
    WriteRegStr HKLM "$Terminal_UninstallRegKey" "DisplayVersion" "${PRODUCT_BUILD}"
    WriteRegDWORD HKLM "$Terminal_UninstallRegKey" "NoModify" "1"
    WriteRegDWORD HKLM "$Terminal_UninstallRegKey" "NoRepair" "1"
    WriteRegDWORD HKLM "$Terminal_UninstallRegKey" "EstimatedSize" "$Terminal_Size"

!macroend

!macro _RegDeleteTerminal

    ${Log} "Deleting registry keys"

    DeleteRegKey HKLM "$Terminal_RegKey"
    DeleteRegKey HKLM "$Terminal_UninstallRegKey"

!macroend


!define Terminal_InitRegKeys '!insertmacro _InitTerminalRegKeys'
!define Terminal_RegRead '!insertmacro _RegReadTerminal'
!define Terminal_RegWrite '!insertmacro _RegWriteTerminal'
!define Terminal_RegDelete '!insertmacro _RegDeleteTerminal'


;--------------------------
; App id management

!macro _InitTerminalId Mode

    ${FindAppIdByPath} terminal_id $Terminal_Id "${REG_KEY_BASE}\${TERMINAL_NAME}" ${REG_PATH_KEY} $Terminal_InstDir

    ${If} $Terminal_Id == ${EMPTY_APPID}
        ${If} ${Mode} == "Install"
            ${CreateAppId} $Terminal_Id
            ${Terminal_InitRegKeys}
            ${Log} "Created BotTerminal Id: $Terminal_Id"
        ${EndIf}
    ${Else}
        ${Log} "Found BotTerminal Id: $Terminal_Id"
        ${Terminal_InitRegKeys}
        ${Terminal_RegRead}
    ${EndIf}

!macroend


!define Terminal_InitId '!insertmacro _InitTerminalId'


;--------------------------
; Misc

!macro _CheckTerminalLock Msg Retry Cancel

    ${If} ${FileExists} "$Terminal_InstDir\*"
        ${Log} "BotTerminal is running"
        ${GetFileLock} $3 "$Terminal_InstDir\${TERMINAL_LOCK_FILE}"
        ${IF} $3 == ${FILE_LOCKED}
            MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION ${Msg} IDRETRY ${Retry} IDCANCEL ${Cancel}
        ${EndIf}
    ${EndIf}

!macroend

!define Terminal_CheckLock '!insertmacro _CheckTerminalLock'