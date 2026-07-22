@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"
if not defined OPENBFME_GODOT (
  echo RETAIL_SLICE FAIL Godot 4.7 was not configured. Set OPENBFME_GODOT.
  exit /b 1
)
if not exist "%OPENBFME_GODOT%" (
  echo RETAIL_SLICE FAIL Godot was not found at "%OPENBFME_GODOT%".
  exit /b 1
)
if not defined OPENBFME_CONTENT set "OPENBFME_CONTENT=%~dp0.private\content-packs"
if /I "%~1"=="--print-paths" (
  echo OPENBFME_CONTENT=%OPENBFME_CONTENT%
  exit /b 0
)
if /I "%~1"=="--test" (
  "%OPENBFME_GODOT%" --headless --path game --script res://tests/retail_slice_runner.gd
  exit /b !ERRORLEVEL!
)
start "OpenBFME Retail Vertical Slice" "%OPENBFME_GODOT%" --path game res://scenes/retail_vertical_slice.tscn
exit /b 0
