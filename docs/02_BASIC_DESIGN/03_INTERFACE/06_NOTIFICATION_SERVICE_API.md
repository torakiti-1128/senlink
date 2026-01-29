# API設計：Notificationサービス

## 概要

Notificationサービスは、SenLink の通知（通知センター表示／既読管理／配信ジョブ）を提供する。  
- 認証方式：Cookie（HttpOnly） + JWT  
- ロール：0=学生 / 1=教員 / 2=管理者  
- セキュリティ前提：学内IP制限（学外は 403）  
- 方針：
  - 通知センター表示のための「通知レコード」をDBに保持する（未読/既読を管理）
  - 配信（メール等）は Worker（非同期）で処理する（APIは基本 enqueue）
  - 画面は「自分宛通知のみ取得・更新可」が原則
  - 一括通知（教員→学生）は School の所属・権限判定が前提（Notificationは受信レコード生成を担う）

### ドメイン定義（共通）
- notification.type（例）：
  - 0: SYSTEM（システム）
  - 1: REQUEST（申請）
  - 2: JOB（求人）
  - 3: ACTIVITY（活動）
  - 4: REMINDER（督促）
  - 5: RECOMMEND（レコメンド）
  - 9: OTHER（その他）
- notification.channel（例）：
  - 0: IN_APP（通知センター）
  - 1: EMAIL（メール）
  - 2: LINE
- notification.read_status：
  - 0: 未読
  - 1: 既読

---

## API一覧（Notification）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 通知一覧（通知センター） | /api/v1/notification/notifications | GET | 必要 | ALL | 自分宛の通知を検索・ページング取得 |
| 2 | 通知詳細 | /api/v1/notification/notifications/{notification_id} | GET | 必要 | ALL | 自分宛通知の詳細（body/link等）を取得 |
| 3 | 既読更新（単体） | /api/v1/notification/notifications/{notification_id}/read | PATCH | 必要 | ALL | 指定通知を既読にする |
| 4 | 既読更新（複数） | /api/v1/notification/notifications/read | PATCH | 必要 | ALL | 複数通知をまとめて既読にする |
| 5 | 未読件数 | /api/v1/notification/notifications/unread-count | GET | 必要 | ALL | 未読件数を返す（バッジ用） |
| 6 | 教員：学生一括通知（enqueue） | /api/v1/notification/teacher/bulk | POST | 必要 | 教員/管理者 | 対象学生へ通知を作成し配信キューへ |
| 7 | 管理者：通知テンプレ一覧 | /api/v1/notification/admin/templates | GET | 必要 | 管理者 | テンプレ一覧取得（運用） |
| 8 | 管理者：通知テンプレ作成 | /api/v1/notification/admin/templates | POST | 必要 | 管理者 | テンプレ作成 |
| 9 | 管理者：通知テンプレ更新 | /api/v1/notification/admin/templates/{template_id} | PUT | 必要 | 管理者 | テンプレ更新 |
| 10 | 管理者：通知テンプレ削除 | /api/v1/notification/admin/templates/{template_id} | DELETE | 必要 | 管理者 | テンプレ削除 |

---

## 1. 通知一覧（通知センター）

パス: `/api/v1/notification/notifications`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 自分宛の通知を検索・ページングして返す（通知センター用）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `type` (任意): `0|1|2|3|4|5`
- `readStatus` (任意): `unread|read|all`（default=all）
- `from` (任意): string（ISO8601, createdAt下限）
- `to` (任意): string（ISO8601, createdAt上限）
- `q` (任意): string（title/body 部分一致）
- `sort` (任意, default=`createdAt:desc`): `createdAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=20)

レスポンスモデル: `ApiResponse[Paged[NotificationRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "notificationId": 50001,
        "type": 1,
        "title": "申請が承認されました",
        "summary": "面談予約申請が承認されました。",
        "readStatus": "unread",
        "createdAt": "2026-01-16T10:25:00Z",
        "link": "/student/requests/9001"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 8
  },
  "operation": "notification_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": { "type": "AUTH_REQUIRED", "details": [] },
  "operation": "notification_list"
}
~~~

---

## 2. 通知詳細

パス: `/api/v1/notification/notifications/{notification_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 自分宛通知の詳細を返す（本文、追加メタ、リンクなど）。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `notification_id`: BIGINT

レスポンスモデル: `ApiResponse[NotificationDetail]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "notificationId": 50001,
    "recipientAccountId": 1001,
    "type": 1,
    "channel": "in_app",
    "title": "申請が承認されました",
    "body": "面談予約申請が承認されました。日時をご確認ください。",
    "readStatus": "unread",
    "createdAt": "2026-01-16T10:25:00Z",
    "readAt": null,
    "link": "/student/requests/9001",
    "metadata": {
      "requestId": 9001,
      "requestType": 1,
      "requestStatus": 2
    }
  },
  "operation": "notification_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500  
※他人の通知は 404 を返す（情報漏洩防止）

~~~json
{
  "success": false,
  "code": 404,
  "message": "Notification not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "notification_get"
}
~~~

---

## 3. 既読更新（単体）

パス: `/api/v1/notification/notifications/{notification_id}/read`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: ALL  
説明: 指定通知を既読にする。既読済みの場合も 200 を返す（冪等）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[MarkReadResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Marked as read",
  "data": {
    "notificationId": 50001,
    "readStatus": "read",
    "readAt": "2026-01-16T10:30:00Z"
  },
  "operation": "notification_mark_read"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Notification not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "notification_mark_read"
}
~~~

---

## 4. 既読更新（複数）

パス: `/api/v1/notification/notifications/read`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: ALL  
説明: 複数通知をまとめて既読にする。対象は「自分宛」のみに限定する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `MarkNotificationsReadRequest`

~~~json
{
  "notificationIds": [50001, 50002, 50003]
}
~~~

レスポンスモデル: `ApiResponse[MarkNotificationsReadResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Marked as read",
  "data": {
    "requested": 3,
    "updated": 3,
    "skipped": 0,
    "readAt": "2026-01-16T10:31:00Z"
  },
  "operation": "notification_mark_read_bulk"
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
      { "field": "notificationIds", "reason": "too_many" }
    ]
  },
  "operation": "notification_mark_read_bulk"
}
~~~

---

## 5. 未読件数

パス: `/api/v1/notification/notifications/unread-count`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 未読件数を返す（ヘッダのバッジ用）。  
※リアルタイム性が必要なため、キャッシュが入る場合でも数十秒程度のTTLに留める。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[UnreadCountResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "unreadCount": 5,
    "generatedAt": "2026-01-16T10:32:00Z"
  },
  "operation": "notification_unread_count"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "notification_unread_count"
}
~~~

---

## 6. 教員：学生一括通知（enqueue）

パス: `/api/v1/notification/teacher/bulk`  
メソッド: `POST`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員が選択学生へ通知を送る。  
- APIは「通知レコード作成 + 配信ジョブenqueue」まで  
- 実際のメール送信は Worker が処理  
- 対象学生の妥当性（担当クラス/管理者権限）は必ず検証する

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `TeacherBulkNotifyRequest`

~~~json
{
  "target": {
    "classId": 101,
    "studentAccountIds": [1001, 1002, 1003]
  },
  "notification": {
    "type": 0,
    "priority": "normal",
    "title": "締切のリマインド",
    "body": "求人応募の締切が近いです。応募状況を確認してください。",
    "link": "/student/jobs"
  },
  "channels": ["in_app", "email"]
}
~~~

レスポンスモデル: `ApiResponse[TeacherBulkNotifyResult]`

~~~json
{
  "success": true,
  "code": 202,
  "message": "Enqueued",
  "data": {
    "requested": 3,
    "created": 3,
    "enqueued": 3,
    "jobId": "notifyjob_20260116_0001",
    "acceptedAt": "2026-01-16T10:35:00Z"
  },
  "operation": "notification_teacher_bulk"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（教員権限なし / 学内IP外 / 担当外学生）
- 404: Not Found（クラス不明など）
- 422: Unprocessable Entity（本文長・件名空など）
- 429: Too Many Requests（連打対策）
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "studentAccountIds", "reason": "not_assigned_scope" }
    ]
  },
  "operation": "notification_teacher_bulk"
}
~~~

---

## 7. 管理者：通知テンプレ一覧

パス: `/api/v1/notification/admin/templates`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 通知テンプレ（件名/本文/差し込み変数）を取得する。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `q` (任意): string（name/title部分一致）
- `sort` (任意, default=`updatedAt:desc`)
- `page` (任意, default=1)
- `pageSize` (任意, default=50)

レスポンスモデル: `ApiResponse[Paged[NotificationTemplateRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "templateId": 70001,
        "name": "reminder_deadline",
        "type": 4,
        "titleTemplate": "【リマインド】{{jobTitle}} の締切が近いです",
        "updatedAt": "2026-01-10T03:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "total": 1
  },
  "operation": "notification_admin_template_list"
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
  "operation": "notification_admin_template_list"
}
~~~

---

## 8. 管理者：通知テンプレ作成

パス: `/api/v1/notification/admin/templates`  
メソッド: `POST`  
認証: 必要  
対象ロール: 管理者  
説明: テンプレを作成する（nameは一意）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateNotificationTemplateRequest`

~~~json
{
  "name": "request_approved",
  "type": 1,
  "titleTemplate": "申請が承認されました",
  "bodyTemplate": "申請（{{requestTitle}}）が承認されました。詳細を確認してください。",
  "placeholders": ["requestTitle"],
  "defaultLink": "/student/requests/{{requestId}}"
}
~~~

レスポンスモデル: `ApiResponse[NotificationTemplateInDB]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Template created",
  "data": {
    "templateId": 70002,
    "name": "request_approved",
    "type": 1,
    "titleTemplate": "申請が承認されました",
    "bodyTemplate": "申請（{{requestTitle}}）が承認されました。詳細を確認してください。",
    "placeholders": ["requestTitle"],
    "defaultLink": "/student/requests/{{requestId}}",
    "createdAt": "2026-01-16T10:40:00Z",
    "updatedAt": "2026-01-16T10:40:00Z"
  },
  "operation": "notification_admin_template_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 409 / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Template name already exists",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "name", "reason": "duplicate" }
    ]
  },
  "operation": "notification_admin_template_create"
}
~~~

---

## 9. 管理者：通知テンプレ更新

パス: `/api/v1/notification/admin/templates/{template_id}`  
メソッド: `PUT`  
認証: 必要  
対象ロール: 管理者  
説明: テンプレを更新する。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `template_id`: BIGINT

リクエストモデル: `UpdateNotificationTemplateRequest`

~~~json
{
  "titleTemplate": "【承認】申請が承認されました",
  "bodyTemplate": "申請（{{requestTitle}}）が承認されました。確認してください。",
  "placeholders": ["requestTitle"],
  "defaultLink": "/student/requests/{{requestId}}"
}
~~~

レスポンスモデル: `ApiResponse[NotificationTemplateInDB]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Template updated",
  "data": {
    "templateId": 70002,
    "name": "request_approved",
    "type": 1,
    "titleTemplate": "【承認】申請が承認されました",
    "bodyTemplate": "申請（{{requestTitle}}）が承認されました。確認してください。",
    "placeholders": ["requestTitle"],
    "defaultLink": "/student/requests/{{requestId}}",
    "createdAt": "2026-01-16T10:40:00Z",
    "updatedAt": "2026-01-16T10:45:00Z"
  },
  "operation": "notification_admin_template_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Template not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "notification_admin_template_update"
}
~~~

---

## 10. 管理者：通知テンプレ削除

パス: `/api/v1/notification/admin/templates/{template_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 管理者  
説明: テンプレを削除する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[DeleteTemplateResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Template deleted",
  "data": {
    "templateId": 70002,
    "deletedAt": "2026-01-16T10:50:00Z"
  },
  "operation": "notification_admin_template_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 500  
※テンプレが「運用上削除不可（使用中）」の場合は 409 を返す

~~~json
{
  "success": false,
  "code": 409,
  "message": "Template cannot be deleted",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "templateId", "reason": "in_use" }
    ]
  },
  "operation": "notification_admin_template_delete"
}
~~~
