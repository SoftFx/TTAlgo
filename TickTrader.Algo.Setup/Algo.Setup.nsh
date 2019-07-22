;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "MUI.nsh"
!include "x64.nsh"
!include "nsDialogs.nsh"

!include "Algo.Utils.nsh"
!include "Resources\Resources.en.nsi"


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

!ifndef PRODUCT_URL
    !define PRODUCT_URL "https://www.soft-fx.com/"
!endif

!ifndef LICENSE_FILE
    !define LICENSE_FILE "license.txt"
!endif

!ifndef SETUP_FILENAME
    !define SETUP_FILENAME "Algo ${PRODUCT_BUILD}.Setup.exe"
!endif

!define BASE_NAME "TickTrader"
!define BASE_INSTDIR "$PROGRAMFILES64\${BASE_NAME}"

!define OUTPUT_DIR "..\build.ouput"
!define ICONS_DIR "..\icons"

!define REG_KEY_BASE "Software\${BASE_NAME}"
!define REG_UNINSTALL_KEY_BASE "Software\Microsoft\Windows\CurrentVersion\Uninstall"

!define REG_PATH_KEY "Path"
!define REG_VERSION_KEY "Version"
!define REG_SERVICE_ID "ServiceId"
!define REG_SHORTCUT_NAME "ShortcutName"

!define FALSE 0
!define TRUE 1


;--------------------------
; Common variables
var Void


;--------------------------
; Components definition

!include "Algo.Terminal.nsh"
!include "Algo.Agent.nsh"
;!include "Algo.Configurator.nsh"


;--------------------------
; Directory page

Var Terminal_DirText
Var Agent_DirText

Function DirectoryPageCreate

    !insertmacro MUI_HEADER_TEXT "Choose Install Localtion" "Choose folders in which to install selected components"

    nsDialogs::Create 1018
    Pop $Void

    ${If} $Void == error
        Abort
    ${EndIf}
    
    ${NSD_CreateLabel} 0% 0u 100% 24u "Setup will install selected components in the following folders. To install in a different folder, click Browse and select another folder. Click Next to continue."
    Pop $Void

    ${NSD_CreateGroupBox} 0 36u 100% 36u "${TERMINAL_NAME} Folder"
    Pop $Void

    ${NSD_CreateDirRequest} 3% 50u 71% 13u "$Terminal_InstDir"
    Pop $Terminal_DirText

    ${NSD_CreateBrowseButton} 77% 50u 20% 14u "Browse..."
    Pop $Void
    ${NSD_OnClick} $Void Terminal_OnDirBrowse

    ${NSD_CreateGroupBox} 0 84u 100% 36u "${AGENT_NAME} Folder"
    Pop $Void

    ${NSD_CreateDirRequest} 3% 98u 71% 13u "$Agent_InstDir"
    Pop $Agent_DirText

    ${NSD_CreateBrowseButton} 77% 98u 20% 14u "Browse..."
    Pop $Void
    ${NSD_OnClick} $Void Agent_OnDirBrowse


    nsDialogs::Show

FunctionEnd

!macro DirBrowse FuncName ComponentName TextCtrl

Function ${FuncName}

    ${NSD_GetText} ${TextCtrl} $Void
    nsDialogs::SelectFolderDialog "Select ${ComponentName} Folder" $Void
    Pop $Void
    ${If} $Void != error
        ${NSD_SetText} ${TextCtrl} $Void
    ${EndIf}

FunctionEnd

!macroend

!insertmacro DirBrowse Terminal_OnDirBrowse ${TERMINAL_NAME} $Terminal_DirText
!insertmacro DirBrowse Agent_OnDirBrowse ${AGENT_NAME} $Agent_DirText

Function DirectoryPageLeave

    ${NSD_GetText} $Terminal_DirText $Terminal_InstDir
    ${NSD_GetText} $Agent_DirText $Agent_InstDir

    ${Terminal_InitId} "Install"
    ${Agent_InitId} "Install"
    StrCpy $Configurator_InstDir "$Agent_InstDir\${CONFIGURATOR_NAME}"

FunctionEnd


;--------------------------
; Shortcut page

var Terminal_ShortcutText
var Configurator_ShortcutText

Function ShortcutPageCreate

    !insertmacro MUI_HEADER_TEXT "Choose Shortcut Name" "Choose components shortcut names"

    nsDialogs::Create 1018
    Pop $Void

    ${If} $Void == error
        Abort
    ${EndIf}
    
    ${NSD_CreateLabel} 0% 0u 100% 12u "Enter name of ${TERMINAL_NAME} shortcut"
    Pop $Void

    ${NSD_CreateText} 0% 13u 100% 13u "${TERMINAL_NAME} ${PRODUCT_BUILD}"
    Pop $Terminal_ShortcutText
    ${NSD_SetText} $Terminal_ShortcutText $Terminal_ShortcutName

    ${NSD_CreateLabel} 0% 35u 100% 12u "Enter name of ${AGENT_NAME} shortcut"
    Pop $Void

    ${NSD_CreateText} 0% 48u 100% 13u "${AGENT_NAME} ${PRODUCT_BUILD}"
    Pop $Configurator_ShortcutText
    ${NSD_SetText} $Configurator_ShortcutText $Configurator_ShortcutName


    nsDialogs::Show

FunctionEnd

Function ShortcutPageLeave

    ${NSD_GetText} $Terminal_ShortcutText $Terminal_ShortcutName
    ${NSD_GetText} $Configurator_ShortcutText $Configurator_ShortcutName

FunctionEnd
