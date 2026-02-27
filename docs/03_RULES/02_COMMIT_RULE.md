# Git コミット & ブランチ運用ルール (SenLink)

## 1. コミットメッセージの形式
[Conventional Commits](https://www.conventionalcommits.org/) に準拠します。

### 基本書式
`type(scope): subject (#issue_number)`

### Type（必須）
| Type | 説明 |
| :--- | :--- |
| **feat** | 新機能の開発、データベーステーブルの追加 |
| **fix** | バグ修正 |
| **docs** | ドキュメントのみの変更（Markdownなど） |
| **style** | コードの意味に影響を与えない変更（改行、フォーマット等） |
| **refactor** | バグ修正も新機能追加も行わないコードの変更 |
| **test** | テストの追加・修正 |
| **chore** | ビルドプロセス、パッケージ管理ツールの更新（NuGet追加など） |
| **infra** | Docker, CI/CD, 環境構築関連 |

### Scope
どのプロジェクト・層を変更したかを明示します。
- `api`, `service`, `domain`, `infra`, `shared`, `worker`, `db`, `ci`

### 具体的な例
- `feat): Jobサービス関連のテーブル定義を追加 (#7)`
- `chore(api): Npgsqlパッケージをインストール (#7)`
- `docs(git): コミットルールガイドを追加 (#8)`
- `fix(infra): Docker Compose の環境変数を修正 (#1)`

---

## 2. ブランチ運用ルール

### メインブランチ
- **main**: 本番環境用。常にリリース可能な状態を維持。
- **dev**: 開発統合ブランチ。すべての機能はこのブランチから分岐し、ここへマージされる。
  - **⚠️ `dev` への直接 push は原則禁止。** 必ず Pull Request (PR) を経由する。

### 作業ブランチ (Feature branches)
`feature/{issue_number}-{summary}` の形式で作成します。
- 例: `feature/7-database-setup`

---

## 3. プルリクエスト (PR) の作法
1. **セルフレビュー**: PRを出す前に、自分で `git diff` を確認する。
2. **動作確認**: `dotnet build` と `dotnet test` が通ることを手元で確認する。
3. **説明文**: 「何をしたか」だけでなく「なぜしたか」を簡潔に記載する。