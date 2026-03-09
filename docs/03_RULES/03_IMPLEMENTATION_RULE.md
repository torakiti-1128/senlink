# 実装規約 (Implementation Rules)

本プロジェクトにおける、AIおよび開発者が遵守すべき実装の詳細ルール。

## 1. ディレクトリ・ファイル構成

### モジュール別 DTO の分割
Service層の DTO は、1ファイルにまとめず、役割ごとに分割して管理する。
- `{ModuleName}Requests.cs`: リクエスト用 record (Create/Update等)
- `{ModuleName}Response.cs`: レスポンス用 record
- 場所: `SenLink.Service/Modules/{ModuleName}/DTOs/`

### バリデーターの配置
- `FluentValidation` を使用し、Service層に配置する。
- 場所: `SenLink.Service/Modules/{ModuleName}/Validators/`

## 2. 認証・認可 (Auth & Role)

### コントローラーでの取得
- 現在のユーザーID (`AccountId`) は、`X-Account-Id` 等の独自ヘッダーではなく、必ず JWT クレーム (`ClaimTypes.NameIdentifier`) から取得すること。
- コントローラーには `[Authorize]` を付与し、必要に応じて `[Authorize(Roles = "Student")]` 等で制限する。

### テスト環境での認証
- 結合テストでは `TestAuthHandler` を使用し、認可ミドルウェアをバイパスせずにクレームを注入して検証すること。

## 3. ログと例外

### 監査ログ (Audit Log)
- **原則**: `AuditInterceptor` (Infrastructure層) による自動記録を利用する。
- サービス層で手動の `publishEndpoint.Publish(new AuditLogCreatedEvent(...))` は、特別なビジネス要件がない限り記述しない（二重ログ防止）。

### 例外ハンドリング
- クライアントエラー (400系) は `GlobalExceptionHandler` で `LogWarning` として出力し、ノイズを抑える。
- レスポンスは必ず `ApiErrorResponse` 形式に統一する。

## 4. 技術スタック特有の注意点

### MassTransit (9.0+)
- 開発・テスト環境での起動失敗を防ぐため、必要に応じて環境変数 `MT_LICENSE=CommunityLicense` を設定するか、単体テストでは `AddMassTransitTestHarness` を活用すること。
