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
)
CALL :ConnectShare H: \\lgs-net.com\AKEL\AkelData %1
CALL :ConnectShare X: \\AKELNASFIS01\Share        %1
ENDLOCAL
EXIT /b

:RemoveLGSShares
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Removing LGS Shares
)
CALL :DisconnectShare H:
CALL :DisconnectShare X:
ENDLOCAL
EXIT /b


:AddLocalNAS
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Adding Local NAS
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
)
CALL :ConnectShare P: \\%1\c$ %2 %3
CALL :ConnectShare Q: \\%1\d$ %2 %3
CALL :ConnectShare R: \\%1\e$ %2 %3
ENDLOCAL
EXIT /b

:RemoveLocalPC
SETLOCAL
IF "%SpecificDrive%" == "" (
  ECHO *** Adding Local NPC
)
CALL :DisconnectShare P:
CALL :DisconnectShare Q:
CALL :DisconnectShare R:
ENDLOCAL
EXIT /b



:Start

SET SpecificDrive=%~2
IF NOT "%SpecificDrive%" == "" (
  SET LastChar=%SpecificDrive:~-1%
  IF NOT "%LastChar%" == ":" (
    SET SpecificDrive=%SpecificDrive%:
  )
  ECHO Addressing drive %SpecificDrive% only.
)

:: Main logic
IF /i "%~1"=="up"   GOTO :connect
IF /i "%~1"=="down" GOTO :disconnect
ECHO Invalid argument: %1 %2
ECHO Usage: NetworkShares.cmd up   <drive letter> # to connect
ECHO.       NetworkShares.cmd down <drive letter> # to disconnect
GOTO :exit


:connect
CALL :AddLGSShares LGS-Net\VHE
: CALL :AddLocalNAS  Mac-Pro.local WORKGROUP\hvanbrug
CALL :AddLocalNAS  192.168.1.30 WORKGROUP\hvanbrug
CALL :AddLocalPC   GEO-WMXL1404988 LGS-Net\VHE
GOTO :exit

:disconnect
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
