# API設計：Auditサービス

## 補足（今回DB表を載せた理由）
前回までの「各サービスAPI設計書」は **“外部IF（HTTP API）”を中心**に書いていたため、DB表は別資料（MODEL）に寄せる方針でした。  
一方、Auditは **運用・監視**が主目的で、APIだけを見ると「どの粒度で/どの項目が取れるべきか」が読み取りづらくなりやすいため、**設計の前提（保持データの種類）**を示す目的で一度載せました。

ただし、方針の揺れは避けたいので、**Auth/他サービスに揃えて**この設計書からは **DB表の章を削除**し、API定義に必要な項目だけをレスポンスモデルに落とし込みます（以降も同様）。

---

# API設計：Auditサービス

## 概要
Auditサービスは、SenLink の監査ログ（操作ログ）・エラーログ・システム状態（メトリクス）を提供する。  
- 認証方式：Cookie（HttpOnly） + JWT  
- ロール：0=学生 / 1=教員 / 2=管理者  
- セキュリティ前提：学内IP制限（学外は 403）  
- 方針：
  - 監査ログ：重要操作の証跡（検索・閲覧は管理者のみ）
  - エラーログ：障害解析（検索・閲覧は管理者のみ）
  - メトリクス：運用ダッシュボード向けの状態表示（検索・閲覧は管理者のみ）
  - 書き込みは基本的にサービス/Workerが内部で行い、外部公開APIとしての作成は持たない（改ざん・乱用対策）

---

## API一覧（Audit）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 管理者：監査ログ一覧 | /api/v1/audit/admin/audit-logs | GET | 必要 | 管理者 | 監査ログを検索・ページング取得 |
| 2 | 管理者：監査ログ詳細 | /api/v1/audit/admin/audit-logs/{audit_log_id} | GET | 必要 | 管理者 | 監査ログ詳細（details含む） |
| 3 | 管理者：エラーログ一覧 | /api/v1/audit/admin/error-logs | GET | 必要 | 管理者 | エラーログを検索・ページング取得 |
| 4 | 管理者：エラーログ詳細 | /api/v1/audit/admin/error-logs/{error_log_id} | GET | 必要 | 管理者 | エラーログ詳細（stackTrace含む） |
| 5 | 管理者：メトリクス履歴 | /api/v1/audit/admin/system-metrics | GET | 必要 | 管理者 | メトリクス履歴を範囲指定で取得 |
| 6 | 管理者：最新メトリクス（サマリ） | /api/v1/audit/admin/system-metrics/latest | GET | 必要 | 管理者 | componentごとの最新状態 |
| 7 | 管理者：運用ダッシュボード | /api/v1/audit/admin/dashboard | GET | 必要 | 管理者 | 監視/運用の集約ビューを返す（管理者画面用） |

---

## 1. 管理者：監査ログ一覧

パス: `/api/v1/audit/admin/audit-logs`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 監査ログを検索・ページングして返す（運用監査向け）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `actorId` (任意): BIGINT
- `targetTable` (任意): string（例: `requests`）
- `targetId` (任意): BIGINT
- `method` (任意): string（例: `CREATE|UPDATE|DELETE|APPROVE`）
- `from` (任意): string（ISO8601, createdAt下限）
- `to` (任意): string（ISO8601, createdAt上限）
- `ip` (任意): string（部分一致可）
- `sort` (任意, default=`createdAt:desc`): `createdAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=50)

レスポンスモデル: `ApiResponse[Paged[AuditLogRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "auditLogId": 30001,
        "actorId": 2001,
        "targetTable": "requests",
        "targetId": 9001,
        "method": "APPROVE",
        "ipAddress": "192.168.1.10",
        "createdAt": "2026-01-16T10:25:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "total": 120
  },
  "operation": "audit_admin_audit_log_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "audit_admin_audit_log_list"
}
~~~

---

## 2. 管理者：監査ログ詳細

パス: `/api/v1/audit/admin/audit-logs/{audit_log_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 監査ログ詳細を返す（detailsを含む）。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `audit_log_id`: BIGINT

レスポンスモデル: `ApiResponse[AuditLogDetail]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "auditLogId": 30001,
    "actorId": 2001,
    "targetTable": "requests",
    "targetId": 9001,
    "method": "APPROVE",
    "details": {
      "before": { "status": 1 },
      "after": { "status": 2 },
      "comment": "承認しました"
    },
    "ipAddress": "192.168.1.10",
    "createdAt": "2026-01-16T10:25:00Z"
  },
  "operation": "audit_admin_audit_log_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Audit log not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "audit_admin_audit_log_get"
}
~~~

---

## 3. 管理者：エラーログ一覧

パス: `/api/v1/audit/admin/error-logs`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: エラーログを検索・ページングして返す（障害解析向け）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `serviceName` (任意): string（例: `Job` / `Worker`）
- `severity` (任意): `0|2|3`
- `q` (任意): string（message 部分一致）
- `accountId` (任意): BIGINT
- `from` (任意): string（ISO8601, createdAt下限）
- `to` (任意): string（ISO8601, createdAt上限）
- `sort` (任意, default=`createdAt:desc`): `createdAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=50)

レスポンスモデル: `ApiResponse[Paged[ErrorLogRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "errorLogId": 40001,
        "serviceName": "Job",
        "severity": 2,
        "message": "Failed to create job",
        "requestUrl": "/api/v1/job/admin/jobs",
        "accountId": 2001,
        "createdAt": "2026-01-16T11:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "total": 35
  },
  "operation": "audit_admin_error_log_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "audit_admin_error_log_list"
}
~~~

---

## 4. 管理者：エラーログ詳細

パス: `/api/v1/audit/admin/error-logs/{error_log_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: エラーログ詳細を返す（stackTrace、requestParamsを含む）。  
※requestParams はPIIを含む可能性があるため、必要に応じてマスク適用。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `error_log_id`: BIGINT

レスポンスモデル: `ApiResponse[ErrorLogDetail]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "errorLogId": 40001,
    "serviceName": "Job",
    "severity": 2,
    "message": "Failed to create job",
    "stackTrace": "Traceback (most recent call last): ...",
    "requestUrl": "/api/v1/job/admin/jobs",
    "requestParams": {
      "title": "会社説明会",
      "deadline": "2026-02-01"
    },
    "accountId": 2001,
    "createdAt": "2026-01-16T11:00:00Z"
  },
  "operation": "audit_admin_error_log_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Error log not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "audit_admin_error_log_get"
}
~~~

---

## 5. 管理者：メトリクス履歴

パス: `/api/v1/audit/admin/system-metrics`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: メトリクス履歴を範囲指定で取得する（運用向け）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `component` (任意): string（例: `Apache|FastAPI|Worker|RabbitMQ`）
- `status` (任意): `0|1|2`
- `from` (任意): string（ISO8601, createdAt下限）
- `to` (任意): string（ISO8601, createdAt上限）
- `sort` (任意, default=`createdAt:desc`): `createdAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=100)

レスポンスモデル: `ApiResponse[Paged[SystemMetricRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "systemMetricId": 50001,
        "component": "FastAPI",
        "status": 1,
        "responseTime": 120,
        "cpuUsage": 23.5,
        "memUsage": 41.2,
        "diskUsage": 72.8,
        "createdAt": "2026-01-16T11:05:00Z"
      }
    ],
    "page": 1,
    "pageSize": 100,
    "total": 500
  },
  "operation": "audit_admin_system_metric_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "from", "reason": "invalid_format" }
    ]
  },
  "operation": "audit_admin_system_metric_list"
}
~~~

---

## 6. 管理者：最新メトリクス（サマリ）

パス: `/api/v1/audit/admin/system-metrics/latest`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 各componentの最新状態を返す（運用ダッシュボードの「現在状態」表示用）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `components` (任意): string（カンマ区切り。未指定なら全component）

レスポンスモデル: `ApiResponse[SystemMetricsLatest]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "generatedAt": "2026-01-16T11:10:00Z",
    "items": [
      {
        "component": "Apache",
        "status": 1,
        "responseTime": 15,
        "cpuUsage": 5.1,
        "memUsage": 12.3,
        "diskUsage": 70.0,
        "createdAt": "2026-01-16T11:09:30Z"
      },
      {
        "component": "RabbitMQ",
        "status": 2,
        "responseTime": null,
        "cpuUsage": 65.2,
        "memUsage": 78.0,
        "diskUsage": 70.0,
        "createdAt": "2026-01-16T11:09:00Z"
      }
    ]
  },
  "operation": "audit_admin_system_metric_latest"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "audit_admin_system_metric_latest"
}
~~~

---

## 7. 管理者：運用ダッシュボード（集約）

パス: `/api/v1/audit/admin/dashboard`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 管理者用ダッシュボード表示に必要な情報を「1回の取得」で返す（運用/監視向けの集約ビュー）。  
※本APIは「管理者UI」用であり、BFFではなく Audit サービスの admin API として提供する。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `rangeHours` (任意, default=24): 集計期間（例: 1/6/24/72）
- `errorLimit` (任意, default=10): 直近エラーの最大件数
- `auditLimit` (任意, default=10): 直近監査ログの最大件数

レスポンスモデル: `ApiResponse[AdminOpsDashboard]`

ダッシュボードに含める情報（設計意図）:
- `systemHealth`: component別の最新ステータス（Down/HighLoadを即検知）
- `trafficHints`: APIの傾向（平均応答/エラー率のヒント）※厳密なAPMではなく「運用の目安」
- `queueHints`: RabbitMQの状態（滞留、失敗などのヒント）※収集できる範囲で
- `recentErrors`: 直近の重要エラー（severity=2/3中心）
- `recentAuditLogs`: 直近の重要操作（承認/権限変更/公開範囲変更など）
- `topProblemEndpoints`: 期間内に多発した requestUrl のランキング（障害箇所の当たり）
- `drilldowns`: 詳細画面へのAPI（一覧へ飛ぶ）

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "filters": {
      "rangeHours": 24,
      "errorLimit": 10,
      "auditLimit": 10
    },
    "systemHealth": {
      "items": [
        {
          "component": "Apache",
          "status": 1,
          "responseTime": 15,
          "cpuUsage": 5.1,
          "memUsage": 12.3,
          "diskUsage": 70.0,
          "updatedAt": "2026-01-16T11:09:30Z"
        },
        {
          "component": "FastAPI",
          "status": 1,
          "responseTime": 120,
          "cpuUsage": 23.5,
          "memUsage": 41.2,
          "diskUsage": 72.8,
          "updatedAt": "2026-01-16T11:09:00Z"
        },
        {
          "component": "RabbitMQ",
          "status": 2,
          "responseTime": null,
          "cpuUsage": 65.2,
          "memUsage": 78.0,
          "diskUsage": 70.0,
          "updatedAt": "2026-01-16T11:08:30Z"
        }
      ],
      "summary": {
        "healthy": 2,
        "highLoad": 1,
        "down": 0
      }
    },
    "trafficHints": {
      "avgApiResponseTimeMs": 180,
      "errorRate": 0.012,
      "generatedAt": "2026-01-16T11:10:00Z",
      "notes": [
        "ヒント値（厳密なAPMではなく運用の目安）"
      ]
    },
    "queueHints": {
      "backlogCount": 120,
      "oldestMessageAgeSeconds": 540,
      "failedJobsLastHour": 3
    },
    "recentErrors": [
      {
        "errorLogId": 40011,
        "serviceName": "Worker",
        "severity": 3,
        "message": "LINE delivery failed: RATE_LIMIT",
        "requestUrl": null,
        "accountId": null,
        "createdAt": "2026-01-16T10:58:00Z"
      }
    ],
    "recentAuditLogs": [
      {
        "auditLogId": 30009,
        "actorId": 2001,
        "targetTable": "accounts",
        "targetId": 2100,
        "method": "ROLE_CHANGE",
        "ipAddress": "192.168.1.10",
        "createdAt": "2026-01-16T10:40:00Z"
      }
    ],
    "topProblemEndpoints": [
      { "requestUrl": "/api/v1/job/admin/jobs", "count": 12, "severityMax": 2 },
      { "requestUrl": "/api/v1/request/teacher/requests/{id}/approve", "count": 6, "severityMax": 3 }
    ],
    "drilldowns": {
      "endpoints": {
        "audit_logs": "/api/v1/audit/admin/audit-logs?from=2026-01-15T11:10:00Z&to=2026-01-16T11:10:00Z",
        "error_logs": "/api/v1/audit/admin/error-logs?from=2026-01-15T11:10:00Z&to=2026-01-16T11:10:00Z",
        "system_metrics": "/api/v1/audit/admin/system-metrics?from=2026-01-15T11:10:00Z&to=2026-01-16T11:10:00Z"
      }
    },
    "warnings": [],
    "meta": {
      "generatedAt": "2026-01-16T11:10:00Z",
      "ttlSeconds": 15
    }
  },
  "operation": "audit_admin_dashboard"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "audit_admin_dashboard"
}
~~~
