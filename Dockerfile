FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FoodDesk.WEB/FoodDesk.WEB.csproj", "FoodDesk.WEB/"]
COPY ["FoodDesk.Application/FoodDesk.Application.csproj", "FoodDesk.Application/"]
COPY ["FoodDesk.Domain/FoodDesk.Domain.csproj", "FoodDesk.Domain/"]
COPY ["FoodDesk.Infrastructure/FoodDesk.Infrastructure.csproj", "FoodDesk.Infrastructure/"]
COPY ["FoodDesk.Persistence/FoodDesk.Persistence.csproj", "FoodDesk.Persistence/"]
RUN dotnet restore "FoodDesk.WEB/FoodDesk.WEB.csproj"
COPY . .
WORKDIR "/src/FoodDesk.WEB"
RUN dotnet build "FoodDesk.WEB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoodDesk.WEB.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodDesk.WEB.dll"] 