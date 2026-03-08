# SenLink (センリンク)

> **「学生・教員・企業」の繋がりを最適化し、就職活動のエンジニアリングを再定義する。**

SenLinkは、専門学校における就職活動支援の課題（情報の断片化、教員の事務負担、学生のタスク管理不足）を解決するための、堅牢なバックエンド設計とモダンなフロントエンドを組み合わせた就職活動支援プラットフォームです。

## プロジェクトのビジョン：エンジニアリングによる「リベンジ」

本プロジェクトは、過去の「コードファーストで品質管理を欠いた開発」への反省から生まれた**「リベンジプロジェクト」**です。単なる機能実装に留まらず、以下のエンジニアリング品質を実証することを目的としています。

- **設計の徹底**: モジュラーモノリス構成による境界の明確化と、疎結合な依存関係の維持。
- **品質の数値化**: テスト駆動開発（TDD）の完遂と、カバレッジ80%以上の維持。
- **プロセスの自動化**: 静的解析、CIを通じたデリバリ品質の担保。

---

## アーキテクチャ：Modular Monolith

「スケーラビリティ」と「開発効率」を両立するため、**モジュラーモノリス（Modular Monolith）**を採用しています。将来的なマイクロサービス化を見据えつつ、現時点でのデプロイ・管理コストを最小化しています。

### 核心的な設計思想
- **Domain-Centric Design**: `api / worker → domain ← infra` の依存関係を徹底し、ビジネスロジックを技術基盤（DB等）から独立。
- **BFF (Backend For Frontend) パターン**: 画面要件に応じたデータの集約（Aggregation）を、各ドメインを跨ぐ Port（Interface）経由で実行。
- **非同期分散処理**: 即時性を要しない処理（通知、監査ログ記録等）は RabbitMQ + Worker コンテナへ委譲。

### モジュール構成
- **Auth**: セキュリティ、認証・認可
- **School**: 学校、クラス、教員・学生管理
- **Job**: 求人、企業、インターンシップ管理
- **Activity**: 学生の活動履歴、フィードバック
- **Request**: 書類添削、面接予約等のワークフロー
- **Notification**: マルチチャネル通知（システム内、メール等）
- **Audit**: システム操作の監査ログ記録
- **Maintenance**: システム設定、稼働状況管理

---

## 🛠 技術スタック

| レイヤー | 技術 | 選定理由 |
| :--- | :--- | :--- |
| **Language** | **C# 12 (.NET 8)** | 静的型付けと最新の言語機能（Primary Constructors等）による安全性。 |
| **Framework** | **ASP.NET Core 8.0** | 高い実行パフォーマンスと、DI/Middleware等の充実した標準機能。 |
| **Database** | **PostgreSQL 16** | 信頼性と拡張性。EF Core 8.0 による Code First 管理。 |
| **Messaging** | **RabbitMQ (MassTransit)** | サービス間の疎結合化と、信頼性の高い非同期メッセージング。 |
| **Frontend** | **Next.js (App Router)** | SSR/ISRによる最適化と、shadcn/ui による高いUXの提供。 |
| **Testing** | **xUnit + Moq** | TDDの標準。モックによる徹底した単体テスト。 |
| **Infra** | **Docker + Apache** | 環境再現性の確保と、Reverse Proxy によるセキュリティ制御。 |

---

## エンジニアリング・スタンダード

技術志向な開発プロセスを遵守しています。

### テスト駆動開発 (TDD)
すべてのビジネスロジックは **Red-Green-Refactor** のサイクルを経て実装されます。
- `Domain` および `Service` 層に対する単体テスト網羅。
- `Architecture.Tests` による、レイヤー間の依存関係違反の自動検知（NetArchTest等）。

### ドキュメント駆動
`docs/` 配下にて要件定義、DB設計（ER図）、API定義（kebab-case準拠）、アーキテクチャ詳細を管理。コードと設計の乖離を防ぎます。

---

## はじめに (Getting Started)

### 前提条件
- Docker / Docker Compose
- .NET 8 SDK
- Node.js (Latest LTS)

### 起動手順
```bash
# リポジトリのクローン
git clone https://github.com/torakiti-1128/senlink.git
cd senlink

# インフラ（DB, RabbitMQ, Apache）の起動
docker-compose up -d

# データベースマイグレーションの適用
./scripts/apply_migration.sh

# APIの起動
dotnet run --project src/SenLink.Api
```
