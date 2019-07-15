;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "MUI.nsh"

!include "Algo.Utils.nsh"
!include "Resources\Resources.en.nsi"
!include "Algo.Setup.nsh"

;--------------------------
; Main Install settings

Name "${PRODUCT_NAME}"
Icon "${ICONS_DIR}\softfx.ico"
BrandingText "${PRODUCT_PUBLISHER}"
OutFile "${OUTPUT_DIR}\${SETUP_FILENAME}"
InstallDir ${BASE_INSTDIR}

;--------------------------
; Modern interface settings

!define MUI_ABORTWARNING
!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_ICON "${ICONS_DIR}\softfx.ico"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${LICENSE_FILE}"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
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

    InstTypeSetText ${StandardInstall} $(StandardInstallText)
    InstTypeSetText ${MinimalInstall} $(MinimalInstallText)
    InstTypeSetText ${TerminalInstall} $(TerminalInstallText)
    InstTypeSetText ${AgentInstall} $(AgentInstallText)
    InstTypeSetText ${FullInstall} $(FullInstallText)

    Call ConfigureComponents
    Call ConfigureInstallTypes

FunctionEnd

;--------------------------
; Components

SectionGroup "Install BotTerminal" TerminalGroup

Section "Core files" TerminalCore

    Push $3

    DetailPrint "Installing BotTerminal"

    SetOutPath "$INSTDIR\${TERMINAL_NAME}"
    ReadRegStr $3 HKLM "${REG_TERMINAL_KEY}" "Path"
    ${If} $OUTDIR == $3
        MessageBox MB_YESNO "$(UninstallPrevTerminal)" IDYES UninstallTerminalLabel IDNO SkipTerminalLabel
UninstallTerminalLabel:
        !insertmacro UninstallApp $OUTDIR
    ${EndIf}

    !insertmacro UnpackTerminal
    !insertmacro RegWriteTerminal
    WriteUninstaller "$INSTDIR\${TERMINAL_NAME}\uninstall.exe"

SkipTerminalLabel:

    Pop $3

SectionEnd

Section "Desktop Shortcut" TerminalDesktop

    DetailPrint "Adding Desktop Shortcut"
    Sleep 500

SectionEnd

Section "StartMenu Shortcut" TerminalStartMenu

    DetailPrint "Adding StartMenu Shortcut"
    Sleep 500

SectionEnd

Section "Test Collection" TerminalTestCollection

    DetailPrint "Installing Test Collection"
    
    SetOutPath "$INSTDIR\${TERMINAL_NAME}\${REPOSITORY_DIR}"
    !insertmacro UnpackTestCollection

    Sleep 1500

SectionEnd

SectionGroupEnd


SectionGroup "Install BotAgent" AgentGroup

Section "Core files" AgentCore

    Push $3

    DetailPrint "Installing BotAgent"

    SetOutPath "$INSTDIR\${AGENT_NAME}"
    ReadRegStr $3 HKLM "${REG_AGENT_KEY}" "Path"
    ${If} $OUTDIR == $3
        MessageBox MB_YESNO "$(UninstallPrevAgent)" IDYES UninstallAgentLabel IDNO SkipAgentLabel
UninstallAgentLabel:
        !insertmacro UninstallApp $OUTDIR
    ${EndIf}

    !insertmacro UnpackAgent
    !insertmacro RegWriteAgent
    WriteUninstaller "$INSTDIR\${AGENT_NAME}\uninstall.exe"

    DetailPrint "Creating BotAgent service"
    Sleep 2000

SkipAgentLabel:

    Pop $3

SectionEnd

SectionGroup "Install Configurator" ConfiguratorGroup

Section "Core files" ConfiguratorCore

    DetailPrint "Installing Configurator"
    
    SetOutPath "$INSTDIR\${AGENT_NAME}\${CONFIGURATOR_NAME}"
    !insertmacro UnpackConfigurator

SectionEnd

Section "Desktop Shortcut" ConfiguratorDesktop

    DetailPrint "Adding Desktop Shortcut"
    Sleep 500

SectionEnd

Section "StartMenu Shortcut" ConfiguratorStartMenu

    DetailPrint "Adding StartMenu Shortcut"
    Sleep 5000

SectionEnd

SectionGroupEnd

SectionGroupEnd


Section - FinishSection

SectionEnd

Section Uninstall

    Push $3

    ReadRegStr $3 HKLM "${REG_TERMINAL_KEY}" "Path"
    ${If} $3 == $INSTDIR
        
        ; Remove installed files, but leave generated
        !include Terminal.Uninstall.nsi
        Delete "terminal.ico"
        
        ; Delete self
        Delete "$INSTDIR\uninstall.exe"
        
        ; Remove registry entries
        !insertmacro RegDeleteTerminal

    ${EndIf}

    ReadRegStr $3 HKLM "${REG_AGENT_KEY}" "Path"
    ${If} $3 == $INSTDIR
        
        ; Remove installed files, but leave generated
        !include Agent.Uninstall.nsi
        Delete "agent.ico"

        ; Delete self
        Delete "$INSTDIR\uninstall.exe"

        ; Remove registry entries
        !insertmacro RegDeleteAgent

    ${EndIf}

    Pop $3

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

        ${ExpandSection} ${TerminalGroup}
        ${ExpandSection} ${AgentGroup}
        ${ExpandSection} ${ConfiguratorGroup}

    ${EndSectionManagement}

    ; SectionSetSize ${TerminalCore} ${TERMINAL_SIZE}
    ; SectionSetSize ${AgentCore} ${AGENT_SIZE}
    ; SectionSetSize ${ConfiguratorCore} ${CONFIGURATOR_SIZE}

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
    ${ReadOnlySection} ${ConfiguratorGroup}
    !insertmacro DisableConfiguratorSections
!macroend

!macro EnableAgentSections
    ${EnableSection} ${ConfiguratorGroup}
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
