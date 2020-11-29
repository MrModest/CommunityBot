FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ./CommunityBot.sln ./
COPY ./CommunityBot/*.csproj ./CommunityBot/

RUN dotnet restore "CommunityBot/CommunityBot.csproj"
COPY . .
WORKDIR "/src/CommunityBot"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS final
EXPOSE 443
WORKDIR /app
COPY --from=build /app/publish .

ENV SQLite__DbFilePath /app/db/database.sqlite

ENV ASPNETCORE_URLS https://+;http://+
#ENV ASPNETCORE_HTTPS_PORT 5001 #ignored

ENTRYPOINT ["dotnet", "CommunityBot.dll"]
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet CommunityBot.dll
