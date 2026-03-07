FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "src/ExpenseManager.API/ExpenseManager.API.csproj"
RUN dotnet publish "src/ExpenseManager.API/ExpenseManager.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS run
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5032
EXPOSE 5032
ENTRYPOINT ["dotnet", "ExpenseManager.API.dll"]
