@ECHO OFF
del *.nupkg
.\nuget.exe pack .\HockeySDK.Core.nuspec
.\nuget.exe pack .\HockeySDK.WP.nuspec
pause