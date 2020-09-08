FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

# Copy solution and project files separately and restore NuGet packages to create layers
COPY *.sln .
COPY Equinor.ProCoSys.IPO.Command/*.csproj ./Equinor.ProCoSys.IPO.Command/
COPY tests/Equinor.ProCoSys.IPO.Command.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.Command.Tests/
COPY Equinor.ProCoSys.IPO.Domain/*.csproj ./Equinor.ProCoSys.IPO.Domain/
COPY tests/Equinor.ProCoSys.IPO.Domain.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.Domain.Tests/
COPY Equinor.ProCoSys.IPO.Infrastructure/*.csproj ./Equinor.ProCoSys.IPO.Infrastructure/
COPY tests/Equinor.ProCoSys.IPO.Infrastructure.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.Infrastructure.Tests/
COPY Equinor.ProCoSys.IPO.MainApi/*.csproj ./Equinor.ProCoSys.IPO.MainApi/
COPY tests/Equinor.ProCoSys.IPO.MainApi.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.MainApi.Tests/
COPY Equinor.ProCoSys.IPO.BlobStorage/*.csproj ./Equinor.ProCoSys.IPO.BlobStorage/
COPY Equinor.ProCoSys.IPO.Query/*.csproj ./Equinor.ProCoSys.IPO.Query/
COPY tests/Equinor.ProCoSys.IPO.Query.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.Query.Tests/
COPY Equinor.ProCoSys.IPO.WebApi/*.csproj ./Equinor.ProCoSys.IPO.WebApi/
COPY tests/Equinor.ProCoSys.IPO.WebApi.Tests/*.csproj ./tests/Equinor.ProCoSys.IPO.WebApi.Tests/
COPY tests/Equinor.ProCoSys.IPO.Test.Common/*.csproj ./tests/Equinor.ProCoSys.IPO.Test.Common/

# Restore NuGet packages
RUN dotnet restore

# Copy full solution
COPY . .

# Build
RUN dotnet build -c Release --no-restore

# Test runner target
FROM build AS testrunner
WORKDIR /src
RUN dotnet test -c Release --no-build --no-restore

# Publish the application
FROM build AS publish
WORKDIR "/src/Equinor.ProCoSys.IPO.WebApi"
RUN dotnet publish "Equinor.ProCoSys.IPO.WebApi.csproj" -c Release --no-restore -o /app/publish

# Define the image used for the final result
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base

# Install System.Drawing native dependencies (added because of Excel export (ClosedXML library) support).
RUN apt-get update
RUN apt-get install -y libc6 libgdiplus
RUN rm -rf /var/lib/apt/lists/*

# Create non-root user. Set ui to 9999 to avoid conflicts with host OS just in case
RUN adduser --disabled-password --uid 9999 --gecos "" apprunner
# Create the folder and set the non-root as owner
RUN mkdir /app && chown apprunner.apprunner /app
# Change the user from root to non-root- From now on, all Docker commands are run as non-root user (except for COPY)
USER apprunner
# Set the port to 5000 since the default production port is 80 and on-root users cannot bind to this port
ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
WORKDIR /app
# Copy the published files from the build image into this one
# COPY defaults to copying files as root, specify the user that should be the owner
COPY --chown=apprunner:apprunner --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.IPO.WebApi.dll"]