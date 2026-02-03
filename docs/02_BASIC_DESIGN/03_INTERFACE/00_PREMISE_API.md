# API設計：共通仕様

SenLink の API 設計における共通認識（認証方式、セキュリティ、レスポンス形式、命名規約など）を定義する。  
各サービスの API 設計書は本書を参照し、重複記述を避ける。

---

## 1. API基本方針

### 1.1 バージョニング
- Base Path: `/api/v1`
- 破壊的変更は `/api/v2` で提供する

### 1.2 データ形式
- Request/Response: `application/json` を基本とする
- 添付ファイルは `multipart/form-data` を使用する（Request添付用）

### 1.3 命名規約
- URL：kebab-case（パスは `/` 区切り）
  - 例：`/api/v1/end-point`
- JSON Key：`lowerCamelCase`
  - 例：`accountId`, `submittedAt`

---

## 2. 認証・認可（Cookie(HttpOnly) + JWT）

### 2.1 認証方式
- 認証トークンは **JWT** を採用する
- JWT は **HttpOnly Cookie** に格納し、ブラウザが自動送信する方式とする
- クライアントは原則 `Authorization: Bearer ...` を付与しない（例外は別クライアント対応時）

### 2.2 Cookie仕様（推奨）
- Cookie名：`access_token`
- 属性：
  - `HttpOnly=true`
  - `Secure=true`（HTTPS必須）
  - `SameSite=Lax`（同一オリジン前提。必要に応じて Strict を検討）
  - `Path=/`

### 2.3 認可（RBAC）
- `accounts.role` によるロールベース制御
  - 0: 学生 / 1: 教員 / 2: 管理者
- 画面・APIのアクセス制御は以下を原則とする
  - 学生：自分の情報のみ参照/更新可（他学生は不可）
  - 教員：担当クラスの学生のみ参照可
  - 管理者：全学生参照可 + 権限管理可

---

## 3. 学内IP制限（アクセス制御）

### 3.1 方針
- **学内のプライベートIP** のみアクセス許可
- 将来的な VPN 対応を見据え、許可IP帯は `system_settings` で変更可能な設計にする

### 3.2 エラー時の扱い
- 学内IP外からのアクセスは `403 Forbidden` を返す

---

## 4. CSRF / XSS / セッション

### 4.1 CSRF（Cookie運用のため）
- 基本：`SameSite=Lax` を利用
- 状態変更系（POST/PUT/PATCH/DELETE）は将来 `CSRFトークン` 導入が可能な設計にしておく
  - 初期は同一オリジン運用を前提に、導入コストを抑える

### 4.2 XSS
- JWT を HttpOnly Cookie に格納し、JSから参照不可
- 画面側は入力値のサニタイズ、HTMLエスケープを徹底

### 4.3 セッション（JWT）
- JWT の有効期限は基本30分に設定
- リフレッシュトークン導入は後続フェーズで検討（必要になったタイミングで追加）
  - 現フェーズは refresh を前提にしない（refresh API は未実装/無効化でもよい）

---

## 5. レスポンス形式（共通ラッパー）

### 5.1 成功レスポンス
- 原則として以下の共通形式を用いる

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {},
  "operation": "operation_name"
}
```

- `code` は HTTP ステータスと一致させる
- `operation` はログ解析/監査ログの追跡用に一意な識別子を付ける

### 5.2 エラーレスポンス
- 原則として以下の共通形式を用いる

```json
{
  "success": false,
  "code": 400,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "email", "reason": "invalid_format" }
    ]
  },
  "operation": "operation_name"
}
```

---

## 6. HTTPステータス運用ルール（共通）
- 200: 取得成功
- 201: 作成成功
- 204: 成功（返却ボディなし）
- 400: 不正なリクエスト（形式/前提違反）
- 401: 未認証（ログイン切れ/トークン無効）
- 403: 禁止（権限なし、学内IP外）
- 404: 対象なし
- 409: 競合（既登録、重複など）
- 422: バリデーション不正（型や項目要件）
- 429: レート制限
- 500: サーバ内部エラー
- 501: 未実装（後続フェーズ予定のAPIを無効化する場合）

---

## 7. ロギング・監査（Audit）

### 7.1 監査ログ（audit_logs）
- 重要操作（申請の承認/差し戻し、求人公開範囲変更、権限変更など）は監査ログ対象
- `actor_id`, `target_table`, `target_id`, `method`, `details` を保存する

### 7.2 エラーログ（error_logs）
- 例外発生時は severity と stack_trace を保存する（PIIは最小化）

---

## 8. 入力バリデーション（共通）

### 8.1 メール（学校ドメイン制限）
- 学校配布ドメインのみ許可（例：`@school.ac.jp`）
- 学生：学籍番号7桁形式（例：`1234567@school.ac.jp`）
  - 正規表現例：`^\d{7}@school\.ac\.jp$`
- 教員：`myouzi_namae@school.ac.jp` 形式（厳密度は別途）
  - 正規表現例（最低限）：`^[a-z]+_[a-z]+@school\.ac\.jp$`

### 8.2 パスワード
- 最低文字数（例：8文字以上）
- 文字種（例：英字+数字を推奨）
- ポリシーは `system_settings` で変更可能

---

## 9. ページング・ソート・検索（共通）
- 取得系のリストAPIは以下を原則サポートする
  - `page`（1始まり）
  - `pageSize`
  - `sort`（例：`createdAt:desc`）

- レスポンス例（ページング）

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 20,
    "total": 100
  },
  "operation": "list_xxx"
}
```

---

## 10. 日時・タイムゾーン
- DBは `TIMESTAMP` で保存
- API返却は ISO8601（例：`2026-01-16T12:34:56Z`）
- タイムゾーン運用は実装側で統一する（推奨：APIはUTC）

---
