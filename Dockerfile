# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK as a build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ./WebAPI/WebAPI.csproj WebAPI/
COPY ./Auth/Auth.csproj Auth/
COPY ./DatabaseInterfacing/DatabaseInterfacing.csproj DatabaseInterfacing/
COPY ./IoTInterfacing/IoTInterfacing.csproj IoTInterfacing/
COPY ./Application/Application.csproj Application/

RUN dotnet restore ./WebAPI/WebAPI.csproj
RUN dotnet restore ./Auth/Auth.csproj
RUN dotnet restore ./DatabaseInterfacing/DatabaseInterfacing.csproj
RUN dotnet restore ./IoTInterfacing/IoTInterfacing.csproj
RUN dotnet restore ./Application/Application.csproj


# Copy the entire project and build the release version
COPY . .
RUN dotnet publish "WebAPI/WebAPI.csproj" -c Release -o /app/publish

# Use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]