#!bin/bash
set -e
dotnet restore
dotnet test test/doc-store.test/project.json -xml $(pwd)/testresults/out.xml
rm -rf $(pwd)/publish/web
dotnet publish src/doc-store/project.json -c release -o $(pwd)/publish/web