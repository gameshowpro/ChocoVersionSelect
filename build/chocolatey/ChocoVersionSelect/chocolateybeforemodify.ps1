﻿$ErrorActionPreference = 'Stop'
Get-Process -Name "$packageId$" -ErrorAction Ignore | Foreach-Object { $_.CloseMainWindow() | Out-Null }