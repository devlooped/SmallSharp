pushd $PSScriptRoot/..
dotnet build -p:PackOnBuild=true
$version = gci bin | select -first 1 -expandproperty BaseName | %{ $_.Substring(11) }
pushd src/Demo
jq --arg version "$version" '.["msbuild-sdks"].SmallSharp = $version' global.json > temp.json && del global.json && mv temp.json global.json

# build with each top-level file as the active one
foreach ($file in gci *.cs) {
    # rm -r -fo obj -ea 0
    dotnet build Demo.csproj -p:ActiveFile=$($file.Name) -bl:"$($file.BaseName).binlog"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed for $($file.Name)"
        popd; popd;
        exit $LASTEXITCODE
    }
}

popd; popd;
