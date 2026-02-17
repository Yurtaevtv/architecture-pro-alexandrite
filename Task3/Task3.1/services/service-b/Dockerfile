FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

USER root
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["./ServiceB.csproj", "."]
RUN dotnet restore "ServiceB.csproj"
COPY . .
RUN dotnet build "ServiceB.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceB.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN ls
ENTRYPOINT ["dotnet", "ServiceB.dll"]