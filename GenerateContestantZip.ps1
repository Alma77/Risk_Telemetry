param(
	[Parameter(mandatory)][int]$version,
	[string]$outdir = "risk_v$version"
)
New-Item -Path "../$outdir" -ItemType "directory"
New-Item -Path "../$outdir\Risk.Shared" -ItemType "directory"
New-Item -Path "../$outdir\YourCode" -ItemType "directory"
New-Item -Path "../$outdir\Risk.Game" -ItemType "directory"
New-Item -Path "../$outdir\Risk.Server" -ItemType "directory"

write-host "this is version $version"

copy-item -path "Risk.Shared\*" -destination "..\$outdir\Risk.Shared" -recurse
copy-item -path "Risk.Signalr.ConsoleClient\*" -destination "..\$outdir\YourCode" -recurse
rename-item -path "../$outdir/YourCode/risk.signalr.consoleclient.csproj" -newname "YourCode.csproj"
copy-item -path "Risk.Game\*" -destination "..\$outdir\Risk.Game" -recurse
copy-item -path "Risk.Server\*" -destination "..\$outdir\Risk.Server" -recurse
copy-item -path "StartGame.ps1" -destination "..\$outdir"

cd ..

get-childitem "$outdir" -Include *obj*,*bin* -Recurse -Directory | remove-item -Recurse -Force

Compress-Archive -path "$outdir\*" -destination "$outdir"
