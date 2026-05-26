# Dockerfile — .NET 8 Backend, ASP.NET Core Web API
ARG DOTNET_VERSION=8.0
ARG REGISTRY=""

FROM ${REGISTRY}mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS builder

WORKDIR /src

COPY src/*.csproj ./src/
RUN dotnet restore ./src/HackBackend.csproj

COPY . .
RUN dotnet publish ./src/HackBackend.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false


FROM ${REGISTRY}mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && if ! id -u app >/dev/null 2>&1; then \
         useradd --create-home --uid 1000 --shell /usr/sbin/nologin app; \
       fi \
    && mkdir -p /app/data/uploads \
    && chown -R app:app /app/data

COPY --from=builder --chown=app:app /app/publish .

USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HackBackend.dll"]