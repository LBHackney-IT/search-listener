FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app

RUN apt-get update && apt-get install -y openjdk-17-jdk
RUN dotnet tool install --global dotnet-sonarscanner
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_housing-search-listener" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"


# Copy csproj and nuget config and restore as distinct layers
COPY ./HousingSearchListener.sln ./
COPY ./HousingSearchListener/HousingSearchListener.csproj ./HousingSearchListener/
COPY ./HousingSearchListener.Tests/HousingSearchListener.Tests.csproj ./HousingSearchListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./HousingSearchListener/HousingSearchListener.csproj
RUN dotnet restore ./HousingSearchListener.Tests/HousingSearchListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out HousingSearchListener/HousingSearchListener.csproj
RUN dotnet build -c debug -o out HousingSearchListener.Tests/HousingSearchListener.Tests.csproj

CMD dotnet test
RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
