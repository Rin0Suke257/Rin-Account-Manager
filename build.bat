@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
echo CSC: %CSC%
if not exist dist mkdir dist
del /Q dist\RinAccountManager.exe 2>nul
"%CSC%" /nologo /t:winexe /out:dist\RinAccountManager.exe ^
 /r:System.dll ^
 /r:System.Core.dll ^
 /r:System.Drawing.dll ^
 /r:System.Windows.Forms.dll ^
 /r:System.Security.dll ^
 /r:System.Management.dll ^
 /r:System.Web.dll ^
 /r:System.Web.Extensions.dll ^
 /r:lib\webview2\lib\net462\Microsoft.Web.WebView2.Core.dll ^
 /r:lib\webview2\lib\net462\Microsoft.Web.WebView2.WinForms.dll ^
 /resource:assets\Rin.png,RinAccountManager.Rin.png ^
 src\Program.cs src\MainForm.cs src\AddAccountForm.cs src\EditAccountForm.cs src\QuickLoginForm.cs src\BrowserLoginForm.cs src\WebAccForm.cs src\GamesForm.cs src\SettingsForm.cs src\RinUI.cs src\AccountStore.cs src\MultiRoblox.cs src\HandleCloser.cs src\RobloxApi.cs src\QuickLogin.cs src\GamesStore.cs src\SettingsStore.cs src\Webhook.cs src\Launcher.cs
if errorlevel 1 (
  echo.
  echo BUILD FAIL
  exit /b 1
)
echo.
if exist dist\RinAccountManager.exe (
  echo BUILD OK: dist\RinAccountManager.exe
  del /Q dist\Rin.png 2>nul
  copy /Y lib\webview2\lib\net462\Microsoft.Web.WebView2.Core.dll dist\ >nul
  copy /Y lib\webview2\lib\net462\Microsoft.Web.WebView2.WinForms.dll dist\ >nul
  copy /Y lib\webview2\runtimes\win-x86\native\WebView2Loader.dll dist\ >nul
) else (
  echo BUILD FAIL
  exit /b 1
)
