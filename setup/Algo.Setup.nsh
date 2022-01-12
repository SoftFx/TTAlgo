;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "MUI2.nsh"
!include "x64.nsh"
!include "nsDialogs.nsh"
!include "WinMessages.nsh"

!include "Algo.Utils.nsh"
!include "Resources\Resources.en.nsi"


;--------------------------
; Parameters

!ifndef PRODUCT_NAME
    !define PRODUCT_NAME "TickTrader Algo Studio"
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
    !define SETUP_FILENAME "Algo Studio ${PRODUCT_BUILD}.Setup.exe"
!endif

!define BASE_NAME "TickTrader"
!define BASE_INSTDIR "$PROGRAMFILES64\${BASE_NAME}"

!define OUTPUT_DIR "..\artifacts.build"
!define ICONS_DIR "..\icons"
!define BANNER_PATH "banner.bmp"
!define BANNER_TMP_PATH "$PLUGINSDIR\banner.bmp"

!define REG_KEY_BASE "Software\${BASE_NAME}"
!define REG_UNINSTALL_KEY_BASE "Software\Microsoft\Windows\CurrentVersion\Uninstall"

!define REG_PATH_KEY "Path"
!define REG_VERSION_KEY "Version"
!define REG_SERVICE_ID "ServiceId"
!define REG_SHORTCUT_NAME "ShortcutName"


;--------------------------
; Common variables
var Void
var OffsetY


;--------------------------
; Components definition

!include "Algo.Terminal.nsh"
!include "Algo.Server.nsh"
;!include "Algo.Configurator.nsh"


;--------------------------
; Directory page

Var Terminal_DirText
Var AlgoServer_DirText

Function DirectoryPageCreate

    ${If} $Terminal_DesktopSelected == ${FALSE}
    ${AndIf} $Terminal_StartMenuSelected == ${FALSE}
    ${AndIf} $Configurator_DesktopSelected == ${FALSE}
    ${AndIf} $Configurator_StartMenuSelected == ${FALSE}

        GetDlgItem $Void $HWNDPARENT 1
        SendMessage $Void ${WM_SETTEXT} 0 "STR:&Install"

    ${EndIf}

    !insertmacro MUI_HEADER_TEXT "Choose Install Localtion" "Choose folders in which to install selected components"

    nsDialogs::Create 1018
    Pop $Void

    ${If} $Void == error
        Abort
    ${EndIf}

    ${NSD_CreateLabel} 0% 0u 100% 24u "Setup will install selected components in the following folders. To install in a different folder, click Browse and select another folder. Click Next to continue."
    Pop $Void

    StrCpy $OffsetY "36"
    
    ${If} $Terminal_CoreSelected == ${TRUE}

        ${NSD_CreateGroupBox} 0 "$OffsetYu" 100% 36u "${TERMINAL_NAME} Folder"
        Pop $Void

        IntOp $OffsetY $OffsetY + 14

        ${NSD_CreateDirRequest} 3% "$OffsetYu" 71% 13u "$Terminal_InstDir"
        Pop $Terminal_DirText

        ${NSD_CreateBrowseButton} 77% "$OffsetYu" 20% 14u "Browse..."
        Pop $Void
        ${NSD_OnClick} $Void Terminal_OnDirBrowse

        IntOp $OffsetY $OffsetY + 34

    ${EndIf}

    ${If} $AlgoServer_CoreSelected == ${TRUE}

        ${NSD_CreateGroupBox} 0 "$OffsetYu" 100% 36u "${ALGOSERVER_NAME} Folder"
        Pop $Void

        IntOp $OffsetY $OffsetY + 14

        ${NSD_CreateDirRequest} 3% "$OffsetYu" 71% 13u "$AlgoServer_InstDir"
        Pop $AlgoServer_DirText

        ${NSD_CreateBrowseButton} 77% "$OffsetYu" 20% 14u "Browse..."
        Pop $Void
        ${NSD_OnClick} $Void AlgoServer_OnDirBrowse

    ${EndIf}

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
!insertmacro DirBrowse AlgoServer_OnDirBrowse ${ALGOSERVER_NAME} $AlgoServer_DirText

Function DirectoryPageLeave

    ${If} $Terminal_CoreSelected == ${TRUE}

        ${NSD_GetText} $Terminal_DirText $Terminal_InstDir
        ${Terminal_InitId} "Install"

    ${EndIf}

    ${If} $AlgoServer_CoreSelected == ${TRUE}

        ${NSD_GetText} $AlgoServer_DirText $AlgoServer_InstDir
        ${AlgoServer_InitId} "Install"
        StrCpy $Configurator_InstDir "$AlgoServer_InstDir\${CONFIGURATOR_NAME}"

    ${EndIf}

FunctionEnd


;--------------------------
; Shortcut page

var Terminal_ShortcutText
var Configurator_ShortcutText

Function ShortcutPageCreate

    ${If} $Terminal_DesktopSelected == ${TRUE}
    ${OrIf} $Terminal_StartMenuSelected == ${TRUE}
    ${OrIf} $Configurator_DesktopSelected == ${TRUE}
    ${OrIf} $Configurator_StartMenuSelected == ${TRUE}

        !insertmacro MUI_HEADER_TEXT "Choose Shortcut Name" "Choose components shortcut names"

        nsDialogs::Create 1018
        Pop $Void

        ${If} $Void == error
            Abort
        ${EndIf}

        StrCpy $OffsetY "0"
        
        ${If} $Terminal_DesktopSelected == ${TRUE}
        ${OrIf} $Terminal_StartMenuSelected == ${TRUE}

            ${NSD_CreateLabel} 0% "$OffsetYu" 100% 12u "Enter name of ${TERMINAL_NAME} shortcut"
            Pop $Void

            IntOp $OffsetY $OffsetY + 13

            ${NSD_CreateText} 0% "$OffsetYu" 100% 13u "$Terminal_ShortcutName"
            Pop $Terminal_ShortcutText

            IntOp $OffsetY $OffsetY + 22

        ${EndIf}

        ${If} $Configurator_DesktopSelected == ${TRUE}
        ${OrIf} $Configurator_StartMenuSelected == ${TRUE}

            ${NSD_CreateLabel} 0% "$OffsetYu" 100% 12u "Enter name of ${CONFIGURATOR_DISPLAY_NAME} shortcut"
            Pop $Void

            IntOp $OffsetY $OffsetY + 13

            ${NSD_CreateText} 0% "$OffsetYu" 100% 13u "$Configurator_ShortcutName"
            Pop $Configurator_ShortcutText

        ${EndIf}

        nsDialogs::Show

    ${EndIf}

FunctionEnd

Function ShortcutPageLeave

    ${If} $Terminal_DesktopSelected == ${TRUE}
    ${OrIf} $Terminal_StartMenuSelected == ${TRUE}

        ${NSD_GetText} $Terminal_ShortcutText $Terminal_ShortcutName

    ${EndIf}

    ${If} $Configurator_DesktopSelected == ${TRUE}
    ${OrIf} $Configurator_StartMenuSelected == ${TRUE}

        ${NSD_GetText} $Configurator_ShortcutText $Configurator_ShortcutName

    ${EndIf}

FunctionEnd


;--------------------------
; Finish page

var BannerImage
var TitleFont
var Terminal_RunCheckBox
var Configurator_RunCheckBox
var RebootNowRadioButton

Function FinishPageCreate

    GetDlgItem $Void $HWNDPARENT 1
    SendMessage $Void ${WM_SETTEXT} 0 "STR:&Finish"
    CreateFont $TitleFont "$(^Font)" "12" "700"

    nsDialogs::Create 1044
    Pop $Void
    SetCtlColors $Void "" "ffffff"

    ${NSD_CreateBitmap} 0u 0u 109u 193u ""
    Pop $Void
    ${NSD_SetStretchedImage} $Void ${BANNER_TMP_PATH} $BannerImage

    ${NSD_CreateLabel} 120u 10u -130u 30u "$(FinishPageTitle)"
    Pop $Void
    SetCtlColors $Void "" "ffffff"
    SendMessage $Void ${WM_SETFONT} $TitleFont 1

    ${NSD_CreateLabel} 120u 45u -130u 12u "$(FinishPageDescription1)"
    Pop $Void
    SetCtlColors $Void "" "ffffff"

    ${NSD_CreateLabel} 120u 60u -130u 12u "$(FinishPageDescription2)"
    Pop $Void
    SetCtlColors $Void "" "ffffff"

    StrCpy $OffsetY "90"

    ${If} $Framework_Installed == ${FALSE}

        ${NSD_CreateLabel} 120u "$OffsetYu" -130u -100u "$(FrameworkManualInstall)"
        Pop $Void
        SetCtlColors $Void "ff0000" "ffffff"

    ${Else}

        ${If} $Framework_RebootNeeded == ${TRUE}

            ${NSD_CreateLabel} 120u "$OffsetYu" -130u 24u "$(FinishPageRebootNeeded)"
            Pop $Void
            SetCtlColors $Void "" "ffffff"

            IntOp $OffsetY $OffsetY + 30

            ${NSD_CreateRadioButton} 120u "$OffsetYu" -130u 12u "$(FinishPageRebootNow)"
            Pop $RebootNowRadioButton
            SetCtlColors $RebootNowRadioButton "" "ffffff"
            ${NSD_AddStyle} $RebootNowRadioButton ${WS_GROUP}
            ${NSD_Check} $RebootNowRadioButton

            IntOp $OffsetY $OffsetY + 15

            ${NSD_CreateRadioButton} 120u "$OffsetYu" -130u 12u "$(FinishPageRebootLater)"
            Pop $Void
            SetCtlColors $Void "" "ffffff"

        ${Else}

            ${If} $Terminal_Installed == ${TRUE}

                ${NSD_CreateCheckBox} 120u "$OffsetYu" -130u 12u "Run ${TERMINAL_NAME}"
                Pop $Terminal_RunCheckBox
                SetCtlColors $Terminal_RunCheckBox "" "ffffff"
                ${NSD_Check} $Terminal_RunCheckBox

                IntOp $OffsetY $OffsetY + 15

            ${EndIf}

            ${If} $AlgoServer_ServiceCreated == ${TRUE}
            ${AndIf} $Configurator_Installed == ${TRUE}

                ${NSD_CreateCheckBox} 120u "$OffsetYu" -130u 12u "Run ${CONFIGURATOR_DISPLAY_NAME}"
                Pop $Configurator_RunCheckBox
                SetCtlColors $Configurator_RunCheckBox "" "ffffff"

                ${If} $AlgoServer_ServiceFailed == ${FALSE}
                ${AndIf} $AlgoServer_LaunchService == ${TRUE}
                    ; no need to run configurator if update went fine
                ${Else}
                    ${NSD_Check} $Configurator_RunCheckBox
                ${EndIf}

                IntOp $OffsetY $OffsetY + 15

            ${EndIf}

            ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}

                ${NSD_CreateLabel} 120u "$OffsetYu" -130u -100u $AlgoServer_ServiceError
                Pop $Void
                SetCtlColors $Void "ff0000" "ffffff"

            ${EndIf}

        ${EndIf}

    ${EndIf}

    nsDialogs::Show

    ${NSD_FreeImage} $BannerImage

FunctionEnd

Function FinishPageLeave

    ${If} $Framework_RebootNeeded == ${TRUE}

        ${NSD_GetState} $RebootNowRadioButton $Void
        ${If} $Void == ${BST_CHECKED}
            Reboot
        ${EndIf}

    ${EndIf}

    ${If} $Terminal_Installed == ${TRUE}

        ${NSD_GetState} $Terminal_RunCheckBox $Void
        ${If} $Void == ${BST_CHECKED}
            StrCpy $OUTDIR $Terminal_InstDir ; Exec command working dir is set to $OUTDIR
            Exec "$Terminal_InstDir\${TERMINAL_EXE}"
        ${EndIf}

    ${EndIf}

    ${If} $Configurator_Installed == ${TRUE}

        ${NSD_GetState} $Configurator_RunCheckBox $Void
        ${If} $Void == ${BST_CHECKED}
            StrCpy $OUTDIR $Configurator_InstDir ; Exec command working dir is set to $OUTDIR
            Exec "$Configurator_InstDir\${CONFIGURATOR_EXE}"
        ${EndIf}

    ${EndIf}

FunctionEnd
