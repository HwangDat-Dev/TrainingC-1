FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Solution1.sln .
COPY Training.ExpenseTracker.WebApi/Training.ExpenseTracker.WebApi.csproj Training.ExpenseTracker.WebApi/
COPY Training.ExpenseTracker.Application/Training.ExpenseTracker.Application.csproj Training.ExpenseTracker.Application/
COPY Training.ExpenseTracker.Infrastructure/Training.ExpenseTracker.Infrastructure.csproj Training.ExpenseTracker.Infrastructure/
COPY Training.ExpenseTracker.Domain/Training.ExpenseTracker.Domain.csproj Training.ExpenseTracker.Domain/

# Restore dependencies
RUN dotnet restore Solution1.sln

# Copy the rest of the source code
COPY . .

# Publish the WebApi project
RUN dotnet publish Training.ExpenseTracker.WebApi/Training.ExpenseTracker.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Expose HTTP port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Training.ExpenseTracker.WebApi.dll"]

