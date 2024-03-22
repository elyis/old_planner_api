FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

COPY ./*.csproj ./

RUN dotnet restore

COPY . ./

RUN dotnet publish -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "old_planner_api.dll"]