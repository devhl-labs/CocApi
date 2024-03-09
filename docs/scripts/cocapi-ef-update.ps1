dotnet ef database update `
    --project $PSScriptRoot/../../src/CocApi.Test `
    --context CocApi.Cache.CacheDbContext `
    --verbose `
    --configuration Debug `
    -- `
    --environment Development
