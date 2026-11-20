@echo off
set ASE=C:\aseprite\build\bin\aseprite.exe
cd /d "%~dp0"
for %%f in (fish_*.png) do (
  echo %%f
  "%ASE%" -b "%%f" --save-as "%%~nf.aseprite"
)
echo Xong.
pause
