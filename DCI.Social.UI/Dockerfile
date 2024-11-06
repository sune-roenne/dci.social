# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DCI.Social.UI/DCI.Social.UI.csproj", "DCI.Social.UI/"]
COPY ["DCI.Social.Identity/DCI.Social.Identity.csproj", "DCI.Social.Identity/"]
ENV http_proxy http://webproxy.nykreditnet.net:8080/
ENV https_proxy http://webproxy.nykreditnet.net:8080/
RUN dotnet nuget remove source nuget.org
RUN dotnet nuget add source http://maven.nykreditnet.net/nexus/repository/nuget.org/ --name nuget.org.nykredit
RUN dotnet nuget add source http://maven.nykreditnet.net/nexus/repository/itcm_nuget/ --name itcm.nykredit
RUN dotnet restore "./DCI.Social.UI/DCI.Social.UI.csproj"
COPY . .
WORKDIR "/src/DCI.Social.UI"
RUN dotnet build "./DCI.Social.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DCI.Social.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ENV "ASPNETCORE_URLS"="http://*:8080"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DCI.Social.UI.dll"]