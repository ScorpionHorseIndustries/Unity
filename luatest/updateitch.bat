@echo off
cd\


date /t
echo %time%

For /f "tokens=1-3 delims=/-" %%a in ('date /t') do (set mydate=%%a.%%b.%%c)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mytime=%%a%%b)
echo %mydate%
butler push S:\unity\luatest\build samuel-rae/no-you-do-it:windows --userversion %mydate%

pause