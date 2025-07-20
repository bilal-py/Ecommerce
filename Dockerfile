
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Use PORT environment variable with 80 as fallback
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ecommerce/Ecommerce.csproj", "Ecommerce/"]
RUN dotnet restore "./Ecommerce/Ecommerce.csproj"
COPY . .
WORKDIR "/src/Ecommerce"
RUN dotnet build "./Ecommerce.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Add runtime environment variables support
ARG ConnectionStrings__DefaultConnection
ARG DATABASE_URL
ARG ADMIN1_EMAIL
ARG ADMIN1_USERNAME
ARG ADMIN1_PASSWORD
ARG ADMIN2_EMAIL
ARG ADMIN2_USERNAME
ARG ADMIN2_PASSWORD
ARG Email__SmtpHost
ARG Email__Port
ARG Email__Username
ARG Email__Password
ARG Email__From
ARG Email__AdminEmail

ENV ConnectionStrings__DefaultConnection=$ConnectionStrings__DefaultConnection \
    DATABASE_URL=$DATABASE_URL \
    ADMIN1_EMAIL=$ADMIN1_EMAIL \
    ADMIN1_USERNAME=$ADMIN1_USERNAME \
    ADMIN1_PASSWORD=$ADMIN1_PASSWORD \
    ADMIN2_EMAIL=$ADMIN2_EMAIL \
    ADMIN2_USERNAME=$ADMIN2_USERNAME \
    ADMIN2_PASSWORD=$ADMIN2_PASSWORD \
    Email__SmtpHost=$Email__SmtpHost \
    Email__Port=$Email__Port \
    Email__Username=$Email__Username \
    Email__Password=$Email__Password \
    Email__From=$Email__From \
    Email__AdminEmail=$Email__AdminEmail

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
