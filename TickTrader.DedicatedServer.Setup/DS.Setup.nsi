;--------------------------
; Includes

!addplugindir "Plugins"

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
 !define DS_ADDRES "http://localhost:8080"

 ;--------------------------
; Function to open link in new window
Function openLinkNewWindow
  Push $3
  Exch
  Push $2
  Exch
  Push $1
  Exch
  Push $0
  Exch
 
  ReadRegStr $0 HKCR "http\shell\open\command" ""
# Get browser path
    DetailPrint $0
  StrCpy $2 '"'
  StrCpy $1 $0 1
  StrCmp $1 $2 +2 # if path is not enclosed in " look for space as final char
    StrCpy $2 ' '
  StrCpy $3 1
  loop:
    StrCpy $1 $0 1 $3
    DetailPrint $1
    StrCmp $1 $2 found
    StrCmp $1 "" found
    IntOp $3 $3 + 1
    Goto loop
 
  found:
    StrCpy $1 $0 $3
    StrCmp $2 " " +2
      StrCpy $1 '$1"'
 
  Pop $0
  Exec '$1 $0'
  Pop $0
  Pop $1
  Pop $2
  Pop $3
FunctionEnd
 
!macro _OpenURL URL
Push "${URL}"
Call openLinkNewWindow
!macroend

!define OpenURL '!insertmacro "_OpenURL"'
 
;--------------------------
; Main Install settings
Name "${PRODUCT_NAME}"
InstallDir "$PROGRAMFILES\${INSTALL_DIRECTORY}"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
OutFile "..\build.ouput\${SETUP_FILENAME}"

;--------------------------
; Modern interface settings

!include "MUI.nsh"
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${LICENSE_FILE}"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Set languages(first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------

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
	SimpleSC::InstallService "${SERVICE_NAME}" "${SERVICE_DISPLAY_NAME}" "16" "2" "$INSTDIR\${APPEXE}" "" "" ""
	SimpleSC::StartService "${SERVICE_NAME}" "" 0
	
	${OpenURL} "${DS_ADDRES}"

SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${Section1} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstall section
Section Uninstall

	; Stop and Remove Service
	SimpleSC::StopService "${SERVICE_NAME}" 1 80
	SimpleSC::RemoveService "${SERVICE_NAME}"
	
	; Remove from registry...
	DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"
	DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
	; Delete self
	Delete "$INSTDIR\uninstall.exe"
	; Delete Shortcuts
	Delete "$SMPROGRAMS\${SM_DIRECTORY}\Uninstall.lnk"
	
	!include DS.Setup.Uninstall.nsi
	
	RMDir "$SMPROGRAMS\${SM_DIRECTORY}"

SectionEnd
BrandingText "${PRODUCT_PUBLISHER}"
; eof
