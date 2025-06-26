# Step 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY SinalR/*.csproj ./SinalR/
COPY SinalR.sln ./
RUN dotnet restore SinalR.sln

# Copy everything else and publish
COPY . ./
RUN dotnet publish SinalR/SinalR.csproj -c Release -o /app/publish

# Step 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SinalR.dll"]
