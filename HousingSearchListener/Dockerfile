FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
COPY ./HousingSearchListener/HousingSearchListener.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./HousingSearchListener.csproj

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o out ./HousingSearchListener.csproj
RUN dotnet publish -c Release -o out ./HousingSearchListener.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/HousingSearchListener.dll
