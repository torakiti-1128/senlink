#!/bin/bash
# マイグレーションを追加す

# 引数チェックのロジック
if [ -z "$1" ] || [ -z "$2" ]; then
    echo "エラー: 引数が足りません。"
    echo "使用例: ./scripts/add_migration.sh マイグレーション名 モジュール名"
    echo "例: ./scripts/add_migration.sh Initial_School_Setup School"
    exit 1
fi

MIGRATION_NAME=$1
MODULE_NAME=$2

# 出力先パスを動的に生成するロジック
OUTPUT_DIR="Modules/${MODULE_NAME}/Persistence/Migrations"

echo "Creating migration '$MIGRATION_NAME' for module '$MODULE_NAME'..."
echo "Output directory: $OUTPUT_DIR"

dotnet ef migrations add "$MIGRATION_NAME" \
    --project src/SenLink.Infrastructure/SenLink.Infrastructure.csproj \
    --startup-project src/SenLink.Api/SenLink.Api.csproj \
    --output-dir "$OUTPUT_DIR"

echo "Migration '$MIGRATION_NAME' created successfully in module '$MODULE_NAME'."