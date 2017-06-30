;--------------------------------------------
;-----Function to open link in new window----
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
;---END Function to open link in new window---


;--------------------------------------------
;-----Functions to manage window service-----
!macro _InstallService Name DisplayName ServiceType StartType BinPath TimeOut
  SimpleSC::ExistsService ${Name}
  Pop $0
  ${If} $0 == 0
    SimpleSC::StopService "${Name}" 1 ${TimeOut}
	Pop $0
    ${If} $0 != 0
      Abort "$(ServiceStopFailMessage) $0"
    ${EndIf}

    SimpleSC::RemoveService ${Name}
    Pop $0
    ${If} $0 != 0
      Push $0
      SimpleSC::GetErrorMessage
      Pop $1
      Abort "$(ServiceUninstallFailMessage) $0 $1"
    ${EndIf}
  ${EndIf}
  
  SimpleSC::InstallService "${Name}" "${DisplayName}" "${ServiceType}" "${StartType}" ${BinPath} "" "" ""
  Pop $0
  ${If} $0 != 0
    Push $0
    SimpleSC::GetErrorMessage
    Pop $1
    Abort "$(ServiceInstallFailMessage) $0 $1"
  ${EndIf}
!macroend

!macro _StartService Name TimeOut
  SimpleSC::ExistsService ${Name}
  Pop $0
  ${If} $0 == 0
    SimpleSC::StartService "${Name}" "" ${TimeOut}
	Pop $0
    ${If} $0 != 0
      Abort "$(ServiceStartFailMessage) $0"
    ${EndIf}
  ${EndIf}
!macroend

!macro _StopService Name TimeOut
  SimpleSC::ExistsService ${Name}
  Pop $0
  ${If} $0 == 0
	SimpleSC::StopService "${SERVICE_NAME}" 1 ${TimeOut}
	Pop $0
    ${If} $0 != 0
      Abort "$(ServiceStopFailMessage) $0"
    ${EndIf}
    Sleep ${Sleep}
  ${EndIf}
!macroend

!macro _UninstallService Name TimeOut
  SimpleSC::ExistsService ${Name}
  Pop $0
  ${If} $0 == 0
    SimpleSC::StopService "${SERVICE_NAME}" 1 ${TimeOut}
	Pop $0
    ${If} $0 != 0
      Abort "$(ServiceStopFailMessage) $0"
    ${EndIf}
   
    SimpleSC::RemoveService ${Name}
    Pop $0
    ${If} $0 != 0
      Push $0
      SimpleSC::GetErrorMessage
      Pop $1
      Abort "$(ServiceUninstallFailMessage) $0 $1"
    ${EndIf}
  ${EndIf}
!macroend

!define InstallService '!insertmacro "_InstallService"'
!define StartService '!insertmacro "_StartService"'
!define StopService '!insertmacro "_StopService"'
!define UninstallService '!insertmacro "_UninstallService"'
;---END Functions to manage window service---