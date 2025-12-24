FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Solution1.sln .
COPY Training.ExpenseTracker.WebApi/Training.ExpenseTracker.WebApi.csproj Training.ExpenseTracker.WebApi/
COPY Training.ExpenseTracker.Application/Training.ExpenseTracker.Application.csproj Training.ExpenseTracker.Application/
COPY Training.ExpenseTracker.Infrastructure/Training.ExpenseTracker.Infrastructure.csproj Training.ExpenseTracker.Infrastructure/
COPY Training.ExpenseTracker.Domain/Training.ExpenseTracker.Domain.csproj Training.ExpenseTracker.Domain/

RUN dotnet restore Solution1.sln

COPY . .

RUN dotnet publish Training.ExpenseTracker.WebApi/Training.ExpenseTracker.WebApi.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN addgroup -g 1000 appgroup && adduser -u 1000 -G appgroup -D appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Training.ExpenseTracker.WebApi.dll"]
