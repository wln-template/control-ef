@echo off
echo.�������ṹ�İ汾�ţ��磺1.0.0
set /p table_version=
dotnet ef migrations add %table_version%
dotnet ef database update