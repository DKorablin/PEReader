/*
@echo off && cls
IF EXIST "%~0.exe" (
"%~0.exe"
exit
)
call CommonLib.bat
set WinDirNet=%WinDir%\Microsoft.NET\Framework
IF EXIST "%WinDirNet%\v2.0.50727\csc.exe" set csc="%WinDirNet%\v2.0.50727\csc.exe"
IF EXIST "%WinDirNet%\v3.5\csc.exe" set csc="%WinDirNet%\v3.5\csc.exe"
IF EXIST "%WinDirNet%\v4.0.30319\csc.exe" set csc="%WinDirNet%\v4.0.30319\csc.exe"
%csc% /nologo /reference:CommonLib.dll  /out:"%~0.exe" %0
"%~0.exe"
PAUSE
exit
*/
using System;
using System.Reflection;
using CommonLib;

namespace EnumTestApp
{
    internal class Program
    {
   	 static void Main(string[] args)
   	 {
   		 EnumAttribute attr = (EnumAttribute)typeof(Program)
   			 .GetMethod("MethodWithEnumAttribute", BindingFlags.Static | BindingFlags.NonPublic)
   			 .GetCustomAttributes(typeof(EnumAttribute), false)[0];

   		 Console.WriteLine(attr.Value.ToString());
   	 }

   	 [Enum(Value = SharedEnum.Third)]
   	 static void MethodWithEnumAttribute() { }
    }

    internal class EnumAttribute : Attribute
    {
   	 public SharedEnum Value { get; set; }
    }
}