@echo off
setlocal
set "ROOT=%~dp0"
if not defined OPENBFME_GODOT (
  echo Godot 4.7 was not configured. Set OPENBFME_GODOT to the executable path.
  exit /b 1
)
if not exist "%OPENBFME_GODOT%" (
  echo Godot was not found at "%OPENBFME_GODOT%".
  exit /b 1
)
if not defined OPENBFME_CONTENT set "OPENBFME_CONTENT=%ROOT%.private\content-packs"
"%OPENBFME_GODOT%" --path "%ROOT%game"
exit /b %ERRORLEVEL%
