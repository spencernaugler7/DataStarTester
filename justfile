set shell := ["powershell.exe", "-c"]

[working-directory: 'DataStarTester.AppHost']
default:
    dotnet watch run --project DataStarTester.AppHost.csproj
