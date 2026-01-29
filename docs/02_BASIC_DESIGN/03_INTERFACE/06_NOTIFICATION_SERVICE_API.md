# API設計：Notificationサービス

## 概要

Notificationサービスは、SenLink の通知（通知センター表示／既読管理／配信状態／LINE連携／受信設定）を提供する。  
- 認証方式：Cookie（HttpOnly） + JWT  
- ロール：0=学生 / 1=教員 / 2=管理者  
- セキュリティ前提：学内IP制限（学外は 403）  
- DB前提（確定スキーマ）：
  - notifications：通知の論理データ（通知センターの既読/未読はここ）
  - notification_deliveries：チャネル別送達状態（in-app/email/LINE の送達結果とリトライ）
  - account_line_links：LINE連携（送信に必要な line_user_id の紐付け）
  - notification_preferences：受信設定（in-app/email/LINE/全停止）

方針：
- 通知センターの既読/未読は `notifications.read_status` を更新する（冪等）
- 配信（メール/LINE）は Worker（非同期）で処理する（APIは基本 enqueue）
- 画面は「自分宛通知のみ取得・更新可」が原則
- 一括通知（教員→学生）は School の所属・権限判定が前提（Notificationは受信レコード生成と配信キュー投入を担う）
- LINE配信は「(1) LINE連携済み かつ (2) 受信設定で line_enabled=true かつ (3) mute_all=false」を満たす場合のみ対象

### ドメイン定義
- notifications.type：
  - 0: SYSTEM（システム）
  - 1: REQUEST（申請）
  - 2: JOB（求人）
  - 3: ACTIVITY（活動）
  - 4: REMINDER（督促）
  - 5: RECOMMEND（レコメンド）
  - 9: OTHER（その他）
- notifications.read_status：
  - 0: 未読
  - 1: 既読
- notification_deliveries.channel：
  - 0: IN_APP（通知センター）
  - 1: EMAIL（メール）
  - 2: LINE
- notification_deliveries.status：
  - 0: PENDING（送信前）
  - 1: SENT（送信済）
  - 2: FAILED（失敗）
- account_line_links.status：
  - 0: 未連携
  - 1: 連携済
  - 2: 解除

---

## API一覧（Notification）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 通知一覧（通知センター） | /api/v1/notification/notifications | GET | 必要 | ALL | 自分宛の通知を検索・ページング取得 |
| 2 | 通知詳細 | /api/v1/notification/notifications/{notification_id} | GET | 必要 | ALL | 自分宛通知の詳細（body/link等）を取得 |
| 3 | 既読更新（単体） | /api/v1/notification/notifications/{notification_id}/read | PATCH | 必要 | ALL | 指定通知を既読にする（冪等） |
| 4 | 既読更新（複数） | /api/v1/notification/notifications/read | PATCH | 必要 | ALL | 複数通知をまとめて既読にする（冪等） |
| 5 | 未読件数 | /api/v1/notification/notifications/unread-count | GET | 必要 | ALL | 未読件数を返す（バッジ用） |
| 6 | 送達状態一覧（自分宛） | /api/v1/notification/deliveries | GET | 必要 | ALL | 自分宛の送達状態を検索・ページング取得（チャネル別） |
| 7 | 送達状態一覧（通知単位） | /api/v1/notification/notifications/{notification_id}/deliveries | GET | 必要 | ALL | ある通知のチャネル別送達状態を取得 |
| 8 | 受信設定取得 | /api/v1/notification/preferences/me | GET | 必要 | ALL | 自分の受信設定を返す |
| 9 | 受信設定更新 | /api/v1/notification/preferences/me | PATCH | 必要 | ALL | 自分の受信設定を更新する |
| 10 | LINE連携状態取得 | /api/v1/notification/line/me | GET | 必要 | ALL | 自分のLINE連携状態（連携済み/解除など）を返す |
| 11 | LINE連携（登録/更新） | /api/v1/notification/line/me | POST | 必要 | ALL | LINE userId を登録して連携済みにする |
| 12 | LINE連携解除 | /api/v1/notification/line/me | DELETE | 必要 | ALL | LINE連携を解除する |
| 13 | 教員：学生一括通知（enqueue） | /api/v1/notification/teacher/bulk | POST | 必要 | 教員/管理者 | 対象学生へ通知を作成し配信キューへ（in-app/email/LINE） |
| 14 | 管理者：配信再試行（enqueue） | /api/v1/notification/admin/deliveries/{delivery_id}/retry | POST | 必要 | 管理者 | FAILED を再送キューへ（運用） |

> 重要：テンプレ管理APIは、現スキーマ（確定4テーブル）に存在しないため本サービス仕様から削除する。

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
- `type` (任意): `0|1|2|3|4|5|9`
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
        "linkUrl": "/student/requests/9001"
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
説明: 自分宛通知の詳細を返す（本文、リンクなど）。

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
    "title": "申請が承認されました",
    "body": "面談予約申請が承認されました。日時をご確認ください。",
    "linkUrl": "/student/requests/9001",
    "readStatus": "unread",
    "createdAt": "2026-01-16T10:25:00Z"
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
    "updatedAt": "2026-01-16T10:30:00Z"
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
説明: 複数通知をまとめて既読にする。対象は「自分宛」のみに限定する（冪等）。

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
    "updatedAt": "2026-01-16T10:31:00Z"
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
説明: 未読件数を返す（バッジ用）。  
※リアルタイム性が必要なため、キャッシュする場合でもTTLは短くする（数十秒目安）。

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

## 6. 送達状態一覧（自分宛）

パス: `/api/v1/notification/deliveries`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 自分宛の送達状態（in-app/email/LINE）を検索・ページングして返す。  
- notification_deliveries を起点に返す（「どのチャネルで送れた/失敗したか」の可視化、運用ログにも使える）

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `channel` (任意): `in_app|email|line|all`（default=all）
- `status` (任意): `pending|sent|failed|all`（default=all）
- `from` (任意): string（ISO8601, createdAt下限）
- `to` (任意): string（ISO8601, createdAt上限）
- `sort` (任意, default=`createdAt:desc`): `createdAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=20)

レスポンスモデル: `ApiResponse[Paged[DeliveryRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "deliveryId": 90001,
        "notificationId": 50001,
        "channel": "email",
        "status": "sent",
        "attemptCount": 1,
        "providerMessageId": "line_or_mail_msg_id",
        "nextRetryAt": null,
        "createdAt": "2026-01-16T10:26:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 3
  },
  "operation": "notification_delivery_list"
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
  "operation": "notification_delivery_list"
}
~~~

---

## 7. 送達状態一覧（通知単位）

パス: `/api/v1/notification/notifications/{notification_id}/deliveries`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: ある通知について、チャネル別送達状態を返す。  
- notifications は「自分宛」しか見せないので、他人宛の場合は 404

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `notification_id`: BIGINT

レスポンスモデル: `ApiResponse[NotificationDeliveries]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "notificationId": 50001,
    "deliveries": [
      { "channel": "in_app", "status": "sent", "attemptCount": 0, "nextRetryAt": null },
      { "channel": "email", "status": "sent", "attemptCount": 1, "nextRetryAt": null },
      { "channel": "line", "status": "failed", "attemptCount": 2, "nextRetryAt": "2026-01-16T11:00:00Z", "errorType": "LINE_BLOCKED" }
    ]
  },
  "operation": "notification_deliveries_get"
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
  "operation": "notification_deliveries_get"
}
~~~

---

## 8. 受信設定取得

パス: `/api/v1/notification/preferences/me`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 自分の受信設定（notification_preferences）を返す。  
※未作成の場合は自動作成（デフォルト）して返してもよい（実装方針）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[NotificationPreferences]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "accountId": 1001,
    "inAppEnabled": true,
    "emailEnabled": true,
    "lineEnabled": false,
    "muteAll": false
  },
  "operation": "notification_preferences_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

~~~json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": { "type": "AUTH_REQUIRED", "details": [] },
  "operation": "notification_preferences_get"
}
~~~

---

## 9. 受信設定更新

パス: `/api/v1/notification/preferences/me`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: ALL  
説明: 自分の受信設定を更新する（部分更新）。  
- muteAll=true の場合、配信対象判定で最優先される

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateNotificationPreferencesRequest`

~~~json
{
  "inAppEnabled": true,
  "emailEnabled": false,
  "lineEnabled": true,
  "muteAll": false
}
~~~

レスポンスモデル: `ApiResponse[NotificationPreferences]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Preferences updated",
  "data": {
    "accountId": 1001,
    "inAppEnabled": true,
    "emailEnabled": false,
    "lineEnabled": true,
    "muteAll": false
  },
  "operation": "notification_preferences_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500  
※LINE未連携なのに lineEnabled=true を許すかは実装方針だが、設計としては「許す（将来連携に備える）」を推奨

~~~json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "lineEnabled", "reason": "invalid_type" }
    ]
  },
  "operation": "notification_preferences_update"
}
~~~

---

## 10. LINE連携状態取得

パス: `/api/v1/notification/line/me`  
メソッド: `GET`  
認証: 必要  
対象ロール: ALL  
説明: 自分のLINE連携状態（account_line_links）を返す。未作成の場合は status=未連携相当で返してよい。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[LineLinkStatus]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "accountId": 1001,
    "status": "linked",
    "linkedAt": "2026-01-16T09:00:00Z",
    "unlinkedAt": null
  },
  "operation": "notification_line_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

~~~json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": { "type": "AUTH_REQUIRED", "details": [] },
  "operation": "notification_line_get"
}
~~~

---

## 11. LINE連携（登録/更新）

パス: `/api/v1/notification/line/me`  
メソッド: `POST`  
認証: 必要  
対象ロール: ALL  
説明: 自分の account に line_user_id を登録し、連携済みにする。  
- 連携フロー（LINEログイン/OAuth等）は別途だが、最終的に得られた line_user_id を登録する
- account_id は一意なので「自分の連携」を上書き更新する形にしてよい
- 既に別アカウントが同じ line_user_id を保持している場合は 409 を推奨（運用の安全性）

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpsertLineLinkRequest`

~~~json
{
  "lineUserId": "Uxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "linkMode": "link"
}
~~~

レスポンスモデル: `ApiResponse[LineLinkStatus]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "LINE linked",
  "data": {
    "accountId": 1001,
    "status": "linked",
    "linkedAt": "2026-01-16T10:40:00Z",
    "unlinkedAt": null
  },
  "operation": "notification_line_link"
}
~~~

エラーレスポンス:
- 401 / 403 / 409 / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "LINE user already linked to another account",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "lineUserId", "reason": "already_linked" }
    ]
  },
  "operation": "notification_line_link"
}
~~~

---

## 12. LINE連携解除

パス: `/api/v1/notification/line/me`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: ALL  
説明: 自分のLINE連携を解除する（status=解除）。  
※line_user_id の物理削除/保持は実装方針だが、スキーマ上は status と unlinked_at を更新する想定。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[LineUnlinkResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "LINE unlinked",
  "data": {
    "accountId": 1001,
    "status": "unlinked",
    "unlinkedAt": "2026-01-16T10:45:00Z"
  },
  "operation": "notification_line_unlink"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

~~~json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": { "type": "AUTH_REQUIRED", "details": [] },
  "operation": "notification_line_unlink"
}
~~~

---

## 13. 教員：学生一括通知（enqueue）

パス: `/api/v1/notification/teacher/bulk`  
メソッド: `POST`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員が選択学生へ通知を作成し、配信をキューに積む。  
- notifications：対象者分 insert（recipient_account_id を学生ごとに）
- notification_deliveries：指定チャネル分 insert（status=PENDING）
- Worker：deliveries を参照して送信し、status / attempt_count / provider_message_id / error_* / next_retry_at を更新  
- 対象学生の妥当性（担当クラス/管理者権限）は必ず検証する（School側の判定結果を利用）

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
    "type": 4,
    "title": "締切のリマインド",
    "body": "求人応募の締切が近いです。応募状況を確認してください。",
    "linkUrl": "/student/jobs"
  },
  "channels": ["in_app", "email", "line"]
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
    "createdNotifications": 3,
    "createdDeliveries": 9,
    "enqueuedDeliveries": 9,
    "acceptedAt": "2026-01-16T10:35:00Z"
  },
  "operation": "notification_teacher_bulk"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（教員権限なし / 学内IP外 / 担当外学生）
- 404: Not Found（クラス不明など）
- 422: Unprocessable Entity（件名空、本文長など）
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

## 14. 管理者：配信再試行（enqueue）

パス: `/api/v1/notification/admin/deliveries/{delivery_id}/retry`  
メソッド: `POST`  
認証: 必要  
対象ロール: 管理者  
説明: FAILED の delivery を再送キューへ積む（運用向け）。  
- status が FAILED のときのみ受け付ける（SENT/PENDING は 409）
- next_retry_at を即時 or 指定時刻に更新し、Workerが拾えるようにする

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `delivery_id`: BIGINT

リクエストモデル: `RetryDeliveryRequest`（任意）

~~~json
{
  "retryAt": "2026-01-16T11:00:00Z"
}
~~~

レスポンスモデル: `ApiResponse[RetryDeliveryResult]`

~~~json
{
  "success": true,
  "code": 202,
  "message": "Enqueued",
  "data": {
    "deliveryId": 90001,
    "scheduledAt": "2026-01-16T11:00:00Z",
    "acceptedAt": "2026-01-16T10:55:00Z"
  },
  "operation": "notification_admin_delivery_retry"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Delivery is not retryable",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "deliveryId", "reason": "status_not_failed" }
    ]
  },
  "operation": "notification_admin_delivery_retry"
}
~~~
