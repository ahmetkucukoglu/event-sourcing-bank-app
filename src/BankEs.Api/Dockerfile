FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/BankEs.Api/BankEs.Api.csproj", "BankEs.Api/"]
RUN dotnet restore "src/BankEs.Api/BankEs.Api.csproj"
COPY . .
WORKDIR "/src/BankEs.Api"
RUN dotnet build "BankEs.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankEs.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankEs.Api.dll"]
