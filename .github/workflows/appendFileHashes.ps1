$directoryPath = $env:BINARIES_PATH
$verificationFilePath = ".\build\chocolatey\ChocoVersionSelect\VERIFICATION.txt"
$files = Get-ChildItem -Path $directoryPath -Recurse -File
Add-Content -Path $verificationFilePath -Value "`nchecksum type: sha256"
foreach ($file in $files) {
    $checksum = Get-FileHash -Path $file.FullName -Algorithm SHA256 | Select-Object -ExpandProperty Hash
    $relativePath = Resolve-Path -Path $file.FullName -RelativeBasePath $directoryPath -Relative
    Add-Content -Path $verificationFilePath -Value "`nfile: $relativePath`nchecksum: $checksum"
}