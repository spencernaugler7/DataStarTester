#set shell := ["/usr/bin/zsh", "-cu"]
set windows-shell := ["powershell.exe", "-c"]

[default]
[working-directory: './DataStarTester.AppHost']
serve: build-styles
    echo serving app
    dotnet watch run --project DataStarTester.AppHost.csproj

alias bs := build-styles
[working-directory: "./DataStarTester"]
build-styles:
    @echo building styles
    bun run css:build
