@echo off
setlocal
rem ── Build the single-file, self-contained, compressed HenksHotkeys.exe ──
rem Double-click this file (or run it from a terminal) to produce one standalone
rem exe at bin\Publish\HenksHotkeys.exe — no .NET install or loose files needed.

cd /d "%~dp0"

echo Publishing HenksHotkeys (single-file, self-contained, compressed)...
echo.

rem Start clean so the output folder only ever holds the latest exe.
if exist "bin\Publish" rmdir /s /q "bin\Publish"

dotnet publish -p:PublishProfile=SingleFile
if errorlevel 1 (
  echo.
  echo *** Publish FAILED ***
  echo.
  pause
  exit /b 1
)

echo.
echo Done:  "%~dp0bin\Publish\HenksHotkeys.exe"
echo.

rem Open Explorer with the new exe selected.
explorer /select,"%~dp0bin\Publish\HenksHotkeys.exe"

pause
