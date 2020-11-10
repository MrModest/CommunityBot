FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ./CommunityBot.sln ./
COPY ./CommunityBot/*.csproj ./CommunityBot/

RUN dotnet restore "CommunityBot/CommunityBot.csproj"
COPY . .
WORKDIR "/src/CommunityBot"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV SQLite__DbFilePath /app/db/database.sqlite

ENV ASPNETCORE_URLS https://+;http://+
#ENV ASPNETCORE_HTTPS_PORT 5001 #ignored

ENTRYPOINT ["dotnet", "CommunityBot.dll"]
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet CommunityBot.dll
