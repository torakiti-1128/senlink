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


## 4. データベースのセットアップ
Supabase (PostgreSQL) を使用します。開発者は各自でデータベース環境を構築してください。これは、開発環境の再現性を確保し、チーム開発におけるデータ競合を防ぐための設計です。

### 4-1. Supabase プロジェクトの準備
1. [Supabase](https://supabase.com/) にサインアップし、新しい Project を作成します。
2. 作成後、Supabaseのヘッダー（Webサイト上部）から**Connect** を開きます。
3. 新たに表示された画面上にある **Method** から `Transaction pooler` に切り替えます。
4. `Connection string` の **URI** をコピーします。

※ IPv4 ネットワーク環境から接続する場合、Direct Connection (Port: 5432) では接続できないため、Pooler (Port: 6543) を使用します。

### 4-2. 接続文字列の設定 (User Secrets)
セキュリティのため、接続文字列はソースコードに含めず `User Secrets` で管理します。
Supabaseから取得したURI（前述4-1の項番4）を、以下の対応表に従って各パラメータに分解して実行してください。

#### URIの構成と対応表
URIは以下の規則で並んでいます：
`postgresql://【Username】:[YOUR-PASSWORD]@【Host】:【Port】/【Database】`

| 項目 | URI内の場所 | 設定例 |
| :--- | :--- | :--- |
| **Host** | `@` 以降から `:` の前まで | `aws-1-ap-southeast-1.pooler.supabase.com` |
| **Username** | `://` 以降から `:` の前まで | `postgres.xxx` |
| **Password** | `:` 以降から `@` の前まで | プロジェクト作成時のパスワード |
| **Port** | ホスト名の後ろの数字 | `6543` |
| **Database** | 最後の `/` 以降 | `postgres` |

#### 実行コマンド
ターミナルでルートディレクトリから、以下のコマンドを実行してください。

```bash
cd src/SenLink.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Host=【Host】;Database=postgres;Username=【Username】;Password=[YOUR-PASSWORD];Port=6543;IPv6=false'
cd ../..
```

### 4-3. 実行権限の付与 (Linux / macOS / WSL)
自動化スクリプトを実行可能にします。

```bash
chmod +x scripts/*.sh
```

### 4-4. データベースの構築
既存のマイグレーション（設計図）を、各自のデータベースへ反映します。

```bash
./scripts/update_database.sh
```

### 補足
このプロジェクトでは、開発経験の有無に関わらずスムーズに共同開発へ参加できるよう、以下の工夫をしています。

- **オンボーディングの簡略化**: 詳細なガイドにより、環境構築時の「動かない」というストレスを最小限に抑えています。
- **環境依存の排除**: IPv6エラーやシェルの特殊文字解釈など、実務で発生しがちなトラブルを先回りして解決策を提示しています。
- **セキュリティの担保**: 各自が秘密情報を安全に管理できる手法（User Secrets）を標準化しています。


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