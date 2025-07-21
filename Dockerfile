
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
# Use PORT environment variable with 80 as fallback
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Ecommerce/Ecommerce.csproj", "Ecommerce/"]
RUN dotnet restore "Ecommerce/Ecommerce.csproj"
COPY . .
WORKDIR "/src/Ecommerce"
RUN dotnet build "Ecommerce.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.dll"]





# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# WORKDIR /app
# EXPOSE 80

# ENV ASPNETCORE_URLS=http://+:8080
# ENV ASPNETCORE_ENVIRONMENT=Production

# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# ARG BUILD_CONFIGURATION=Release
# WORKDIR /src
# COPY ["Ecommerce/Ecommerce.csproj", "Ecommerce/"]
# RUN dotnet restore "./Ecommerce/Ecommerce.csproj"
# COPY . .
# WORKDIR "/src/Ecommerce"
# RUN dotnet build "./Ecommerce.csproj" -c $BUILD_CONFIGURATION -o /app/build

# FROM build AS publish
# ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "./Ecommerce.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "Ecommerce.dll"]
