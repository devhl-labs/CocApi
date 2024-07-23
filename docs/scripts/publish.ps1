dotnet publish $PSScriptRoot/../../src/CocApi.Test/CocApi.Test.csproj `
    -c Release `
    -o $PSScriptRoot/../../src/CocApi.Test/bin/Release/net5.0/win-x64/publish `
    -r win-x64 `
    -p:PublishReadyToRun=true `
    -p:PublishSingleFile=true
