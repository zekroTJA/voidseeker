FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RESTAPI/RESTAPI.csproj", "RESTAPI/"]
RUN dotnet restore "RESTAPI/RESTAPI.csproj"
COPY . .
WORKDIR "/src/RESTAPI"
RUN dotnet build "RESTAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RESTAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "RESTAPI.dll"]