FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EstoqueBackEnd.csproj", "./"]
RUN dotnet restore "EstoqueBackEnd.csproj"
COPY . .
RUN dotnet publish "EstoqueBackEnd.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EstoqueBackEnd.dll"]