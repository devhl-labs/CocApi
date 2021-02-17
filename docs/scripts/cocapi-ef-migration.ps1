$count=(Get-ChildItem -File -Path $PSScriptRoot/../../src/CocApi.Test/Migrations).Count

dotnet ef migrations add Migration$count `
    --project $PSScriptRoot/../../src/CocApi.Test `
    --context CocApi.Cache.CocApiCacheContext