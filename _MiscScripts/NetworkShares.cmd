@ECHO OFF

GOTO :Start


:ConnectShare
SETLOCAL
:: Parameters: %1 = Drive Letter, %2 = Share Path, %3 = Username, %4 = Password
SET "Drive=%~1"
SET "Share=%~2"
IF NOT "%SpecificDrive%" == "" (
  IF /i NOT "%SpecificDrive%" == "%Drive%" (
    ENDLOCAL
    EXIT /b
  )
)
:: Check if the drive is already mapped to the same share
FOR /f "tokens=2,3" %%A IN ('NET USE ^| FINDSTR /i "%Drive%"') DO (
  IF /i NOT "%%B" == "%Share%" (
    IF NOT "%%B" == "" (
      ECHO %Drive% is currently mapped to %Share%
    ) ELSE (
      IF NOT DEFINED %%B (
        ECHO %Drive% is currently mapped to an unidentified share
      ) ELSE (
        ECHO %Drive% is currently mapped to an unidentified share
      )
    )
    ENDLOCAL
    EXIT /b
  )
  ECHO %Drive% is already mapped to %Share%
  ENDLOCAL
  EXIT /b
)

:: Map the drive if not already mapped
ECHO Mapping %Drive% to %Share% ...
NET USE %1 %2 /USER:%3 %4 /PERSISTENT:YES
ENDLOCAL
EXIT /b

:DisconnectShare
SETLOCAL
SET "Drive=%~1"
IF NOT "%SpecificDrive%" == "" (
  IF /i NOT "%SpecificDrive%" == "%Drive%" (
    ENDLOCAL
    EXIT /b
  )
)
NET USE %1 /delete
ENDLOCAL
EXIT /b


:AddLGSShares
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Adding LGS Shares
) ELSE (
  ECHO *** Adding %SpecificDrive%
)
CALL :ConnectShare H: \\lgs-net.com\AKEL\AkelData %1
CALL :ConnectShare M: \\AKELDSKMSIBLD03\c$        %1
CALL :ConnectShare N: \\AKELDSKMSIBLD03\d$        %1
CALL :ConnectShare O: \\AKELDSKMSIBLD03\e$        %1
CALL :ConnectShare T: \\AKELDSKMSIBLD01\c$        %1
CALL :ConnectShare U: \\AKELDSKMSIBLD02\c$        %1
CALL :ConnectShare X: \\AKELNASFIS01\Share        %1
ENDLOCAL
EXIT /b

:RemoveLGSShares
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Removing LGS Shares
) ELSE (
  ECHO *** Removing %SpecificDrive%
)
CALL :DisconnectShare H:
CALL :DisconnectShare X:
ENDLOCAL
EXIT /b


:AddLocalNAS
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Adding Local NAS
) ELSE (
  ECHO *** Adding %SpecificDrive%
)
CALL :ConnectShare I: \\%1\NASData %2 %3
CALL :ConnectShare J: \\%1\Fred    %2 %3
CALL :ConnectShare K: \\%1\Pebbles %2 %3
ENDLOCAL
EXIT /b

:RemoveLocalNAS
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Removing Local NAS
) ELSE (
  ECHO *** Removing %SpecificDrive%
)
CALL :DisconnectShare I:
CALL :DisconnectShare J:
CALL :DisconnectShare K:
ENDLOCAL
EXIT /b


:AddLocalPC
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Adding Local PC
) ELSE (
  ECHO *** Adding %SpecificDrive%
)
CALL :ConnectShare P: \\%1\c$ %2 %3
CALL :ConnectShare Q: \\%1\d$ %2 %3
CALL :ConnectShare R: \\%1\e$ %2 %3
ENDLOCAL
EXIT /b

:RemoveLocalPC
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Removing Local PC
) ELSE (
  ECHO *** Removing %SpecificDrive%
)
CALL :DisconnectShare P:
CALL :DisconnectShare Q:
CALL :DisconnectShare R:
ENDLOCAL
EXIT /b



:Start

SET NAS=nas
SET PC=pc
SET LGS=lgs

SET SpecificGroup=
SET SpecificDrive=%~2

IF NOT "%SpecificDrive%" == "" (
  IF "%SpecificDrive%" == "%NAS%" (
    SET SpecificGroup=%SpecificDrive%
    SET SpecificDrive=
    GOTO :SkipSpecificDrive
  )
  IF "%SpecificDrive%" == "%PC%" (
    SET SpecificGroup=%SpecificDrive%
    SET SpecificDrive=
    GOTO :SkipSpecificDrive
  )
  IF "%SpecificDrive%" == "%LGS%" (
    SET SpecificGroup=%SpecificDrive%
    SET SpecificDrive=
    GOTO :SkipSpecificDrive
  )
  SET LastChar=%SpecificDrive:~-1%
  IF NOT "%LastChar%" == ":" (
    SET SpecificDrive=%SpecificDrive%:
  )
  ECHO Addressing drive %SpecificDrive% only.
)
:SkipSpecificDrive
ECHO SpecificGroup = %SpecificGroup%
ECHO SpecificDrive = %SpecificDrive%

:: Main logic
IF /i "%~1"=="up"   GOTO :connect
IF /i "%~1"=="down" GOTO :disconnect
ECHO Invalid argument: %1 %2
ECHO Usage: NetworkShares.cmd up   <drive letter> # to connect
ECHO.       NetworkShares.cmd down <drive letter> # to disconnect
GOTO :exit

:connect
IF NOT "%SpecificGroup%" == "" (
  ECHO "Connecting up '%SpecificGroup%'"
  IF "%SpecificGroup%" == "%NAS%" (
    ECHO "Dispatching up '%SpecificGroup%'"
    CALL :AddLocalNAS  192.168.1.30 WORKGROUP\hvanbrug
    GOTO :exit
  )
  IF "%SpecificGroup%" == "%PC%" (
    ECHO "Dispatching up '%SpecificGroup%'"
    CALL :AddLocalPC   GEO-WMXL1404988 LGS-Net\VHE
    GOTO :exit
  )
  IF "%SpecificGroup%" == "%LGS%" (
    ECHO "Dispatching up '%SpecificGroup%'"
    CALL :AddLGSShares LGS-Net\VHE
    GOTO :exit
  )
  GOTO :exit
)
ECHO "Connecting everything!"
CALL :AddLGSShares LGS-Net\VHE
CALL :AddLocalNAS  192.168.1.30 WORKGROUP\hvanbrug
CALL :AddLocalPC   GEO-WMXL1404988 LGS-Net\VHE
GOTO :exit

:disconnect
IF NOT "%SpecificGroup%" == "" (
  ECHO "Disconnecting '%SpecificGroup%'"
  IF "%SpecificGroup%" == "%NAS%" (
    ECHO "Dispatching down '%SpecificGroup%'"
    CALL :RemoveLocalNAS
    GOTO :exit
  )
  IF "%SpecificGroup%" == "%PC%" (
    ECHO "Dispatching down '%SpecificGroup%'"
    CALL :RemoveLocalPC
    GOTO :exit
  )
  IF "%SpecificGroup%" == "%LGS%" (
    ECHO "Dispatching down '%SpecificGroup%'"
    CALL :RemoveLGSShares
    GOTO :exit
  )
  GOTO :exit
)

ECHO "Disconnecting everything!"
CALL :RemoveLGSShares
CALL :RemoveLocalNAS
CALL :RemoveLocalPC
: GOTO :exit

:exit
ECHO.
ECHO.
ECHO Current mappings...
NET USE

:: Check if the batch was launched from Explorer (i.e., not an interactive shell)
ECHO %cmdcmdline% | FIND "/c" >NUL
IF %errorlevel%==0 (
    PAUSE
)
