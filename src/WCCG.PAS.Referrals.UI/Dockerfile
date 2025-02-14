FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./WCCG.PAS.Referrals.UI.csproj", "WCCG.PAS.Referrals.UI/"]
RUN dotnet restore "./WCCG.PAS.Referrals.UI/WCCG.PAS.Referrals.UI.csproj"
COPY . "./WCCG.PAS.Referrals.UI"
WORKDIR "/src/WCCG.PAS.Referrals.UI"
RUN dotnet build "./WCCG.PAS.Referrals.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WCCG.PAS.Referrals.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WCCG.PAS.Referrals.UI.dll"]
