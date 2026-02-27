#!/bin/bash
# データベースのセットアップを行う

set -e # エラー内容を表示してスクリプトを終了する

echo "Initializing database for SenLink..."

# 1. DBを最新の状態にする（既存の全マイグレーションを適用）
echo "Step 1: Applying all migrations..."
dotnet ef database update \
    --project src/SenLink.Infrastructure/SenLink.Infrastructure.csproj \
    --startup-project src/SenLink.Api/SenLink.Api.csproj

# 2. 初期データの投入（Seeding）
# ToDo：将来的に初期データをここで投入するためのコードを追加予定
echo "Step 2: Seeding initial data..."
# dotnet run --project src/SenLink.Api/SenLink.Api.csproj -- --seed

echo "Database initialization complete!"