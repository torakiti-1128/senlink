# API設計：Activityサービス

## 概要

Activityサービスは、学生の「応募・参加・選考の進捗（活動）」を管理する。
- 活動（applications / activities）の一覧・詳細
- 活動に紐づくToDo（activity_todos）の一覧・更新
- 辞退申請（withdrawal_requests）※承認フローは Request サービスと連携（境界は実装方針で統一）
- 活動サマリ（応募数、直近イベント、進捗率など）※画面向け集約はBFFが行うが、Activityは必要な素材APIを提供

前提：
- 認証方式：Cookie（HttpOnly） + JWT
- ロール：0=学生 / 1=教員 / 2=管理者
- セキュリティ前提：学内IP制限（学外は 403）

責務境界（重要）：
- 求人情報（会社名/求人名/締切等）は Job サービスが主責務
- Activity は job_id を参照し、活動の状態・ToDo・履歴を管理する
- 画面で「活動履歴+学生名」等の結合が必要な場合は BFF が Port 経由で集約する（Activity単体は最小限の参照情報を返す）

---

## API一覧（Activity）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 活動サマリ（学生） | /api/v1/activity/summary | GET | 必要 | 学生 | 応募数集計、直近イベント、要対応ToDo数など |
| 2 | 活動一覧（学生） | /api/v1/activity/activities | GET | 必要 | 学生 | 応募済求人（活動）を検索/ソートして取得 |
| 3 | 活動詳細（学生） | /api/v1/activity/activities/{activity_id} | GET | 必要 | 学生 | 活動詳細（状態、ToDo進捗、履歴） |
| 4 | ToDo一覧（活動） | /api/v1/activity/activities/{activity_id}/todos | GET | 必要 | 学生 | 活動のToDo一覧 |
| 5 | ToDo更新（完了/未完了等） | /api/v1/activity/todos/{todo_id} | PATCH | 必要 | 学生 | ToDoのステータス更新 |
| 6 | 辞退申請（学生） | /api/v1/activity/activities/{activity_id}/withdrawal | POST | 必要 | 学生 | 辞退申請を作成（教員承認フロー） |
| 7 | 辞退申請状況取得（学生） | /api/v1/activity/activities/{activity_id}/withdrawal | GET | 必要 | 学生 | 自分の辞退申請の状態を取得 |
| 8 | 活動一覧（教員/管理者） | /api/v1/activity/admin/activities | GET | 必要 | 教員/管理者 | クラス/学生単位で活動一覧を取得（監督・指導用） |
| 9 | 活動詳細（教員/管理者） | /api/v1/activity/admin/activities/{activity_id} | GET | 必要 | 教員/管理者 | 学生活動の詳細を参照（指導用） |
| 10 | ToDo一覧（教員/管理者） | /api/v1/activity/admin/activities/{activity_id}/todos | GET | 必要 | 教員/管理者 | 指導に必要なToDo状況参照 |
| 11 | 活動ステータス更新（教員/管理者） | /api/v1/activity/admin/activities/{activity_id}/status | PATCH | 必要 | 教員/管理者 | 例：企業側の進捗反映など（運用で必要なら） |

---

## 1. 活動サマリ（学生）

パス: `/api/v1/activity/summary`  
メソッド: `GET`  
認証: 必要  
説明: 学生ホーム/活動画面で使う「集計の素材」を返す（BFFで他情報と結合する前提）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `rangeDays`: int（default=30）※推移系を返す場合の期間
- `tz`: string（default="UTC"）※日付境界

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[StudentActivitySummary]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "counts": {
      "seminar": 3,
      "intern": 2,
      "exam": 1,
      "offer": 0
    },
    "nextEvent": {
      "activityId": 70001,
      "jobId": 3001,
      "eventStartAt": "2026-01-20T01:00:00Z"
    },
    "urgentTodoCount": 2,
    "overallProgressRate": 0.42,
    "trend": {
      "rangeDays": 30,
      "progressRateSeries": [
        { "date": "2025-12-18", "value": 0.30 },
        { "date": "2026-01-01", "value": 0.38 },
        { "date": "2026-01-16", "value": 0.42 }
      ]
    },
    "updatedAt": "2026-01-16T10:00:00Z"
  },
  "operation": "activity_student_summary"
}
```

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（学内IP外）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": { "type": "AUTH_REQUIRED", "details": [] },
  "operation": "activity_student_summary"
}
```

---

## 2. 活動一覧（学生）

パス: `/api/v1/activity/activities`  
メソッド: `GET`  
認証: 必要  
説明: 学生の応募済求人（活動）一覧を取得する。求人名/会社名は Job から補完する前提のため、Activity は job_id を返す。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `status`: string（`all | upcoming | past | withdrawn | withdrawalRequested`）
- `jobType`: string（`seminar | intern | exam`）※または int enum（実装統一）
- `q`: string（フリーワード ※BFFでJobと結合して検索する方針の場合は無効でも可）
- `sort`: string（default=`eventStartAt:asc`）例：`eventStartAt:asc|desc`, `progressRate:desc`, `deadline:asc`
- `page`: int（default=1）
- `pageSize`: int（default=20）
- `tz`: string（default="UTC"`）

リクエストモデル: `ListStudentActivitiesQuery`（query）

```json
{
  "status": "all",
  "jobType": null,
  "q": null,
  "sort": "eventStartAt:asc",
  "page": 1,
  "pageSize": 20,
  "tz": "UTC"
}
```

レスポンスモデル: `ApiResponse[Paged[StudentActivityRow]]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "activityId": 70001,
        "jobId": 3001,
        "jobType": "seminar",
        "eventStartAt": "2026-01-20T01:00:00Z",
        "cancelDeadline": "2026-01-19",
        "status": "normal",
        "progressRate": 0.25,
        "todoCounts": { "done": 1, "total": 4 },
        "nearestTodoDeadline": "2026-01-17",
        "urgentTodoCount": 1,
        "canWithdraw": true
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 12
  },
  "operation": "activity_student_list"
}
```

エラーレスポンス:
- 401 / 403 / 422 / 500

```json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "status", "reason": "invalid_enum" }
    ]
  },
  "operation": "activity_student_list"
}
```

---

## 3. 活動詳細（学生）

パス: `/api/v1/activity/activities/{activity_id}`  
メソッド: `GET`  
認証: 必要  
説明: 活動の詳細を取得する（ToDoや履歴のサマリを含む）。求人/会社名はBFFでJobから取得して結合する想定。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `activity_id`: BIGINT

クエリパラメータ（任意）:
- `include`: string（例: `todos,history` / default=`todos`）

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[StudentActivityDetail]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "activity": {
      "activityId": 70001,
      "jobId": 3001,
      "jobType": "seminar",
      "status": "normal",
      "createdAt": "2026-01-10T02:00:00Z",
      "updatedAt": "2026-01-16T09:00:00Z",
      "eventStartAt": "2026-01-20T01:00:00Z",
      "cancelDeadline": "2026-01-19"
    },
    "todoSummary": {
      "done": 1,
      "total": 4,
      "progressRate": 0.25,
      "urgentTodoCount": 1
    },
    "todos": [
      {
        "todoId": 91001,
        "title": "履歴書ドラフト作成",
        "content": "テンプレに沿って作成",
        "status": 0,
        "deadline": "2026-01-17",
        "isUrgent": true,
        "daysToDeadline": 1
      }
    ],
    "history": [
      {
        "type": "TODO_DONE",
        "occurredAt": "2026-01-16T08:30:00Z",
        "details": { "todoId": 91001 }
      }
    ]
  },
  "operation": "activity_student_detail"
}
```

エラーレスポンス:
- 401 / 403（他人のactivity） / 404 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "activityId", "reason": "not_owner" }
    ]
  },
  "operation": "activity_student_detail"
}
```

---

## 4. ToDo一覧（活動）

パス: `/api/v1/activity/activities/{activity_id}/todos`  
メソッド: `GET`  
認証: 必要  
説明: 活動に紐づくToDo一覧を返す（学生本人のみ）。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `activity_id`: BIGINT

クエリパラメータ（任意）:
- `status`: string（`all | open | done`）
- `sort`: string（default=`deadline:asc`）

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[ActivityTodoList]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "activityId": 70001,
    "items": [
      {
        "todoId": 91001,
        "title": "履歴書ドラフト作成",
        "content": "テンプレに沿って作成",
        "status": 0,
        "deadline": "2026-01-17",
        "isVerificationRequired": false
      }
    ]
  },
  "operation": "activity_todos_list"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Activity not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "activity_todos_list"
}
```

---

## 5. ToDo更新（完了/未完了等）

パス: `/api/v1/activity/todos/{todo_id}`  
メソッド: `PATCH`  
認証: 必要  
説明: ToDoステータスを更新する（学生本人のみ）。必要なら `doneAt` をサーバ側で付与。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `todo_id`: BIGINT

リクエストモデル: `UpdateActivityTodoRequest`

```json
{
  "status": 1
}
```

レスポンスモデル: `ApiResponse[UpdateActivityTodoResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "todoId": 91001,
    "status": 1,
    "updatedAt": "2026-01-16T10:01:00Z"
  },
  "operation": "activity_todo_update"
}
```

エラーレスポンス:
- 401 / 403（他人のtodo） / 404 / 409（状態遷移不可） / 422 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Todo not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "activity_todo_update"
}
```

---

## 6. 辞退申請（学生）

パス: `/api/v1/activity/activities/{activity_id}/withdrawal`  
メソッド: `POST`  
認証: 必要  
説明: 辞退申請を作成する。承認フローは「Requestサービスで一元管理」する運用でもよいが、Activityは「辞退の開始点」を提供する。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `activity_id`: BIGINT

リクエストモデル: `CreateWithdrawalRequest`

```json
{
  "reason": "他社内定のため",
  "note": "可能であれば早めに承認お願いします"
}
```

レスポンスモデル: `ApiResponse[WithdrawalRequestResult]`

```json
{
  "success": true,
  "code": 201,
  "message": "Requested",
  "data": {
    "activityId": 70001,
    "withdrawalRequestId": 88001,
    "status": "pending",
    "requestedAt": "2026-01-16T10:05:00Z"
  },
  "operation": "activity_withdrawal_request"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409（既に申請中・辞退済等） / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Withdrawal already requested",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "activityId", "reason": "already_requested" }
    ]
  },
  "operation": "activity_withdrawal_request"
}
```

---

## 7. 辞退申請状況取得（学生）

パス: `/api/v1/activity/activities/{activity_id}/withdrawal`  
メソッド: `GET`  
認証: 必要  
説明: 自分の辞退申請状況を返す（申請なしなら 404 を返す）。

リクエストヘッダー:
- `Accept: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[WithdrawalStatus]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "activityId": 70001,
    "withdrawalRequestId": 88001,
    "status": "pending",
    "reason": "他社内定のため",
    "requestedAt": "2026-01-16T10:05:00Z",
    "resolvedAt": null,
    "reviewComment": null
  },
  "operation": "activity_withdrawal_get"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Withdrawal request not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "activity_withdrawal_get"
}
```

---

## 8. 活動一覧（教員/管理者）

パス: `/api/v1/activity/admin/activities`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 指導・監督用に、クラス/学生で活動を検索する。学生名などの結合は BFF 側で School と結合する前提（ActivityはID群を返す）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `classId`: BIGINT（担当範囲チェック）
- `studentAccountId`: BIGINT
- `status`: string（`all | normal | done | withdrawn | withdrawalRequested`）
- `jobType`: string（`seminar | intern | exam`）
- `fromDate`: string（YYYY-MM-DD）
- `toDate`: string（YYYY-MM-DD）
- `page`: int（default=1）
- `pageSize`: int（default=20）
- `sort`: string（default=`updatedAt:desc`）

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[Paged[AdminActivityRow]]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "activityId": 70001,
        "studentAccountId": 1001,
        "jobId": 3001,
        "status": "normal",
        "progressRate": 0.25,
        "overdueTodos": 1,
        "updatedAt": "2026-01-16T09:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 40
  },
  "operation": "activity_admin_list"
}
```

エラーレスポンス:
- 401 / 403（担当外class） / 422 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "classId", "reason": "not_assigned" }
    ]
  },
  "operation": "activity_admin_list"
}
```

---

## 9. 活動詳細（教員/管理者）

パス: `/api/v1/activity/admin/activities/{activity_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 指導用の活動詳細参照。個人情報は School 側が主なので、必要に応じ BFF で結合する。

リクエストヘッダー:
- `Accept: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[AdminActivityDetail]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "activityId": 70001,
    "studentAccountId": 1001,
    "jobId": 3001,
    "status": "normal",
    "todoSummary": { "done": 1, "total": 4, "overdue": 1 },
    "lastActivityAt": "2026-01-16T08:30:00Z"
  },
  "operation": "activity_admin_detail"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Activity not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "activity_admin_detail"
}
```

---

## 10. ToDo一覧（教員/管理者）

パス: `/api/v1/activity/admin/activities/{activity_id}/todos`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 指導のためのToDo一覧参照（更新は学生のみを基本）。

リクエストヘッダー:
- `Accept: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[AdminActivityTodoList]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "activityId": 70001,
    "items": [
      {
        "todoId": 91001,
        "title": "履歴書ドラフト作成",
        "status": 0,
        "deadline": "2026-01-17",
        "isOverdue": false
      }
    ]
  },
  "operation": "activity_admin_todos_list"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "activity_admin_todos_list"
}
```

---

## 11. 活動ステータス更新（教員/管理者）

パス: `/api/v1/activity/admin/activities/{activity_id}/status`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 企業側の進捗などを反映したい場合の運用API（必要なければ実装しない）。  
※「基本的なAPIで、使用予定がないものは消していい」方針に従い、後で削除可。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateActivityStatusRequest`

```json
{
  "status": "done",
  "note": "内定確定"
}
```

レスポンスモデル: `ApiResponse[UpdateActivityStatusResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "activityId": 70001,
    "status": "done",
    "updatedAt": "2026-01-16T10:30:00Z"
  },
  "operation": "activity_admin_status_update"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409（不正遷移） / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Invalid status transition",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "status", "reason": "transition_not_allowed" }
    ]
  },
  "operation": "activity_admin_status_update"
}
```
