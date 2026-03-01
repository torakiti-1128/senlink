#!/bin/bash
# 最後に作成されたマイグレーションファイルを削除し、DbContextの状態を一つ前に戻す

echo "Removing the last migration..."

dotnet ef migrations remove \
    --project src/SenLink.Infrastructure/SenLink.Infrastructure.csproj \
    --startup-project src/SenLink.Api/SenLink.Api.csproj

echo "Last migration removed."