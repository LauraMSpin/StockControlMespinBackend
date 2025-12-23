FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["EstoqueBackEnd.csproj", "./"]
RUN dotnet restore "EstoqueBackEnd.csproj" -a $TARGETARCH
COPY . .
RUN dotnet publish "EstoqueBackEnd.csproj" -c Release -o /app/publish -a $TARGETARCH --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EstoqueBackEnd.dll"]