# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SF5_Tournament/SF5_Tournament.csproj", "SF5_Tournament/"]
RUN dotnet restore "SF5_Tournament/SF5_Tournament.csproj"

COPY . .
WORKDIR "/src/SF5_Tournament"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:10000

EXPOSE 10000
ENTRYPOINT ["dotnet", "SF5_Tournament.dll"]
