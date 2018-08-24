;--------------------------
; Includes

!addplugindir "Plugins"

!include "LogicLib.nsh"
!include "BA.Utils.nsh"
!include "MUI.nsh"
!include "Resources\Resources.en.nsi"

;--------------------------
; Parameters

!ifndef PRODUCT_NAME
  !define PRODUCT_NAME "TickTrader Bot Agent"
!endif

!ifndef PRODUCT_BUILD
  !define PRODUCT_BUILD "1.0"
!endif

!ifndef SERVICE_NAME
  !define SERVICE_NAME "_sfxBotAgent"
!endif

!ifndef SERVICE_DISPLAY_NAME
  !define SERVICE_DISPLAY_NAME "_sfxBotAgent"
!endif

!ifndef INSTALL_DIRECTORY
  !define INSTALL_DIRECTORY "TickTrader\BotAgent"
!endif

!ifndef SM_DIRECTORY
  !define SM_DIRECTORY "TickTrader\BotAgent"
!endif

!ifndef SETUP_FILENAME
  !define SETUP_FILENAME "BotAgent ${PRODUCT_BUILD}.Setup.x64.exe"
!endif

!ifndef LICENSE_FILE
  !define LICENSE_FILE "..\TickTrader.BotAgent\bin\Release\net462\publish\license.txt"
!endif

!ifndef APPDIR
  !define APPDIR "..\TickTrader.BotAgent\bin\Release\net462\publish"
!endif

!ifndef APPEXE
  !define APPEXE "TickTrader.BotAgent.exe"
!endif

!ifndef APPSETTINGS
  !define APPSETTINGS "WebAdmin\appsettings.json"
!endif

!ifndef PRODUCT_PUBLISHER
  !define PRODUCT_PUBLISHER "SoftFX"
!endif

;--------------------------
; Basic definitions

!define PRODUCT_DIR_REGKEY "Software\${PRODUCT_NAME}"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
;!define BA_DEFAULT_ADDRESS "http://localhost:5000"

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

FunctionEnd

;--------------------------------------------
;------------Generate Sertificate------------
!macro _CreatePfxContainer Password

	ExecWait `"$INSTDIR\Utilities\openssl.exe" req -config "$INSTDIR\Utilities\openssl.cnf" -x509 -sha512 -subj "/CN=localhost" -newkey rsa:4096 -keyout "$INSTDIR\key.pem" -out "$INSTDIR\cert.cer" -days 14600 -nodes`
	ExecWait `"$INSTDIR\Utilities\openssl.exe" pkcs12 -export -out "$INSTDIR\certificate.pfx" -inkey "$INSTDIR\key.pem" -in "$INSTDIR\cert.cer" -passout pass:${Password}`
	
	Delete "$INSTDIR\cert.cer"
	Delete "$INSTDIR\key.pem"
	Delete "$INSTDIR\.rnd"
!macroend

!define CreatePfx '!insertmacro "_CreatePfxContainer"'

!macro UninstallBAMacro un
	Function ${un}UninstallBA
		; Stop and Remove BA Service
		${UninstallService} "${SERVICE_NAME}" 80
		
		; Clear InstallDir
		!include BA.Setup.Uninstall.nsi
	
		; Delete Shortcuts
		Delete "$SMPROGRAMS\${SM_DIRECTORY}\Uninstall.lnk"
		RMDir "$SMPROGRAMS\${SM_DIRECTORY}"
	
		; Delete self
		Delete "$INSTDIR\uninstall.exe"
	
		; Remove from registry...
		DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"
		DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
	FunctionEnd
!macroend

!insertmacro UninstallBAMacro ""
!insertmacro UninstallBAMacro "un."


Section "TickTrader Bot Agent" Section1

	ReadRegStr $R0 HKLM "${PRODUCT_UNINST_KEY}" "UninstallString"
	${If} $R0 != "" 
	MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "$(UninstallPrev) $\n$\nClick OK to remove the previous version or Cancel to cancel this upgrade." IDOK uninst IDCANCEL uninstcancel
 	uninstcancel:
		Quit
	uninst:
		ClearErrors
		Call UninstallBA
	${EndIf}

	; Set Section properties
	SetOverwrite on
	; Set Section Files and Shortcuts
	
	SetOutPath "$INSTDIR\Utilities"
	File /r "Utilities\*.*"
	
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
	
	;Generate Certificate If Needed
	;Pwgen::GeneratePassword 20
	;Pop $0
	
	;${IfNot} ${FileExists} "$INSTDIR\${APPSETTINGS}"
	;	nsJSON::Set /value `{}`
	;	nsJSON::Set `Ssl` `File` /value `"certificate.pfx"`
	;	nsJSON::Set `Ssl` `Password` /value `"$0"`
	;	nsJSON::Serialize /format /file $INSTDIR\${APPSETTINGS}
	;	${CreatePfx} $0
	${If} ${FileExists} "$INSTDIR\${APPSETTINGS}"
		nsJSON::Set /file "$INSTDIR\${APPSETTINGS}"
		ClearErrors
		nsJSON::Get `Ssl` /end
		${If} ${Errors}
			nsJSON::Set /value `{}`
			nsJSON::Set `Ssl` `File` /value `"certificate.pfx"`
			nsJSON::Set `Ssl` `Password` /value `""`
			nsJSON::Serialize /format /file $INSTDIR\${APPSETTINGS}
		
			${CreatePfx} ""
		${EndIf}
	${Else}
		${CreatePfx} ""
	${EndIf}

	
	; Install Service
	${InstallService} "${SERVICE_NAME}" "${SERVICE_DISPLAY_NAME}" "16" "2" "$INSTDIR\${APPEXE}" 80
	${StartService} "${SERVICE_NAME}" 30
	
	Sleep 5000
	
	nsJSON::Set /file "$INSTDIR\${APPSETTINGS}"
	ClearErrors
	nsJSON::Get `server.urls` /end
	${IfNot} ${Errors}
		Pop $R0
		DetailPrint `server.urls = $R0`
		${OpenURL} "$R0"
    ${EndIf}
	
SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${Section1} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstall section
Section Uninstall
	Call un.UninstallBA
SectionEnd

BrandingText "${PRODUCT_PUBLISHER}"
; eof
