ARG cfg=Release

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
# Install Node.js
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_16.x | bash \
    && apt-get install nodejs -yq

FROM build-env AS project
WORKDIR /project
# Copy everything
COPY . ./

FROM project AS build
ARG cfg
RUN dotnet publish /project/TickTrader.BotAgent/TickTrader.BotAgent.csproj -c $cfg -o out
RUN dotnet publish /project/src/csharp/apps/TickTrader.Algo.RuntimeV1Host/TickTrader.Algo.RuntimeV1Host.csproj -c $cfg -o out/bin/runtime

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as base
RUN apk add curl

FROM base AS final
WORKDIR /app
COPY --from=build /project/out ./bin
COPY --from=build /project/docker/appinfo.json .

EXPOSE 15443
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f -k https://localhost:15443/ || exit 1

ENTRYPOINT [ "dotnet", "bin/TickTrader.AlgoServer.dll" ]
CMD [ "-c", "1" ]
