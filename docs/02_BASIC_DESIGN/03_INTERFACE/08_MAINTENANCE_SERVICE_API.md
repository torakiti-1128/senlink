# API設計：Maintenanceサービス

## 概要

Maintenanceサービスは、SenLink の **システム設定（system_settings）** を管理する。  
- 認証方式：Cookie（HttpOnly） + JWT  
- ロール：0=学生 / 1=教員 / 2=管理者  
- セキュリティ前提：学内IP制限（学外は 403）  
- 方針：
  - 設定値は **system_settings に即時反映**する（更新APIはDB更新のみ）
  - 設定変更後の「バックエンドの再取得（メモリ内設定オブジェクトの再構築）」は **RabbitMQ 経由で Worker が実行**する
  - フロントへの「再取得通知」も **Worker が実行**する（通知方式は別設計）
  - 変更内容の監査ログ・エラーログは **Auditサービスの責任**（Maintenanceはログを持たない）
  - フロントは「機密設定（is_sensitive=true）」を受け取らない

---

## API一覧（Maintenance）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 設定一覧（フロント用） | /api/v1/maintenance/settings/public | GET | 必要 | ALL | 機密を除いた設定一覧を返す（フロントの設定再取得に使用） |
| 2 | 設定一覧（管理者用） | /api/v1/maintenance/admin/settings | GET | 必要 | 管理者 | 全設定（機密含む）を返す |
| 3 | 設定更新（単体） | /api/v1/maintenance/admin/settings/{key} | PUT | 必要 | 管理者 | 指定keyの設定値を更新（DB更新のみ + Workerへenqueue） |
| 4 | 設定更新（複数） | /api/v1/maintenance/admin/settings | PUT | 必要 | 管理者 | 複数keyを一括更新（DB更新のみ + Workerへenqueue） |

---

## 1. 設定一覧（フロント用）

パス: `/api/v1/maintenance/settings/public`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: フロントが必要とする設定値を取得する。`is_sensitive=true` は返さない。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `keys` (任意): string（カンマ区切り）例：`otp_digits,session_ttl_minutes`
  - 未指定なら「公開設定を全件返す」
- `format` (任意, default=`typed`): `typed|raw`
  - typed：value_type に基づいて型変換した `typedValue` を返す
  - raw：文字列の value のみ返す（軽量）

レスポンスモデル: `ApiResponse[PublicSystemSettings]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "key": "otp_digits",
        "valueType": 1,
        "typedValue": 6,
        "description": "OTPの桁数",
        "changeCounts": 3
      },
      {
        "key": "session_ttl_minutes",
        "valueType": 1,
        "typedValue": 30,
        "description": "アクセストークンの有効期限（分）",
        "changeCounts": 5
      }
    ]
  },
  "operation": "maintenance_settings_public_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

---

## 2. 設定一覧（管理者用）

パス: `/api/v1/maintenance/admin/settings`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 管理者が全設定（機密含む）を取得する。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `q` (任意): string（key/description 部分一致）
- `isSensitive` (任意): `true|false|all`（default=all）
- `sort` (任意, default=`key:asc`): `key:asc|desc`, `changeCounts:desc`
- `page` (任意, default=1)
- `pageSize` (任意, default=50)

レスポンスモデル: `ApiResponse[Paged[SystemSettingRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "id": 101,
        "key": "jwt_secret",
        "value": "***",
        "valueType": 0,
        "description": "JWT署名キー",
        "isSensitive": true,
        "changeCounts": 1
      }
    ],
    "page": 1,
    "pageSize": 50,
    "total": 1
  },
  "operation": "maintenance_admin_settings_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

---

## 3. 設定更新（単体）

パス: `/api/v1/maintenance/admin/settings/{key}`  
メソッド: `PUT`  
認証: 必要  
対象ロール: 管理者  
説明: 指定keyの設定を更新する。  
- APIの責務は **DB更新のみ**  
- 更新成功後、Workerへ「設定再取得ジョブ」を enqueue（失敗時の詳細ログはAudit）

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `key`: string

リクエストモデル: `UpdateSystemSettingRequest`

~~~json
{
  "value": "45",
  "valueType": 1,
  "description": "アクセストークンの有効期限（分）",
  "isSensitive": false
}
~~~

レスポンスモデル: `ApiResponse[SystemSettingUpdated]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Setting updated",
  "data": {
    "key": "session_ttl_minutes",
    "changeCounts": 6
  },
  "operation": "maintenance_admin_setting_update"
}
~~~

エラーレスポンス:
- 400: Bad Request（形式不正）
- 401: Unauthorized
- 403: Forbidden（管理者権限なし / 学内IP外）
- 404: Not Found（対象keyなし）
- 422: Unprocessable Entity（value_typeと値の整合性不正）
- 409: Conflict（運用上ロック/更新不可キー等がある場合）
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "value", "reason": "type_mismatch_with_valueType" }
    ]
  },
  "operation": "maintenance_admin_setting_update"
}
~~~

---

## 4. 設定更新（複数）

パス: `/api/v1/maintenance/admin/settings`  
メソッド: `PUT`  
認証: 必要  
対象ロール: 管理者  
説明: 複数keyを一括更新する。  
- DB更新はトランザクションで行う（全成功 or 全失敗）
- 成功後、Workerへ enqueue（keysをまとめて通知）

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `BulkUpdateSystemSettingsRequest`

~~~json
{
  "items": [
    { "key": "otp_digits", "value": "6", "valueType": 1 },
    { "key": "session_ttl_minutes", "value": "30", "valueType": 1 }
  ]
}
~~~

レスポンスモデル: `ApiResponse[BulkUpdateSystemSettingsResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Settings updated",
  "data": {
    "requested": 2,
    "updated": 2,
    "keys": ["otp_digits", "session_ttl_minutes"]
  },
  "operation": "maintenance_admin_settings_bulk_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500
