# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY DatabaseQueryApp.csproj ./
RUN dotnet restore DatabaseQueryApp.csproj

# Copy the rest of the source code
COPY . ./

# Build the application in Release mode
RUN dotnet publish DatabaseQueryApp.csproj -c Release -o out

# Use the official .NET 8.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 8005
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8005

# Define the entry point
ENTRYPOINT ["dotnet", "DatabaseQueryApp.dll"]