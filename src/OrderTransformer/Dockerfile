FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/OrderTransformer/OrderTransformer.csproj ./src/OrderTransformer/
RUN dotnet restore src/OrderTransformer/OrderTransformer.csproj
COPY src/OrderTransformer/ ./src/OrderTransformer/
RUN dotnet publish src/OrderTransformer/OrderTransformer.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OrderTransformer.dll"]
