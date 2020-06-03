FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["RESTAPI/RESTAPI.csproj", "RESTAPI/"]
RUN dotnet restore "RESTAPI/RESTAPI.csproj"
COPY . .
WORKDIR "/src/RESTAPI"
RUN dotnet build "RESTAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RESTAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "RESTAPI.dll"]