FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore LiveTvWorker/LiveTvWorker.csproj
RUN dotnet build LiveTvWorker/LiveTvWorker.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish LiveTvWorker/LiveTvWorker.csproj -c Release -p:PublishReadyToRun=true -p:PublishTrimmed=true -r linux-x64 --self-contained true  -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./LiveTvWorker"]