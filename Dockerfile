FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY ["src/Hinario.API/Hinario.API.csproj", "src/Hinario.API/"]
COPY ["src/Hinario.Application/Hinario.Application.csproj", "src/Hinario.Application/"]
COPY ["src/Hinario.Domain/Hinario.Domain.csproj", "src/Hinario.Domain/"]
COPY ["src/Hinario.Infra/Hinario.Infra.csproj", "src/Hinario.Infra/"]

RUN dotnet restore "src/Hinario.API/Hinario.API.csproj"
COPY . .
WORKDIR /build/src/Hinario.API

FROM build AS publish
RUN dotnet publish "Hinario.API.csproj" -c Release -o /publish

FROM base AS final
WORKDIR /app
COPY --from=publish /publish .

ENTRYPOINT ["dotnet", "Hinario.API.dll"]
