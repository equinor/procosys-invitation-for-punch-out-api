FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project files separately and restore NuGet packages to create layers. Skip test projects!
COPY Equinor.ProCoSys.IPO.Command/*.csproj ./Equinor.ProCoSys.IPO.Command/
COPY Equinor.ProCoSys.IPO.Domain/*.csproj ./Equinor.ProCoSys.IPO.Domain/
COPY Equinor.ProCoSys.IPO.Infrastructure/*.csproj ./Equinor.ProCoSys.IPO.Infrastructure/
COPY Equinor.ProCoSys.IPO.ForeignApi/*.csproj ./Equinor.ProCoSys.IPO.ForeignApi/
COPY Equinor.ProCoSys.IPO.Query/*.csproj ./Equinor.ProCoSys.IPO.Query/
COPY Equinor.ProCoSys.IPO.WebApi/*.csproj ./Equinor.ProCoSys.IPO.WebApi/
COPY tests/Equinor.ProCoSys.IPO.WebApi.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.WebApi.Tests/
COPY tests/Equinor.ProCoSys.IPO.Test.Common/*.csproj ./tests/Equinor.ProCoSys.IPO.Test.Common/

COPY nuget.config .

# Copy full solution
COPY . .

# Set workdir where main project is
WORKDIR "/src/Equinor.ProCoSys.IPO.WebApi"

# Build
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ARG FEED_ACCESSTOKEN
RUN echo $FEED_ACCESSTOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
RUN echo $VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
RUN dotnet build "Equinor.ProCoSys.IPO.WebApi.csproj" -c Release

# Publish the application
FROM build AS publish
WORKDIR "/src/Equinor.ProCoSys.IPO.WebApi"
RUN dotnet publish "Equinor.ProCoSys.IPO.WebApi.csproj" -c Release --no-restore -o /app/publish

# Define the image used for the final result
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# Install System.Drawing native dependencies (added because of Excel export (ClosedXML library) support).
RUN apt-get update
RUN apt-get install -y libc6 libgdiplus
RUN rm -rf /var/lib/apt/lists/*

# Create non-root user. Set ui to 9999 to avoid conflicts with host OS just in case
RUN adduser --disabled-password --uid 9999 --gecos "" apprunner

# Create the folder and set the non-root as owner
RUN mkdir /app && chown apprunner.apprunner /app

# Change the user from root to non-root- From now on, all Docker commands are run as non-root user (except for COPY)
USER 9999

# Set the port to 5000 since the default production port is 80 and on-root users cannot bind to this port
ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
WORKDIR /app

# Copy the published files from the build image into this one
# Copy defaults to copying files as root, specify the user that should be the owner
COPY --chown=apprunner:apprunner --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.IPO.WebApi.dll"]