# 開発環境セットアップ

このドキュメントでは、SenLink の開発を開始するために必要な手順を説明します。

## 1. 前提条件
以下のツールがインストールされていることを確認してください。

- **Windows 11 + WSL 2 (Ubuntu 22.04 推奨)**
- **Docker Desktop**
    - Settings > Resources > WSL Integration で、使用している Ubuntu の連携が有効であること。
- **.NET 8.0 SDK**
- **Visual Studio Code** (推奨拡張機能: C# Dev Kit, Docker)


## 2. リポジトリのクローン
```bash
git clone <repository-url>
cd senlink
```


## 3. 環境構築 (Docker)
プロジェクトは Docker Compose を使用して、API、Worker、RabbitMQ、Apache を一括で起動します。

```bash
# コンテナのビルドと起動
docker compose up -d --build
```

起動後、以下のエンドポイントにアクセスできるか確認してください。
- API Swagger UI: http://localhost:5000/swagger/index.html
- RabbitMQ Management: http://localhost:15672 (ユーザー名：guest / パスワード：guest)


## 4. データベースのセットアップ (今後実装予定)
現在はコンテナの起動のみで動作しますが、今後の実装により Supabase (PostgreSQL) への接続設定が必要になります。


## 5. テストの実行
品質を担保するため、プルリクエストの作成前には必ず全てのテストがパスすることを確認してください。

```bash
# 全プロジェクトのテスト実行
dotnet test
```

### テストプロジェクトの構成
- **SenLink.Domain.Tests**: ドメインモデル・ビジネスルールのテスト
- **SenLink.Service.Tests**: ユースケースのテスト
- **SenLink.Infrastructure.Tests**: DB接続・外部リポジトリのテスト
- **SenLink.Api.Tests**: APIエンドポイントの統合テスト
- **SenLink.Architecture.Tests**: 依存関係ルールのチェック