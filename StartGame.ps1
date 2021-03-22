write-host "Looking for an available port..." -foregroundcolor green
$serverport = 5000

while((test-netconnection localhost -port $serverport -WarningAction SilentlyContinue).TcpTestSucceeded){
	write-host "Port $serverport is in use, trying next port"
	$serverport++
}

write-host "Starting risk server on port $serverport" -foregroundcolor green
start-process dotnet -argumentlist "run", "--project", "./Risk.Server", "--urls", "http://localhost:$serverport"

write-host "Sleeping for 10 seconds while server spins up..." -foregroundcolor green
start-sleep -Seconds 10

write-host "Opening up a browser at http://localhost:$serverport" -foregroundcolor green
start-process "http://localhost:$serverport"

write-host "Starting up console-based competitor (you)`n`n`n" -foregroundcolor green
dotnet run --project ./YourCode "http://localhost:$serverport"

