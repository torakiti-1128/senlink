#!/bin/bash
# 適用されていないマイグレーションをDBへ反映する

echo "Applying migrations to database..."

dotnet ef database update \
    --project src/SenLink.Infrastructure/SenLink.Infrastructure.csproj \
    --startup-project src/SenLink.Api/SenLink.Api.csproj

echo "Database updated successfully!"