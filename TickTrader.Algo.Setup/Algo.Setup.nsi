;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "MUI.nsh"

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

!ifndef LICENSE_FILE
    !define LICENSE_FILE "license.txt"
!endif

!ifndef SETUP_FILENAME
    !define SETUP_FILENAME "Algo ${PRODUCT_BUILD}.Setup.exe"
!endif

!define BASE_NAME "TickTrader"
!define TERMINAL_NAME "BotTerminal"
!define AGENT_NAME "BotAgent"
!define CONFIGURATOR_NAME "Configurator"

!define REPOSITORY_DIR "AlgoRepository"
!define OUTPUT_DIR "..\build.ouput"
!define ICONS_DIR "..\icons"

!define TERMINAL_BINDIR "..\TickTrader.BotTerminal\bin\Release\"
!define AGENT_BINDIR "..\TickTrader.BotAgent\bin\Release\net462\publish"
!define CONFIGURATOR_BINDIR "..\TickTrader.BotAgent.Configurator\bin\Release\"

!define AGENT_EXE "TickTrader.BotAgent.exe"

!define SERVICE_NAME "_sfxBotAgent"
!define SERVICE_DISPLAY_NAME "_sfxBotAgent"

!define BASE_INSTDIR "$PROGRAMFILES64\${BASE_NAME}"
!define TERMINAL_INSTDIR "${BASE_INSTDIR}\${TERMINAL_NAME}"
!define AGENT_INSTDIR "${BASE_INSTDIR}\${AGENT_NAME}"

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
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Set languages(first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL


Function .onInit

    Call ConfigureComponents
    Call ConfigureInstallTypes

FunctionEnd

;--------------------------
; Components

SectionGroup "Install BotTerminal" TerminalGroup

Section "Core files" TerminalSection1

    DetailPrint "Installing BotTerminal"

    SetOutPath "$INSTDIR\${TERMINAL_NAME}"
    File /r "${TERMINAL_BINDIR}\*.*"

SectionEnd

Section "Desktop Shortcut" TerminalSection2

    DetailPrint "Adding Desktop Shortcut"
    Sleep 500

SectionEnd

Section "StartMenu Shortcut" TerminalSection3

    DetailPrint "Adding StartMenu Shortcut"
    Sleep 500

SectionEnd

Section "Test Collection" TerminalSection4

    DetailPrint "Installing Test Collection"
    
    SetOutPath "$INSTDIR\${TERMINAL_NAME}\${REPOSITORY_DIR}"
    File /r "${OUTPUT_DIR}\TickTrader.Algo.TestCollection.ttalgo"
    File /r "${OUTPUT_DIR}\TickTrader.Algo.VersionTest.ttalgo"

SectionEnd

SectionGroupEnd


SectionGroup "Install BotAgent" AgentGroup

Section "Core files" AgentSection1

    DetailPrint "Installing BotAgent"

    SetOutPath "$INSTDIR\${AGENT_NAME}"
    File /r "${AGENT_BINDIR}\*.*"

    DetailPrint "Creating BotAgent service"
    Sleep 2000

SectionEnd

SectionGroup "Install Configurator" ConfiguratorGroup

Section "Core files" ConfiguratorSection1

    DetailPrint "Installing Configurator"
    
    SetOutPath "$INSTDIR\${AGENT_NAME}\${CONFIGURATOR_NAME}"
    File /r "${CONFIGURATOR_BINDIR}\*.*"

SectionEnd

Section "Desktop Shortcut" ConfiguratorSection2

    DetailPrint "Adding Desktop Shortcut"
    Sleep 500

SectionEnd

Section "StartMenu Shortcut" ConfiguratorSection3

    DetailPrint "Adding StartMenu Shortcut"
    Sleep 500

SectionEnd

SectionGroupEnd

SectionGroupEnd


Section - FinishSection

SectionEnd

Function .onSelChange

    ${if} $0 == ${TerminalGroup}
        # MessageBox MB_OK "Terminal Group"
        ${If} ${SectionIsSelected} ${TerminalSection1}
            ${UnselectSection} ${TerminalSection1}
            ${DisableSection} ${TerminalSection2}
            ${DisableSection} ${TerminalSection3}
            ${DisableSection} ${TerminalSection4}
        ${Else}
            ${SelectSection} ${TerminalSection1}
            ${EnableSection} ${TerminalSection2}
            ${EnableSection} ${TerminalSection3}
            ${EnableSection} ${TerminalSection4}
        ${EndIf}
    ${EndIf}

    ${if} $0 == ${AgentGroup}
        # MessageBox MB_OK "Agent Group"
        ${If} ${SectionIsSelected} ${AgentSection1}
            ${UnselectSection} ${AgentSection1}
            ${UnselectSection} ${ConfiguratorSection1}
            ${ReadOnlySection} ${ConfiguratorGroup}
            ${DisableSection} ${ConfiguratorSection2}
            ${DisableSection} ${ConfiguratorSection3}
        ${Else}
            ${SelectSection} ${AgentSection1}
            ${SelectSection} ${ConfiguratorSection1}
            ${EnableSection} ${ConfiguratorGroup}
            ${EnableSection} ${ConfiguratorSection2}
            ${EnableSection} ${ConfiguratorSection3}
        ${EndIf}
    ${EndIf}

    ${if} $0 == ${ConfiguratorGroup}
        # MessageBox MB_OK "Configurator Group"
        ${If} ${SectionIsSelected} ${ConfiguratorSection1}
            ${UnselectSection} ${ConfiguratorSection1}
            ${DisableSection} ${ConfiguratorSection2}
            ${DisableSection} ${ConfiguratorSection3}
        ${Else}
            ${SelectSection} ${ConfiguratorSection1}
            ${EnableSection} ${ConfiguratorSection2}
            ${EnableSection} ${ConfiguratorSection3}
        ${EndIf}
    ${EndIf}

FunctionEnd


Function ConfigureComponents

    ${ReadOnlySection} ${TerminalSection1}
    ${ReadOnlySection} ${AgentSection1}
    ${ReadOnlySection} ${ConfiguratorSection1}

    ${ExpandSection} ${TerminalGroup}
    ${ExpandSection} ${AgentGroup}
    ${ExpandSection} ${ConfiguratorGroup}

FunctionEnd

;--------------------------
; Components description

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN

    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalGroup} $(TerminalSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalSection1} $(TerminalSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalSection2} $(TerminalSection2Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalSection3} $(TerminalSection3Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${TerminalSection4} $(TerminalSection4Description)

    !insertmacro MUI_DESCRIPTION_TEXT ${AgentGroup} $(AgentSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${AgentSection1} $(AgentSection1Description)

    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorGroup} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorSection1} $(ConfiguratorSection1Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorSection2} $(ConfiguratorSection2Description)
    !insertmacro MUI_DESCRIPTION_TEXT ${ConfiguratorSection3} $(ConfiguratorSection3Description)

!insertmacro MUI_FUNCTION_DESCRIPTION_END

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

Function ConfigureInstallTypes

    InstTypeSetText ${StandardInstall} $(StandardInstallText)
    InstTypeSetText ${MinimalInstall} $(MinimalInstallText)
    InstTypeSetText ${TerminalInstall} $(TerminalInstallText)
    InstTypeSetText ${AgentInstall} $(AgentInstallText)
    InstTypeSetText ${FullInstall} $(FullInstallText)

    Push $0

    StrCpy $0 ${FullInstallBitFlag}
    ; 010000b
    SectionSetInstTypes ${TerminalSection4} $0

    IntOp $0 $0 | ${StandardInstallBitFlag}
    IntOp $0 $0 | ${TerminalInstallBitFlag}
    ; 010101b
    SectionSetInstTypes ${TerminalSection2} $0
    SectionSetInstTypes ${TerminalSection3} $0

    IntOp $0 $0 | ${MinimalInstallBitFlag}
    ; 010111b
    SectionSetInstTypes ${TerminalSection1} $0

    IntOp $0 $0 ^ ${TerminalInstallBitFlag}
    IntOp $0 $0 | ${AgentInstallBitFlag}
    ; 011011b
    SectionSetInstTypes ${AgentSection1} $0
    SectionSetInstTypes ${ConfiguratorSection1} $0

    IntOp $0 $0 ^ ${MinimalInstallBitFlag}
    ; 011001b
    SectionSetInstTypes ${ConfiguratorSection2} $0
    SectionSetInstTypes ${ConfiguratorSection3} $0

    Pop $0

    SetCurInstType ${StandardInstall}

FunctionEnd
