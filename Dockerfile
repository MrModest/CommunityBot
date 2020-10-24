FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["CommunityBot/CommunityBot.csproj", "CommunityBot/"]
RUN dotnet restore "CommunityBot/CommunityBot.csproj"
COPY . .
WORKDIR "/src/CommunityBot"
RUN dotnet build "CommunityBot.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "CommunityBot.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommunityBot.dll"]
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet CommunityBot.dll