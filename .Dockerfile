# build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY ["source/", "source/"]
WORKDIR "/source/ExampleApp"
RUN dotnet publish ExampleApp.fsproj \
    -c Release \
    -o /app/publish \
    --os linux \
    --arch x64 \
    --no-self-contained

# run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 443
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ExampleApp.dll"]