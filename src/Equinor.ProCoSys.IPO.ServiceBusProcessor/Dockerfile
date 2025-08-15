FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src/Equinor.ProCoSys.IPO.ServiceBusProcessor

RUN ls -la
RUN tree

RUN dotnet restore "Equinor.ProCoSys.IPO.ServiceBusProcessor.csproj"
RUN dotnet build "Equinor.ProCoSys.IPO.ServiceBusProcessor.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Equinor.ProCoSys.IPO.ServiceBusProcessor.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=publish /app .
USER 1000
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.IPO.ServiceBusProcessor.dll"]
