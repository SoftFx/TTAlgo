;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "DS.Utils.nsh"
!include "MUI.nsh"
!include "Resources\Resources.en.nsi"

;--------------------------
; Parameters

!ifndef PRODUCT_NAME
  !define PRODUCT_NAME "TickTrader Dedicated Server"
!endif

!ifndef PRODUCT_BUILD
  !define PRODUCT_BUILD "1.0"
!endif

!ifndef SERVICE_NAME
  !define SERVICE_NAME "_sfxDedicatedServer"
!endif

!ifndef SERVICE_DISPLAY_NAME
  !define SERVICE_DISPLAY_NAME "_sfxDedicatedServer"
!endif

!ifndef INSTALL_DIRECTORY
  !define INSTALL_DIRECTORY "TickTrader\DedicatedServer"
!endif

!ifndef SM_DIRECTORY
  !define SM_DIRECTORY "TickTrader\DedicatedServer"
!endif

!ifndef SETUP_FILENAME
  !define SETUP_FILENAME "TickTrader.DedicatedServer.Setup ${PRODUCT_BUILD}.exe"
!endif

!ifndef LICENSE_FILE
  !define LICENSE_FILE "..\TickTrader.DedicatedServer\bin\Release\net462\publish\license.txt"
!endif

!ifndef APPDIR
  !define APPDIR "..\TickTrader.DedicatedServer\bin\Release\net462\publish"
!endif

!ifndef APPEXE
  !define APPEXE "TickTrader.DedicatedServer.exe"
!endif

!ifndef PRODUCT_PUBLISHER
  !define PRODUCT_PUBLISHER "SoftFX"
!endif

;--------------------------
; Basic definitions

!define PRODUCT_DIR_REGKEY "Software\${PRODUCT_NAME}"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define DS_ADDRESS "http://localhost:8080"

;--------------------------
; Main Install settings
Name "${PRODUCT_NAME}"
InstallDir "$PROGRAMFILES\${INSTALL_DIRECTORY}"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
OutFile "..\build.ouput\${SETUP_FILENAME}"

;--------------------------
; Modern interface settings

!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${LICENSE_FILE}"
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

;--------------------------
; Auto-uninstall old before installing new

Function .onInit
 
  ReadRegStr $R0 HKLM "${PRODUCT_UNINST_KEY}" "UninstallString"
  ${If} $R0 != "" 
	MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "$(UninstallPrev) $\n$\nClick OK to remove the previous version or Cancel to cancel this upgrade." IDOK uninst IDCANCEL uninstcancel
 	uninst:
		ClearErrors
		ExecWait '$R0'
	uninstcancel:
		Abort
  ${EndIf}

FunctionEnd

Section "TickTrader Dedicated Server" Section1
	; Set Section properties
	SetOverwrite on
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\"
	File /r "${APPDIR}\*.*"
	CreateDirectory "$SMPROGRAMS\${SM_DIRECTORY}"
	CreateShortCut "$SMPROGRAMS\${SM_DIRECTORY}\Uninstall.lnk" "$INSTDIR\uninstall.exe"
SectionEnd

Section - FinishSection
	WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR"
	WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayName" "${PRODUCT_NAME}"
	WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteUninstaller "$INSTDIR\uninstall.exe"
	
	; Install Service
	${InstallService} "${SERVICE_NAME}" "${SERVICE_DISPLAY_NAME}" "16" "2" "$INSTDIR\${APPEXE}" 80
	${StartService} "${SERVICE_NAME}" 30
	${OpenURL} "${DS_ADDRESS}"

SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${Section1} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstall section
Section Uninstall
	; Stop and Remove DS Service
	${UninstallService} "${SERVICE_NAME}" 80
	
	; Remove from registry...
	DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"
	DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
	
	; Delete self
	Delete "$INSTDIR\uninstall.exe"
	
	; Delete Shortcuts
	Delete "$SMPROGRAMS\${SM_DIRECTORY}\Uninstall.lnk"
	
	; Clear InstallDir
	!include DS.Setup.Uninstall.nsi
	
	RMDir "$SMPROGRAMS\${SM_DIRECTORY}"

SectionEnd

BrandingText "${PRODUCT_PUBLISHER}"
; eof
