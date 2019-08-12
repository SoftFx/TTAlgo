;--------------------------------------------
;-----Bool constants-----

!define FALSE 0
!define TRUE 1

;-----Bool constants-----

;--------------------------------------------
;-----Functions to manage window service-----

!define NO_ERR_MSG "no_error"

!macro _InstallService Name DisplayName ServiceType StartType BinPath TimeOut ErrMsg
    StrCpy ${ErrMsg} ${NO_ERR_MSG}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::RemoveService ${Name}
        Pop $0
        ${If} $0 != 0
            Push $0
            SimpleSC::GetErrorMessage
            Pop $1
            StrCpy ${ErrMsg} "$(ServiceUninstallFailMessage) $0 $1"
            Goto _InstallServiceEnd
        ${EndIf}
    ${EndIf}
    
    SimpleSC::InstallService "${Name}" "${DisplayName}" "${ServiceType}" "${StartType}" ${BinPath} "" "" ""
    Pop $0
    ${If} $0 != 0
        Push $0
        SimpleSC::GetErrorMessage
        Pop $1
        StrCpy ${ErrMsg} "$(ServiceInstallFailMessage) $0 $1"
    ${EndIf}

_InstallServiceEnd:

!macroend

!macro _ConfigureService Name ErrMsg
    StrCpy ${ErrMsg} ${NO_ERR_MSG}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::SetServiceFailure ${Name} 0 "" "" 1 60000 1 60000 0 60000
        Pop $0
        ${If} $0 != 0
            StrCpy ${ErrMsg} "$(ServiceConfigFailMessage) $0"
        ${EndIf}
    ${EndIf}
!macroend

!macro _StartService Name TimeOut ErrMsg
    StrCpy ${ErrMsg} ${NO_ERR_MSG}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::StartService "${Name}" "" ${TimeOut}
    Pop $0
        ${If} $0 != 0
            StrCpy ${ErrMsg} "$(ServiceStartFailMessage) $0"
        ${EndIf}
    ${EndIf}
!macroend

!macro _StopService Name TimeOut ErrMsg
    StrCpy ${ErrMsg} ${NO_ERR_MSG}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::ServiceIsStopped ${Name}
        Pop $0
        Pop $1
        ${If} $1 == 0
            SimpleSC::StopService "${Name}" 1 ${TimeOut}
            Pop $0
            ${If} $0 != 0
                StrCpy ${ErrMsg} "$(ServiceStopFailMessage) $0"
            ${EndIf}
        ${EndIf}
    ${EndIf}
!macroend

!macro _UninstallService Name TimeOut ErrMsg
    StrCpy ${ErrMsg} ${NO_ERR_MSG}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::ServiceIsStopped ${Name}
        Pop $0
        Pop $1
        ${If} $1 == 0
            SimpleSC::StopService "${Name}" 1 ${TimeOut}
            Pop $0
            ${If} $0 != 0
                StrCpy ${ErrMsg} "$(ServiceStopFailMessage) $0"
                Goto _UninstallServiceEnd
            ${EndIf}
        ${EndIf}
     
        SimpleSC::RemoveService ${Name}
        Pop $0
        ${If} $0 != 0
            Push $0
            SimpleSC::GetErrorMessage
            Pop $1
            StrCpy ${ErrMsg} "$(ServiceUninstallFailMessage) $0 $1"
        ${EndIf}
    ${EndIf}

_UninstallServiceEnd:

!macroend

!macro _ServiceIsRunning Name RetVar

    StrCpy ${RetVar} ${FALSE}
    SimpleSC::ExistsService ${Name}
    Pop $0
    ${If} $0 == 0
        SimpleSC::ServiceIsRunning ${Name}
        Pop $0
        Pop $1
        ${If} $1 == 1
            StrCpy ${RetVar} ${TRUE}
        ${EndIf}
    ${EndIf}


!macroend


!define InstallService '!insertmacro _InstallService'
!define StartService '!insertmacro _StartService'
!define StopService '!insertmacro _StopService'
!define UninstallService '!insertmacro _UninstallService'
!define ConfigureService '!insertmacro _ConfigureService'
!define IsServiceRunning '!insertmacro _ServiceIsRunning'

;---END Functions to manage window service---

;--------------------------------------------
;-----Functions to manage sections-----

!define SECTION_ENABLE 0xFFFFFFEF # remove read-only flag
!define GROUP_REMOVE 0xFFFFFFFD # remove group flag
 
!macro SecSelect SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 | ${SF_SELECTED}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecUnselect SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 & ${SECTION_OFF}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecRO SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 | ${SF_RO}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecDisable SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 & ${SECTION_OFF}
    IntOp $7 $7 | ${SF_RO}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecRemoveRO SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 & ${SECTION_ENABLE}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecExpand SecId
    Push $7
    SectionGetFlags ${SecId} $7
    IntOp $7 $7 | ${SF_EXPAND}
    SectionSetFlags ${SecId} $7
    Pop $7
!macroend

!macro SecManageBegin
    Push $7
!macroend

!macro SecManageEnd
    Pop $7
!macroend

!define SelectSection '!insertmacro SecSelect'
!define UnselectSection '!insertmacro SecUnselect'
!define ReadOnlySection '!insertmacro SecRO'
!define DisableSection '!insertmacro SecDisable'
!define EnableSection '!insertmacro SecRemoveRO'
!define ExpandSection '!insertmacro SecExpand'
!define BeginSectionManagement '!insertmacro SecManageBegin'
!define EndSectionManagement '!insertmacro SecManageEnd'

;---END Functions to manage sections---

;--------------------------------------------
;-----Functions to manage app id-----

!define EMPTY_APPID 0

!macro CreateGUID RetVar
    System::Call 'ole32::CoCreateGuid(g .s)'
    !if ${RetVar} != no_var
        Pop ${RetVar}
    !endif
!macroend

!macro _FindAppIdByPath LabelId RetVar AppRootKey PathSubKey InstallPath

    Push $0
    Push $1
    Push $2
    
    StrCpy ${RetVar} ${EMPTY_APPID}
    StrCpy $0 0
    loop_${LabelId}:
        EnumRegKey $1 HKLM "${AppRootKey}" $0
        StrCmp $1 "" done_${LabelId}
        IntOp $0 $0 + 1

        ReadRegStr $2 HKLM "${AppRootKey}\$1" "${PathSubKey}"
        StrCmp $2 ${InstallPath} found_${LabelId} loop_${LabelId}
    found_${LabelId}:
        StrCpy ${RetVar} $1
    done_${LabelId}:

    Pop $2
    Pop $1
    Pop $0

!macroend

!define CreateAppId '!insertmacro CreateGUID'
!define FindAppIdByPath '!insertmacro _FindAppIdByPath'

;---END Functions to manage app id---

;--------------------------------------------
;-----Utility functions-----

!define FILE_NOT_LOCKED 0
!define FILE_LOCKED 1

!macro _GetFileLock RetVar FilePath

    Push $9

    ClearErrors
    FileOpen $9 ${FilePath} w
    ${If} ${Errors}
        StrCpy ${RetVar} ${FILE_LOCKED}
    ${Else}
        FileClose $9
        StrCpy ${RetVar} ${FILE_NOT_LOCKED}
    ${EndIf}

    Pop $9

!macroend

!macro _UninstallApp Path
    
    ; Copy previous uninstaller to temp location
    CreateDirectory "${Path}\tmp"
    CopyFiles /SILENT /FILESONLY "${Path}\uninstall.exe" "${Path}\tmp"
    ; Run uninstaller of previous version
    ExecWait '"${Path}\tmp\uninstall.exe" /S _?=${Path}'
    RMDir /r "${Path}\tmp"

!macroend

!define GetFileLock '!insertmacro _GetFileLock'
!define UninstallApp '!insertmacro _UninstallApp'

;---END Utility functions---

;--------------------------------------------
;-----Logging functions-----

var LogFile

!macro _SetLogFile Path

    ${If} ${FileExists} ${Path}
        Delete ${Path}
    ${EndIf}
    StrCpy $LogFile ${Path}

!macroend

!macro _Print Msg

    nsislog::log $LogFile "${Msg}"
    DetailPrint "${Msg}"

!macroend

!macro _Log Msg

    nsislog::log $LogFile "${Msg}"

!macroend

!define SetLogFile '!insertmacro _SetLogFile'
!define Print '!insertmacro _Print'
!define Log '!insertmacro _Log'

;---END Logging functions---