# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory inside the container
WORKDIR /app/InventoryService

COPY  InventoryService/InventoryService.csproj .
RUN dotnet restore
COPY InventoryService/. .
RUN #dotnet build -c Release


# Build the application
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image as the base image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the published output from the build image to the runtime image
COPY --from=build /app/InventoryService/out .

# Expose the port your Giraffe app is listening on (change 8080 to the appropriate port)
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "InventoryService.dll"]