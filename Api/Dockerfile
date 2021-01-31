FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore Api/Api.csproj
RUN dotnet build Api/Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Api/Api.csproj -c Release -p:PublishReadyToRun=true -p:PublishTrimmed=true -r linux-x64 --self-contained true  -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Api"]