@echo off
echo.请输入表结构的版本号，如：1.0.0
set /p table_version=
dotnet ef migrations add %table_version%
dotnet ef database update