FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libgomp1 \
    libstdc++6 \
    libc6 \
    ca-certificates \
    curl && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish RAG_Code_Base.csproj -c Release -o /app/publish --no-restore

RUN find /root/.nuget/packages/treesitter.dotnet -path "*/linux-x64/native/*.so" \
    -exec cp {} /app/publish/ \; 2>/dev/null || echo "tree-sitter .so not found"

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RAG_Code_Base.dll"]