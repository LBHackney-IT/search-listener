FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
COPY ./HousingSearchListener.sln ./
COPY ./HousingSearchListener/HousingSearchListener.csproj ./HousingSearchListener/
COPY ./HousingSearchListener.Tests/HousingSearchListener.Tests.csproj ./HousingSearchListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./HousingSearchListener/HousingSearchListener.csproj
RUN dotnet restore ./HousingSearchListener.Tests/HousingSearchListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out HousingSearchListener.Tests/HousingSearchListener.Tests.csproj

CMD dotnet test
