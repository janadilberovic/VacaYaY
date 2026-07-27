FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/VacaYAY.Api/VacaYAY.Api.csproj src/VacaYAY.Api/
COPY src/VacaYAY.Business/VacaYAY.Business.csproj src/VacaYAY.Business/
COPY src/VacaYAY.Data/VacaYAY.Data.csproj src/VacaYAY.Data/
COPY src/VacaYAY.Domain/VacaYAY.Domain.csproj src/VacaYAY.Domain/
RUN dotnet restore src/VacaYAY.Api/VacaYAY.Api.csproj

COPY src/ src/
RUN dotnet publish src/VacaYAY.Api/VacaYAY.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VacaYAY.Api.dll"]
