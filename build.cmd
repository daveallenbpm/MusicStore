@echo off
cls

REM TODO - nuget / paket restore from here.

"packages\FAKE\tools\Fake.exe" build.fsx
pause
