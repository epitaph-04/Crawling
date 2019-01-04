#!/bin/bash

dotnet pack --no-build --output nupkgs
dotnet publish SportApi/SportApi.csproj --framework netcoreapp2.2 -r linux-x64 --self-contained -o publishFolder
