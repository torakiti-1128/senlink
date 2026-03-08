# 実装ルール：SenLink (センリンク)

本プロジェクトの実装における共通の技術ルール、コードスタイル、および設計原則を定義します。

## 1. 設計原則 (Architecture)

### 1.1 モジュラーモノリス (Modular Monolith)
- **モジュール分離**: 各ドメイン（Auth, School, Job 等）は論理的に分離し、疎結合を保つ。
- **依存関係のルール**:
    - モジュール間の直接的な参照（他モジュールの内部クラスの利用）は禁止。
    - 他モジュールのデータが必要な場合は、`Domain` 層で定義された `Public Query Port`（インターフェース）経由で取得する。
- **データアクセス**: 原則として、各モジュールは自身の責務外のテーブルを直接更新してはならない。

### 1.2 レイヤード構造
各プロジェクト（モジュール）は以下のレイヤーに役割を分担する。

| レイヤー | プロジェクト例 | 役割 |
| :--- | :--- | :--- |
| **Presentation** | `SenLink.Api` | エンドポイント定義、リクエストバリデーション、BFF 集約ロジック |
| **Application** | `SenLink.Service` | ユースケースの実装、ドメイン間のオーケストレーション |
| **Domain** | `SenLink.Domain` | エンティティ、値オブジェクト、ドメインサービス、Port (インターフェース) |
| **Infrastructure** | `SenLink.Infrastructure` | DB 永続化 (EF Core)、外部メッセージング (RabbitMQ)、Port の実装 |
| **Shared** | `SenLink.Shared` | 共通の例外定義、定数、DTO ユーティリティ |

### 1.3 BFF (Backend For Frontend)
- **集約の責務**: 画面表示に複数のサービスデータが必要な場合、`BFF` モジュールで集約を行う。
- **通信**: BFF から他モジュールの呼び出しは、HTTP 通信ではなく、同一プロセス内での Port 経由の呼び出しとする。

---

## 2. バックエンド実装 (C# / .NET)

### 2.1 命名規則
- **クラス/メソッド**: `PascalCase`
- **変数/引数**: `camelCase`
- **非同期メソッド**: 末尾に `Async` を付与。
- **インターフェース**: `I` プレフィックスを付与 (例: `IJobRepository`)。

### 2.2 非同期処理
- 原則として `async / await` を使用し、ブロッキングコール (`.Result`, `.Wait()`) は禁止。
- キャンセル操作を伝播させるため、可能な限り `CancellationToken` を引数に含める。

### 2.3 エラーハンドリング
- **グローバル例外ハンドラ**: 個別の `try-catch` によるレスポンス整形は避け、`GlobalExceptionHandler` で一括制御する。
- **ビジネス例外**: ドメイン固有のエラーは `Shared` プロジェクトで定義したカスタム例外をスローする。
- **レスポンス**: 常に `ApiResponse<T>` ラッパーを使用し、統一された JSON 形式で返す。

### 2.4 データベース (EF Core)
- **BaseEntity**: 全てのエンティティは `BaseEntity` を継承し、`CreatedAt`, `UpdatedAt` を保持する。
- **ID 生成**: DB 側の `GENERATED ALWAYS AS IDENTITY` を利用する。
- **マイグレーション**: スクリプト (`scripts/*.sh`) を使用して反映・管理する。

---

## 3. API 仕様

### 3.1 命名・形式
- **URL パス**: `kebab-case` (例: `/api/v1/student-profiles`)
- **JSON キー**: `lowerCamelCase`
- **日時**: ISO8601 形式、UTC 基準で返却。

### 3.2 認証・認可
- **認証**: `HttpOnly Cookie` + `JWT`。
- **認可**: `[Authorize(Roles = "...")]` 属性を用いて、ロール (Student, Teacher, Admin) ベースの制御を行う。

---

## 4. テストと品質 (Quality)

### 4.1 テストコード
- **TDD の実践**: 複雑なロジックを実装する際は、テストを先に記述することを推奨。
- **カバレッジ**: 重要なビジネスロジックが含まれる `Service` および `Domain` レイヤーのカバレッジ **80% 以上**を目標とする。
- **xUnit**: テストフレームワークとして `xUnit` を、モックとして `Moq` または `NSubstitute` を使用する。

### 4.2 品質指標
- **循環的複雑度**: 1 メソッドにつき **10 以下** を維持する。
- **静的解析**: `dotnet format` を実行し、Lint エラーがない状態でコミットする。

---

## 5. フロントエンド実装 (Next.js)

### 5.1 スタイリング
- **Vanilla CSS**: 柔軟性と学習コストの観点から標準の CSS (CSS Modules) を優先する。
- **shadcn/ui**: UI コンポーネントの基盤として利用する。

### 5.2 状態管理
- `React Context` または `TanStack Query` (SWR) を利用し、不要なグローバルストアの肥大化を避ける。
