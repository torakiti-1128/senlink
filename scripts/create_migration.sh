#!/bin/bash
# 新しいマイグレーションを作成するスクリプト

set -e # エラーが発生した時点でスクリプトを終了する

# 引数（マイグレーション名）が指定されているかチェック
if [ -z "$1" ]; then
  echo "エラー: マイグレーション名を指定してください。"
  echo "使い方: ./scripts/create_migration.sh <MigrationName>"
  echo "例: ./scripts/create_migration.sh Init_Database_Setup"
  exit 1
fi

MIGRATION_NAME=$1

echo "Creating new migration: $MIGRATION_NAME for SenLink..."

# マイグレーションの作成
dotnet ef migrations add "$MIGRATION_NAME" \
    --project src/SenLink.Infrastructure/SenLink.Infrastructure.csproj \
    --startup-project src/SenLink.Api/SenLink.Api.csproj \
    --output-dir Migrations

echo "Migration '$MIGRATION_NAME' created successfully!"