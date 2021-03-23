;!define DEBUG

!include "Algo.Setup.nsh"

;--------------------------
; Main Install settings

Name "${PRODUCT_NAME}"
Icon "${ICONS_DIR}\softfx.ico"
BrandingText "${PRODUCT_PUBLISHER}"
OutFile "${OUTPUT_DIR}\${SETUP_FILENAME}"
InstallDir ${BASE_INSTDIR}

VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © ${PRODUCT_PUBLISHER} 2019"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${PRODUCT_NAME} installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${PRODUCT_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${PRODUCT_BUILD}"

VIProductVersion "${PRODUCT_BUILD}"
VIFileVersion "${PRODUCT_BUILD}"

;--------------------------
; Modern interface settings

!define MUI_ABORTWARNING
!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_ICON "${ICONS_DIR}\softfx.ico"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_UNFINISHPAGE_NOAUTOCLOSE
!define MUI_WELCOMEFINISHPAGE_BITMAP ${BANNER_PATH}
!define MUI_UNWELCOMEFINISHPAGE_BITMAP ${BANNER_PATH}

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${LICENSE_FILE}"
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE ComponentsOnLeave
!insertmacro MUI_PAGE_COMPONENTS
Page custom DirectoryPageCreate DirectoryPageLeave
Page custom ShortcutPageCreate ShortcutPageLeave
!insertmacro MUI_PAGE_INSTFILES
Page custom FinishPageCreate FinishPageLeave

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Set languages(first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------
; Installation types

InstType Standard
InstType Minimal
InstType Terminal
InstType AlgoServer
InstType Full

!define StandardInstall 0
!define MinimalInstall 1
!define TerminalInstall 2
!define AlgoServerInstall 3
!define FullInstall 4

!define StandardInstallBitFlag 1
!define MinimalInstallBitFlag 2
!define TerminalInstallBitFlag 4
!define AlgoServerInstallBitFlag 8
!define FullInstallBitFlag 16

;--------------------------
; Init

Function .onInit

    ${SetLogFile} "$TEMP\install.log"

    ${If} ${Runningx64}
        SetRegView 64
    ${EndIf}

    InitPluginsDir
    File /oname=${BANNER_TMP_PATH} "${BANNER_PATH}"

    InstTypeSetText ${StandardInstall} $(StandardInstallText)
    InstTypeSetText ${MinimalInstall} $(MinimalInstallText)
    InstTypeSetText ${TerminalInstall} $(TerminalInstallText)
    InstTypeSetText ${AlgoServerInstall} $(AlgoServerInstallText)
    InstTypeSetText ${FullInstall} $(FullInstallText)

    Call ConfigureComponents
    Call ConfigureInstallTypes

    ${Terminal_Init}
    ${AlgoServer_Init}

    StrCpy $Framework_Checked ${FALSE}

FunctionEnd

Function un.onInit

    ${SetLogFile} "$TEMP\uninstall.log"

    ${If} ${Runningx64}
        SetRegView 64
    ${EndIf}

FunctionEnd

;--------------------------
; Components

SectionGroup "Install AlgoTerminal" TerminalGroup

Section "Core files" TerminalCore

    Push $3
    Push $4

    CreateDirectory $Terminal_InstDir
    ${SetLogFile} "$Terminal_InstDir\install.log"

    ${Framework_Check}
    ${Framework_Install}

    ${Print} "Installing AlgoTerminal"
    ${Log} "AlgoTerminal Id: $Terminal_Id"

    ReadRegStr $3 HKLM "$Terminal_RegKey" "${REG_PATH_KEY}"
    ReadRegStr $4 HKLM "$Terminal_LegacyRegKey" "${REG_PATH_KEY}"

    ${If} $Terminal_InstDir == $3
        ${OrIf} $Terminal_InstDir == $4
            ${If} ${FileExists} "$Terminal_InstDir\uninstall.exe"
                ${Log} "Previous installation found"
                MessageBox MB_YESNO|MB_ICONQUESTION "$(UninstallPrevTerminal)" IDYES UninstallTerminalLabel IDNO SkipTerminalLabel
    UninstallTerminalLabel:
                ${Terminal_CheckLock} $(TerminalIsRunningInstall) UninstallTerminalLabel SkipTerminalLabel
                ${UninstallApp} $Terminal_InstDir
            ${Else}
                ${Log} "Unable to find uninstall.exe for previous installation"
                MessageBox MB_OK|MB_ICONEXCLAMATION "$(UninstallBrokenMessage)"
                Goto SkipTerminalLabel
            ${EndIf}
    ${EndIf}

    ${Terminal_Unpack}
    ${Terminal_RegWrite}
    ${Terminal_CreateShortcuts}
    WriteUninstaller "$Terminal_InstDir\uninstall.exe"

    StrCpy $Terminal_Installed ${TRUE}

    ${Log} "Finished AlgoTerminal installation"
    Goto TerminalInstallEnd
SkipTerminalLabel:
    ${Print} "Skipped AlgoTerminal installation"
TerminalInstallEnd:

    Pop $4
    Pop $3

SectionEnd

Section "Desktop Shortcut" TerminalDesktop

SectionEnd

Section "StartMenu Shortcut" TerminalStartMenu

SectionEnd

Section "Test Collection" TerminalTestCollection

    ${Print} "Installing TestCollection"
    ${TestCollection_Unpack}

SectionEnd

SectionGroupEnd


SectionGroup "Install AlgoServer" AlgoServerGroup

Section "Core files" AlgoServerCore

    Push $3
    Push $4

    CreateDirectory $AlgoServer_InstDir
    ${SetLogFile} "$AlgoServer_InstDir\install.log"

    ${Framework_Check}
    ${Framework_Install}

    ${Print} "Installing AlgoServer"
    ${Log} "AlgoServer Id: $AlgoServer_Id"

    ReadRegStr $3 HKLM "$AlgoServer_RegKey" "${REG_PATH_KEY}" ; for AlgoServer key
    ReadRegStr $4 HKLM "$AlgoServer_LegacyRegKey" "${REG_PATH_KEY}" ; for BotAgent key

    ${If} $AlgoServer_InstDir == $3
        ${OrIf} $AlgoServer_InstDir == $4
            ${Log} "Previous installation found"
            ${If} ${FileExists} "$AlgoServer_InstDir\uninstall.exe"
                ${AlgoServer_RememberServiceState}
                MessageBox MB_YESNO|MB_ICONQUESTION "$(UninstallPrevAlgoServer)" IDYES UninstallAlgoServerLabel IDNO SkipAlgoServerLabel
    UninstallAlgoServerLabel:
                ${Configurator_CheckLock} $(ConfiguratorIsRunningInstall) UninstallAlgoServerLabel SkipAlgoServerLabel
                ${AlgoServer_StopService} UninstallAlgoServerLabel SkipAlgoServerLabel
                ${UninstallApp} $AlgoServer_InstDir
            ${Else}
                ${Log} "Unable to find uninstall.exe for previous installation"
                MessageBox MB_OK|MB_ICONEXCLAMATION "$(UninstallBrokenMessage)"
                Goto SkipAlgoServerLabel
    ${EndIf}
    ${Else}
        SetRegView 32
        ReadRegStr $3 HKLM "${ALGOSERVER_LEGACY_REG_KEY}" ""
        ${If} $3 != ""
            ${Log} "Legacy installation found"
            ${If} ${FileExists} "$3\uninstall.exe"
                MessageBox MB_YESNO|MB_ICONQUESTION "$(UninstallPrevAlgoServer)" IDYES UninstallLegacyAlgoServerLabel IDNO SkipAlgoServerLabel
UninstallLegacyAlgoServerLabel:
                ${AlgoServer_StopLegacyService} UninstallLegacyAlgoServerLabel SkipAlgoServerLabel
                ${UninstallApp} $3
                ${If} $AlgoServer_InstDir != $3
                    MessageBox MB_YESNO|MB_ICONQUESTION "$(CopyLegacyInstallConfig)" IDYES CopyFilesFromLegacyVersion IDNO IgnoreFilesFromLegacyVersion
CopyFilesFromLegacyVersion:
                    ${AlgoServer_CopyConfig} $3 $AlgoServer_InstDir
IgnoreFilesFromLegacyVersion:
                ${EndIf}
            ${Else}
                ${Log} "Unable to find uninstall.exe for legacy installation"
                MessageBox MB_OK|MB_ICONEXCLAMATION "$(UninstallBrokenMessage)"
                Goto SkipAlgoServerLabel
            ${EndIf}
        ${EndIf}
        SetRegView 64
    ${EndIf}

    ${AlgoServer_Unpack}
    ${AlgoServer_RegWrite}
    ${Configurator_CreateShortcuts}
    WriteUninstaller "$AlgoServer_InstDir\uninstall.exe"

    StrCpy $Configurator_Installed ${TRUE}

    ${AlgoServer_CreateService}
    ${If} $AlgoServer_ServiceCreated == ${TRUE}
        StrCpy $AlgoServer_Installed ${TRUE}
        ${If} $AlgoServer_LaunchService == ${TRUE}
            ${AlgoServer_StartService}
        ${EndIf}
    ${EndIf}

    ${Log} "Finished AlgoServer installation"
    Goto AlgoServerInstallEnd
SkipAlgoServerLabel:
    ${Print} "Skipped AlgoServer installation"
AlgoServerInstallEnd:

    Pop $4
    Pop $3

SectionEnd

SectionGroup "Install ${CONFIGURATOR_DISPLAY_NAME}" ConfiguratorGroup

Section "Core files" ConfiguratorCore

SectionEnd

Section "Desktop Shortcut" ConfiguratorDesktop

SectionEnd

Section "StartMenu Shortcut" ConfiguratorStartMenu

SectionEnd

SectionGroupEnd

SectionGroupEnd


Section - FinishSection

SectionEnd

Section Uninstall

    ${SetLogFile} "$INSTDIR\uninstall.log"
    StrCpy $Terminal_InstDir $INSTDIR
    ${Terminal_InitId} "Uninstall"
    ${If} $Terminal_Id != ${EMPTY_APPID}

        ${Log} "Uninstalling AlgoTerminal from $INSTDIR"
    RetryUninstallTerminal:
        ${Terminal_CheckLock} $(TerminalIsRunningUninstall) RetryUninstallTerminal SkipUninstallTerminal

        ${Terminal_DeleteShortcuts}
        ${Terminal_DeleteFiles}

        ; Delete self
        Delete "$INSTDIR\uninstall.exe"

        ; Remove registry entries
        ${Terminal_RegDelete}

        ${Log} "Finished AlgoTerminal uninstallation"
        Goto TerminalUninstallEnd
    SkipUninstallTerminal:
        ${Log} "Skipped AlgoTerminal uninstallation"
        Abort $(UninstallCanceledMessage)
    TerminalUninstallEnd:

    ${EndIf}

    StrCpy $AlgoServer_InstDir $INSTDIR
    ${AlgoServer_InitId} "Uninstall"
    ${If} $AlgoServer_Id != ${EMPTY_APPID}

        ${Log} "Uninstalling AlgoServer from $INSTDIR"
    RetryUninstallAlgoServer:
        ${Configurator_CheckLock} $(ConfiguratorIsRunningUninstall) RetryUninstallAlgoServer SkipUninstallAlgoServer
        ${AlgoServer_StopService} RetryUninstallAlgoServer SkipUninstallAlgoServer
        ${AlgoServer_DeleteService}

        ${If} $AlgoServer_ServiceError != ${NO_ERR_MSG}
            Abort $AlgoServer_ServiceError
        ${EndIf}

        ${Configurator_DeleteShortcuts}
        ${AlgoServer_DeleteFiles}

        ; Delete self
        Delete "$INSTDIR\uninstall.exe"

        ; Remove registry entries
        ${AlgoServer_RegDelete}

        ${Log} "Finished AlgoServer uninstallation"
        Goto AlgoServerUninstallEnd
    SkipUninstallAlgoServer:
        ${Log} "Skipped AlgoServer uninstallation"
        Abort $(UninstallCanceledMessage)
    AlgoServerUninstallEnd:

    ${EndIf}

    ${If} $Terminal_Id == ${EMPTY_APPID}
    ${AndIf} $AlgoServer_Id == ${EMPTY_APPID}
        ${Log} "No valid installation of AlgoTerminal or AlgoServer was found"
        Abort $(UninstallUnknownPathMessage)
    ${EndIf}

SectionEnd

;--------------------------
; Components description

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN

    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalGroup} $(TerminalSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalCore} $(TerminalSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalDesktop} $(TerminalSection2Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalStartMenu} $(TerminalSection3Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalTestCollection} $(TerminalSection4Description)

    !insertmacro MUI_DESCRIPTION_TEXT ${AlgoServerGroup} $(AlgoServerSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${AlgoServerCore} $(AlgoServerSection1Description)

    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorGroup} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorCore} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorDesktop} $(ConfiguratorSection2Description)

!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------
; Components configuration

Function ConfigureComponents

    ${BeginSectionManagement}

        ${ReadOnlySection} ${TerminalCore}
        ${ReadOnlySection} ${AlgoServerCore}
        ${ReadOnlySection} ${ConfiguratorCore}
        ${ReadOnlySection} ${ConfiguratorGroup} ; configurator is always installed with AlgoServer

        ${ExpandSection} ${TerminalGroup}
        ${ExpandSection} ${AlgoServerGroup}
        ${ExpandSection} ${ConfiguratorGroup}

    ${EndSectionManagement}

    SectionGetSize ${TerminalCore} $Terminal_Size
    SectionGetSize ${AlgoServerCore} $AlgoServer_Size

FunctionEnd

Function ConfigureInstallTypes

    Push $0

    StrCpy $0 ${FullInstallBitFlag}
    ; 010000b
    SectionSetInstTypes ${TerminalTestCollection} $0

    IntOp $0 $0 | ${StandardInstallBitFlag}
    IntOp $0 $0 | ${TerminalInstallBitFlag}
    ; 010101b
    SectionSetInstTypes ${TerminalDesktop} $0
    SectionSetInstTypes ${TerminalStartMenu} $0

    IntOp $0 $0 | ${MinimalInstallBitFlag}
    ; 010111b
    SectionSetInstTypes ${TerminalCore} $0

    IntOp $0 $0 ^ ${TerminalInstallBitFlag}
    IntOp $0 $0 | ${AlgoServerInstallBitFlag}
    ; 011011b
    SectionSetInstTypes ${AlgoServerCore} $0
    SectionSetInstTypes ${ConfiguratorCore} $0

    IntOp $0 $0 ^ ${MinimalInstallBitFlag}
    ; 011001b
    SectionSetInstTypes ${ConfiguratorDesktop} $0
    SectionSetInstTypes ${ConfiguratorStartMenu} $0

    Pop $0

    SetCurInstType ${StandardInstall}

FunctionEnd

!macro DisableTerminalSections
    ${DisableSection} ${TerminalDesktop}
    ${DisableSection} ${TerminalStartMenu}
    ${DisableSection} ${TerminalTestCollection}
!macroend

!macro EnableTerminalSections
    ${EnableSection} ${TerminalDesktop}
    ${EnableSection} ${TerminalStartMenu}
    ${EnableSection} ${TerminalTestCollection}
!macroend

!macro DisableConfiguratorSections
    ${DisableSection} ${ConfiguratorDesktop}
    ${DisableSection} ${ConfiguratorStartMenu}
!macroend

!macro EnableConfiguratorSections
    ${EnableSection} ${ConfiguratorDesktop}
    ${EnableSection} ${ConfiguratorStartMenu}
!macroend

!macro DisableAlgoServerSections
    ; ${ReadOnlySection} ${ConfiguratorGroup}
    !insertmacro DisableConfiguratorSections
!macroend

!macro EnableAlgoServerSections
    ; ${EnableSection} ${ConfiguratorGroup}
    !insertmacro EnableConfiguratorSections
!macroend

Function .onSelChange

    ${BeginSectionManagement}

        ${if} $0 == ${TerminalGroup}
            ; MessageBox MB_OK "Terminal Group"
            ${If} ${SectionIsSelected} ${TerminalCore}
                ${UnselectSection} ${TerminalCore}
                !insertmacro DisableTerminalSections
            ${Else}
                ${SelectSection} ${TerminalCore}
                !insertmacro EnableTerminalSections
            ${EndIf}
        ${EndIf}

        ${if} $0 == ${AlgoServerGroup}
            ; MessageBox MB_OK "AlgoServer Group"
            ${If} ${SectionIsSelected} ${AlgoServerCore}
                ${UnselectSection} ${AlgoServerCore}
                ${UnselectSection} ${ConfiguratorCore}
                !insertmacro DisableAlgoServerSections
            ${Else}
                ${SelectSection} ${AlgoServerCore}
                ${SelectSection} ${ConfiguratorCore}
                !insertmacro EnableAlgoServerSections
            ${EndIf}
        ${EndIf}

        ${if} $0 == ${ConfiguratorGroup}
            ; MessageBox MB_OK "Configurator Group"
            ${If} ${SectionIsSelected} ${ConfiguratorCore}
                ${UnselectSection} ${ConfiguratorCore}
                !insertmacro DisableConfiguratorSections
            ${Else}
                ${SelectSection} ${ConfiguratorCore}
                !insertmacro EnableConfiguratorSections
            ${EndIf}
        ${EndIf}

        ${If} $0 == -1
            ; MessageBox MB_OK "Installation type change"
            ${If} ${SectionIsSelected} ${TerminalCore}
                !insertmacro EnableTerminalSections
            ${Else}
                !insertmacro DisableTerminalSections
            ${EndIf}
            ${If} ${SectionIsSelected} ${AlgoServerCore}
                !insertmacro EnableAlgoServerSections
            ${Else}
                !insertmacro DisableAlgoServerSections
            ${EndIf}
            ${If} ${SectionIsSelected} ${ConfiguratorCore}
                !insertmacro EnableConfiguratorSections
            ${Else}
                !insertmacro DisableConfiguratorSections
            ${EndIf}

        ${EndIf}

    ${EndSectionManagement}

FunctionEnd

;--------------------------
; Callbacks

Function ComponentsOnLeave

    ${If} ${SectionIsSelected} ${TerminalCore}
        StrCpy $Terminal_CoreSelected ${TRUE}
    ${Else}
        StrCpy $Terminal_CoreSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${TerminalDesktop}
        StrCpy $Terminal_DesktopSelected ${TRUE}
    ${Else}
        StrCpy $Terminal_DesktopSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${TerminalStartMenu}
        StrCpy $Terminal_StartMenuSelected ${TRUE}
    ${Else}
        StrCpy $Terminal_StartMenuSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${TerminalTestCollection}
        StrCpy $TestCollection_Selected ${TRUE}
    ${Else}
        StrCpy $TestCollection_Selected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${AlgoServerCore}
        StrCpy $AlgoServer_CoreSelected ${TRUE}
    ${Else}
        StrCpy $AlgoServer_CoreSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${ConfiguratorCore}
        StrCpy $Configurator_CoreSelected ${TRUE}
    ${Else}
        StrCpy $Configurator_CoreSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${ConfiguratorDesktop}
        StrCpy $Configurator_DesktopSelected ${TRUE}
    ${Else}
        StrCpy $Configurator_DesktopSelected ${FALSE}
    ${EndIf}

    ${If} ${SectionIsSelected} ${ConfiguratorStartMenu}
        StrCpy $Configurator_StartMenuSelected ${TRUE}
    ${Else}
        StrCpy $Configurator_StartMenuSelected ${FALSE}
    ${EndIf}

FunctionEnd