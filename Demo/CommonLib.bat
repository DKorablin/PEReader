/*
@echo off && cls
set WinDirNet=%WinDir%\Microsoft.NET\Framework
IF EXIST "%WinDirNet%\v2.0.50727\csc.exe" set csc="%WinDirNet%\v2.0.50727\csc.exe"
IF EXIST "%WinDirNet%\v3.5\csc.exe" set csc="%WinDirNet%\v3.5\csc.exe"
IF EXIST "%WinDirNet%\v4.0.30319\csc.exe" set csc="%WinDirNet%\v4.0.30319\csc.exe"
%csc% /nologo /target:library  /out:"CommonLib.dll" %0
goto :eof
*/
using System;


namespace CommonLib
{
    public enum SharedEnum : int
    {
   	 Undefined = 0,
   	 First = 1,
   	 Second = 2,
   	 Third = 3,
    }
}