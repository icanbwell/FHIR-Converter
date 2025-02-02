FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:3.1
WORKDIR /App
COPY data/Templates /Templates
COPY data/SampleData /SampleData
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "Microsoft.Health.Fhir.Liquid.Converter.Tool.dll"]
