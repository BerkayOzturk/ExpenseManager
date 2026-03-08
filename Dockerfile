# Production: single image = API + frontend (API serves SPA at same origin)
# Build frontend
FROM node:20-alpine AS frontend
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json* ./
RUN npm ci
COPY frontend/ .
RUN npm run build

# Build API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
COPY . .
RUN dotnet restore src/ExpenseManager.API/ExpenseManager.API.csproj
RUN dotnet publish src/ExpenseManager.API/ExpenseManager.API.csproj -c Release -o /app/publish --no-restore

# Combine: API publish + frontend in wwwroot
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS combined
COPY --from=api-build /app/publish /app/publish
COPY --from=frontend /app/dist /app/publish/wwwroot

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS run
WORKDIR /app
COPY --from=combined /app/publish .
# Persist DB: use /data (mount a volume here on Railway so data survives redeploys)
ENV ConnectionStrings__Default=Data Source=/data/expensemanager.db
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "ExpenseManager.API.dll"]
