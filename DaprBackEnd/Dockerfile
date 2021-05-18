FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["DaprBackEnd/DaprBackEnd.csproj", "DaprBackEnd/"]
RUN dotnet restore "DaprBackEnd/DaprBackEnd.csproj"
COPY . .
WORKDIR "/src/DaprBackEnd"
RUN dotnet build "DaprBackEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaprBackEnd.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DaprBackEnd.dll"]