#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#
#
#COPY ["Bank.csproj", "NuGet.config", "./"]
#
#
#RUN dotnet restore "Bank.csproj" -p:TargetFramework=net8.0
#
#
#COPY . .
#RUN dotnet publish "Bank.csproj" -c Release -o /app -p:TargetFramework=net8.0
#
#FROM base AS final
#WORKDIR /app
#COPY --from=build /app .
#ENTRYPOINT ["dotnet", "Bank.dll"]