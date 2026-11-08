# TTPlugins
# Copyright (C) 2024  TTPlugins
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

Write-Host
Write-Host
Write-Host -ForegroundColor Cyan "Compiling Indicators++ Release for Tiger.com 7.4 (net10.0-windows)"
Write-Host
Write-Host

# Requires the .NET 10 SDK. Tiger.com 7.4 vendor DLLs are net10.0.
# SDK 9 cannot target net10.0. SkipPluginInstall leaves copying to the Indicators folder manual.
dotnet build "$PSScriptRoot\..\Indicators\Indicators.csproj" -c Release -p:SkipPluginInstall=true
