#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AuthServer.Host/AuthServer.Host.csproj", "AuthServer.Host/"]
COPY ["AccountModule/Volo.Abp.Account.Web/Volo.Abp.Account.Web.csproj", "AccountModule/Volo.Abp.Account.Web/"]
COPY ["AccountModule/Volo.Abp.Account.HttpApi/Volo.Abp.Account.HttpApi.csproj", "AccountModule/Volo.Abp.Account.HttpApi/"]
COPY ["AccountModule/Volo.Abp.Account.Application.Contracts/Volo.Abp.Account.Application.Contracts.csproj", "AccountModule/Volo.Abp.Account.Application.Contracts/"]
COPY ["AccountModule/Volo.Abp.Account.Web.IdentityServer/Volo.Abp.Account.Web.IdentityServer.csproj", "AccountModule/Volo.Abp.Account.Web.IdentityServer/"]
COPY ["AccountModule/Volo.Abp.Account.Application/Volo.Abp.Account.Application.csproj", "AccountModule/Volo.Abp.Account.Application/"]
RUN dotnet restore "AuthServer.Host/AuthServer.Host.csproj"
COPY . .
WORKDIR "/src/AuthServer.Host"
RUN dotnet build "AuthServer.Host.csproj" --no-restore -nowarn:msb3202,msb3277,nu1503 -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthServer.Host.csproj" --no-restore -nowarn:msb3202,msb3277,nu1503 -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthServer.Host.dll"]