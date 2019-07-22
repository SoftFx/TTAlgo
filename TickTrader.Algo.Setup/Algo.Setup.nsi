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
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${TERMINAL_NAME} and ${AGENT_NAME} installer"
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

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${LICENSE_FILE}"
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE ComponentsOnLeave
!insertmacro MUI_PAGE_COMPONENTS
Page custom DirectoryPageCreate DirectoryPageLeave
Page custom ShortcutPageCreate ShortcutPageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

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
InstType Agent
InstType Full

!define StandardInstall 0
!define MinimalInstall 1
!define TerminalInstall 2
!define AgentInstall 3
!define FullInstall 4

!define StandardInstallBitFlag 1
!define MinimalInstallBitFlag 2
!define TerminalInstallBitFlag 4
!define AgentInstallBitFlag 8
!define FullInstallBitFlag 16

;--------------------------
; Init

Function .onInit

    ${If} ${Runningx64}
        SetRegView 64
    ${EndIf}

    InstTypeSetText ${StandardInstall} $(StandardInstallText)
    InstTypeSetText ${MinimalInstall} $(MinimalInstallText)
    InstTypeSetText ${TerminalInstall} $(TerminalInstallText)
    InstTypeSetText ${AgentInstall} $(AgentInstallText)
    InstTypeSetText ${FullInstall} $(FullInstallText)

    Call ConfigureComponents
    Call ConfigureInstallTypes

    ${Terminal_Init}
    ${Agent_Init}

FunctionEnd

Function un.onInit

    ${If} ${Runningx64}
        SetRegView 64
    ${EndIf}

FunctionEnd

;--------------------------
; Components

SectionGroup "Install BotTerminal" TerminalGroup

Section "Core files" TerminalCore

    Push $3

    DetailPrint "Installing BotTerminal"

    ReadRegStr $3 HKLM "$Terminal_RegKey" "${REG_PATH_KEY}"
    ${If} $Terminal_InstDir == $3
        MessageBox MB_YESNO|MB_ICONQUESTION "$(UninstallPrevTerminal)" IDYES UninstallTerminalLabel IDNO SkipTerminalLabel
UninstallTerminalLabel:
        ${Terminal_CheckLock} $(TerminalIsRunningInstall) UninstallTerminalLabel SkipTerminalLabel
        ${UninstallApp} $Terminal_InstDir
    ${EndIf}

    ${Terminal_Unpack}
    ${Terminal_RegWrite}
    ${Terminal_CreateShortcuts}
    WriteUninstaller "$Terminal_InstDir\uninstall.exe"
    Goto TerminalInstallEnd
SkipTerminalLabel:
    DetailPrint "Skipped BotTerminal installation"
TerminalInstallEnd:

    Pop $3

SectionEnd

Section "Desktop Shortcut" TerminalDesktop

SectionEnd

Section "StartMenu Shortcut" TerminalStartMenu

SectionEnd

Section "Test Collection" TerminalTestCollection

    DetailPrint "Installing TestCollection"
    ${TestCollection_Unpack}

SectionEnd

SectionGroupEnd


SectionGroup "Install BotAgent" AgentGroup

Section "Core files" AgentCore

    Push $3

    DetailPrint "Installing BotAgent"

    ReadRegStr $3 HKLM "$Agent_RegKey" "${REG_PATH_KEY}"
    ${If} $Agent_InstDir == $3
        MessageBox MB_YESNO|MB_ICONQUESTION "$(UninstallPrevAgent)" IDYES UninstallAgentLabel IDNO SkipAgentLabel
UninstallAgentLabel:
        ${StopService} $Agent_ServiceId 80
        ${UninstallApp} $Agent_InstDir
    ${EndIf}

    ${Agent_Unpack}
    ${Agent_RegWrite}
    ${Configurator_CreateShortcuts}
    WriteUninstaller "$Agent_InstDir\uninstall.exe"

    DetailPrint "Creating BotAgent service"
    ${InstallService} $Agent_ServiceId "${SERVICE_DISPLAY_NAME}" "16" "2" "$Agent_InstDir\${AGENT_EXE}" 80
    ${ConfigureService} $Agent_ServiceId    

    DetailPrint "Starting BotAgent service"
    ${StartService} $Agent_ServiceId 30
    Goto AgentInstallEnd
SkipAgentLabel:
    DetailPrint "Skipped BotAgent installation"
AgentInstallEnd:

    Pop $3

SectionEnd

SectionGroup "Install Configurator" ConfiguratorGroup

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

    StrCpy $Terminal_InstDir $INSTDIR
    ${Terminal_InitId} "Uninstall"
    ${If} $Terminal_Id != ${EMPTY_APPID}
        
    RetryUninstallTerminal:
        ${Terminal_CheckLock} $(TerminalIsRunningUninstall) RetryUninstallTerminal SkipUninstallTerminal

        ${Terminal_DeleteShortcuts}
        ${Terminal_DeleteFiles}
        
        ; Delete self
        Delete "$INSTDIR\uninstall.exe"
        
        ; Remove registry entries
        ${Terminal_RegDelete}
        Goto TerminalUninstallEnd
    SkipUninstallTerminal:
        Abort $(UninstallCanceledMessage)
    TerminalUninstallEnd:

    ${EndIf}

    StrCpy $Agent_InstDir $INSTDIR
    ${Agent_InitId} "Uninstall"
    ${If} $Agent_Id != ${EMPTY_APPID}
        
        ${StopService} $Agent_ServiceId 80
        ${UninstallService} $Agent_ServiceId 80

        ${Configurator_DeleteShortcuts}
        ${Agent_DeleteFiles}

        ; Delete self
        Delete "$INSTDIR\uninstall.exe"

        ; Remove registry entries
        ${Agent_RegDelete}

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

    !insertmacro MUI_DESCRIPTION_TEXT ${AgentGroup} $(AgentSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${AgentCore} $(AgentSection1Description)

    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorGroup} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorCore} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorDesktop} $(ConfiguratorSection2Description)

!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------
; Components configuration

Function ConfigureComponents

    ${BeginSectionManagement}

        ${ReadOnlySection} ${TerminalCore}
        ${ReadOnlySection} ${AgentCore}
        ${ReadOnlySection} ${ConfiguratorCore}
        ${ReadOnlySection} ${ConfiguratorGroup} ; configurator is always installed with agent

        ${ExpandSection} ${TerminalGroup}
        ${ExpandSection} ${AgentGroup}
        ${ExpandSection} ${ConfiguratorGroup}

    ${EndSectionManagement}

    SectionGetSize ${TerminalCore} $Terminal_Size
    SectionGetSize ${AgentCore} $Agent_Size

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
    IntOp $0 $0 | ${AgentInstallBitFlag}
    ; 011011b
    SectionSetInstTypes ${AgentCore} $0
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

!macro DisableAgentSections
    ; ${ReadOnlySection} ${ConfiguratorGroup}
    !insertmacro DisableConfiguratorSections
!macroend

!macro EnableAgentSections
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

        ${if} $0 == ${AgentGroup}
            ; MessageBox MB_OK "Agent Group"
            ${If} ${SectionIsSelected} ${AgentCore}
                ${UnselectSection} ${AgentCore}
                ${UnselectSection} ${ConfiguratorCore}
                !insertmacro DisableAgentSections
            ${Else}
                ${SelectSection} ${ConfiguratorCore}
                ${SelectSection} ${AgentCore}
                ${SelectSection} ${ConfiguratorCore}
                !insertmacro EnableAgentSections
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
            ${If} ${SectionIsSelected} ${AgentCore}
                !insertmacro EnableAgentSections
            ${Else}
                !insertmacro DisableAgentSections
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

    ${If} ${SectionIsSelected} ${AgentCore}
        StrCpy $Agent_CoreSelected ${TRUE}
    ${Else}
        StrCpy $Agent_CoreSelected ${FALSE}
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