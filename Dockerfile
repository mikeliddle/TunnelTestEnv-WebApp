FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["sampleWebService.csproj", ""]
RUN dotnet restore "./sampleWebService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "sampleWebService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "sampleWebService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY entrypoint.sh entrypoint.sh
ENTRYPOINT ["bash", "entrypoint.sh"]
