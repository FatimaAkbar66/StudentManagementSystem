FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SMS.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish SMS.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data
ENV DB_PATH=/app/data/sms.db
ENTRYPOINT ["dotnet", "SMS.dll"]