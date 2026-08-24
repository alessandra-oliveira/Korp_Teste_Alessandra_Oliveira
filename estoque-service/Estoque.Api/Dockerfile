FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Estoque.Api.csproj", "."]
RUN dotnet restore "Estoque.Api.csproj"

COPY . .
RUN dotnet publish "Estoque.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Estoque.Api.dll"]