FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["DaprFrontEnd/DaprFrontEnd.csproj", "DaprFrontEnd/"]
RUN dotnet restore "DaprFrontEnd/DaprFrontEnd.csproj"
COPY . .
WORKDIR "/src/DaprFrontEnd"
RUN dotnet build "DaprFrontEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaprFrontEnd.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DaprFrontEnd.dll"]