# API設計：Requestサービス

## 概要

Requestサービスは、学生の各種申請（書類添削／面談予約／内定報告など）と、教員による承認・差し戻し（ステート管理）を提供する。
- 認証方式：Cookie（HttpOnly） + JWT
- ロール：0=学生 / 1=教員 / 2=管理者
- セキュリティ前提：学内IP制限（学外は 403）
- DBモデル方針：
  - 申請本体：`requests`（payloadはJSONBで種別差分を吸収）
  - コメント：`request_comments`
  - 添付：`request_attachments`（Storageパスを保持）

### ドメイン定義（共通）
- request.type：
  - 0: 書類提出（書類添削）
  - 1: 面談予約
  - 2: 合否発表（内定報告）
- request.status：
  - 0: 下書き
  - 1: 申請中
  - 2: 承認
  - 3: 差し戻し

---

## API一覧（Request）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 申請下書き作成 | /api/v1/request/requests | POST | 必要 | 学生 | 申請を下書きで作成（payloadはtype別） |
| 2 | 申請下書き更新 | /api/v1/request/requests/{request_id} | PUT | 必要 | 学生 | 下書きのタイトル/payload/添付の紐付けを更新 |
| 3 | 申請送信（下書き→申請中） | /api/v1/request/requests/{request_id}/submit | POST | 必要 | 学生 | 下書きを送信し status=申請中 に遷移 |
| 4 | 自分の申請一覧 | /api/v1/request/requests | GET | 必要 | 学生 | 自分の申請を検索・ページングして取得 |
| 5 | 自分の申請詳細 | /api/v1/request/requests/{request_id} | GET | 必要 | 学生 | 自分の申請の詳細（コメント/添付含む） |
| 6 | コメント投稿 | /api/v1/request/requests/{request_id}/comments | POST | 必要 | 学生/教員/管理者 | 申請にコメントを追加（メッセージ） |
| 7 | コメント一覧 | /api/v1/request/requests/{request_id}/comments | GET | 必要 | 学生/教員/管理者 | 申請コメントの一覧取得 |
| 8 | 添付アップロード | /api/v1/request/requests/{request_id}/attachments | POST | 必要 | 学生 | 申請添付をアップロードし Storage パスを保存 |
| 9 | 添付削除 | /api/v1/request/requests/{request_id}/attachments/{attachment_id} | DELETE | 必要 | 学生 | 添付を削除（Storage削除は別途方針） |
| 10 | 審査対象申請一覧（教員） | /api/v1/request/reviewer/requests | GET | 必要 | 教員/管理者 | 審査対象（担当範囲）を検索・ページング取得 |
| 11 | 審査対象申請詳細（教員） | /api/v1/request/reviewer/requests/{request_id} | GET | 必要 | 教員/管理者 | 審査用に詳細（コメント/添付含む）を取得 |
| 12 | 担当者割当（自分に割当） | /api/v1/request/reviewer/requests/{request_id}/assign | PATCH | 必要 | 教員/管理者 | reviewer_account_id を自分に設定（任意運用） |
| 13 | 承認 | /api/v1/request/reviewer/requests/{request_id}/approve | PATCH | 必要 | 教員/管理者 | 申請を承認（status=承認、承認コメント） |
| 14 | 差し戻し | /api/v1/request/reviewer/requests/{request_id}/return | PATCH | 必要 | 教員/管理者 | 申請を差し戻し（status=差し戻し、差し戻しコメント） |
| 15 | 管理者：申請一覧（全件） | /api/v1/request/admin/requests | GET | 必要 | 管理者 | 全件を検索・ページング取得（監査・運用） |

---

## payload（JSONB）スキーマ（推奨）

> `requests.payload` は JSONB で保持し、typeごとに必須項目をバリデーションする。  
> `kind` はクライアントとデバッグのためのヒント（冗長だが可読性優先）として推奨。

### type=0（書類提出：DocumentRequestPayload）
```json
{
  "kind": "document",
  "documentCategory": "resume",
  "messageToTeacher": "添削お願いします",
  "desiredDueDate": "2026-02-01",
  "priority": "normal"
}
```

### type=1（面談予約：InterviewRequestPayload）
```json
{
  "kind": "interview",
  "topic": "ES相談",
  "messageToTeacher": "面談希望です",
  "candidateWindows": [
    { "from": "2026-01-20T01:00:00Z", "to": "2026-01-20T03:00:00Z" },
    { "from": "2026-01-21T06:00:00Z", "to": "2026-01-21T08:00:00Z" }
  ],
  "preferredMeetingPlace": "201号室 or オンライン",
  "confirmed": null
}
```
- 教員が承認時に `confirmed` を埋める（例）：
```json
{
  "confirmed": {
    "scheduledAt": "2026-01-21T06:30:00Z",
    "meetingPlace": "201号室",
    "note": "時間厳守でお願いします"
  }
}
```

### type=2（合否発表：OfferRequestPayload）
```json
{
  "kind": "offer",
  "companyName": "株式会社サンプル",
  "jobTitle": "バックエンドエンジニア",
  "offerDate": "2026-01-18",
  "messageToTeacher": "内定しました。ご指導ありがとうございました"
}
```

---

## 1. 申請下書き作成

パス: `/api/v1/request/requests`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 申請を下書き（status=0）で作成する。`type` に応じた `payload` を保存する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateRequestDraftRequest`

```json
{
  "type": 1,
  "title": "面談予約申請",
  "payload": {
    "kind": "interview",
    "topic": "ES相談",
    "messageToTeacher": "面談希望です",
    "candidateWindows": [
      { "from": "2026-01-20T01:00:00Z", "to": "2026-01-20T03:00:00Z" }
    ],
    "preferredMeetingPlace": "201号室",
    "confirmed": null
  }
}
```

レスポンスモデル: `ApiResponse[RequestInDB]`

```json
{
  "success": true,
  "code": 201,
  "message": "Draft created",
  "data": {
    "requestId": 9001,
    "requesterAccountId": 1001,
    "reviewerAccountId": null,
    "type": 1,
    "status": 0,
    "title": "面談予約申請",
    "payload": {
      "kind": "interview",
      "topic": "ES相談",
      "messageToTeacher": "面談希望です",
      "candidateWindows": [
        { "from": "2026-01-20T01:00:00Z", "to": "2026-01-20T03:00:00Z" }
      ],
      "preferredMeetingPlace": "201号室",
      "confirmed": null
    },
    "submittedAt": null,
    "resolvedAt": null
  },
  "operation": "request_create_draft"
}
```

エラーレスポンス:
- 400: Bad Request（形式不正）
- 401: Unauthorized（未ログイン）
- 403: Forbidden（学内IP外 / 学生以外）
- 422: Unprocessable Entity（payloadバリデーション不正）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "payload.candidateWindows", "reason": "required_for_type_interview" }
    ]
  },
  "operation": "request_create_draft"
}
```

---

## 2. 申請下書き更新

パス: `/api/v1/request/requests/{request_id}`  
メソッド: `PUT`  
認証: 必要  
対象ロール: 学生  
説明: 下書き（status=0）のみ更新できる。申請中以降は 409 を返す。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `request_id`: BIGINT

リクエストモデル: `UpdateRequestDraftRequest`

```json
{
  "title": "面談予約申請（候補日更新）",
  "payload": {
    "kind": "interview",
    "topic": "ES相談",
    "messageToTeacher": "候補日を更新しました",
    "candidateWindows": [
      { "from": "2026-01-21T06:00:00Z", "to": "2026-01-21T08:00:00Z" }
    ],
    "preferredMeetingPlace": "オンライン",
    "confirmed": null
  }
}
```

レスポンスモデル: `ApiResponse[RequestInDB]`

```json
{
  "success": true,
  "code": 200,
  "message": "Draft updated",
  "data": {
    "requestId": 9001,
    "requesterAccountId": 1001,
    "reviewerAccountId": null,
    "type": 1,
    "status": 0,
    "title": "面談予約申請（候補日更新）",
    "payload": {
      "kind": "interview",
      "topic": "ES相談",
      "messageToTeacher": "候補日を更新しました",
      "candidateWindows": [
        { "from": "2026-01-21T06:00:00Z", "to": "2026-01-21T08:00:00Z" }
      ],
      "preferredMeetingPlace": "オンライン",
      "confirmed": null
    },
    "submittedAt": null,
    "resolvedAt": null
  },
  "operation": "request_update_draft"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Request is not editable",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "status", "reason": "only_draft_editable" }
    ]
  },
  "operation": "request_update_draft"
}
```

---

## 3. 申請送信（下書き→申請中）

パス: `/api/v1/request/requests/{request_id}/submit`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 下書きを送信し、status=1（申請中）へ遷移する。`submitted_at` を設定する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[RequestSubmittedResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Request submitted",
  "data": {
    "requestId": 9001,
    "status": 1,
    "submittedAt": "2026-01-16T10:00:00Z"
  },
  "operation": "request_submit"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Request cannot be submitted",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "status", "reason": "only_draft_submittable" }
    ]
  },
  "operation": "request_submit"
}
```

---

## 4. 自分の申請一覧

パス: `/api/v1/request/requests`  
メソッド: `GET`  
認証: 必要  
対象ロール: 学生  
説明: 自分の申請のみを検索・ページングして返す。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `type` (任意): `0|1|2`
- `status` (任意): `0|1|2|3`
- `q` (任意): string（title部分一致）
- `from` (任意): string（ISO8601, submittedAt の下限）
- `to` (任意): string（ISO8601, submittedAt の上限）
- `sort` (任意, default=`submittedAt:desc`): `submittedAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=20)

レスポンスモデル: `ApiResponse[Paged[RequestRow]]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "requestId": 9001,
        "type": 1,
        "status": 1,
        "title": "面談予約申請",
        "reviewerAccountId": null,
        "submittedAt": "2026-01-16T10:00:00Z",
        "resolvedAt": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 3
  },
  "operation": "request_list_mine"
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
      { "field": "sort", "reason": "invalid_format" }
    ]
  },
  "operation": "request_list_mine"
}
```

---

## 5. 自分の申請詳細

パス: `/api/v1/request/requests/{request_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: 学生  
説明: 自分の申請詳細を返す（payload、コメント、添付を含む）。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `request_id`: BIGINT

レスポンスモデル: `ApiResponse[RequestDetail]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "request": {
      "requestId": 9001,
      "requesterAccountId": 1001,
      "reviewerAccountId": 2001,
      "type": 1,
      "status": 1,
      "title": "面談予約申請",
      "payload": {
        "kind": "interview",
        "topic": "ES相談",
        "messageToTeacher": "面談希望です",
        "candidateWindows": [
          { "from": "2026-01-20T01:00:00Z", "to": "2026-01-20T03:00:00Z" }
        ],
        "preferredMeetingPlace": "201号室",
        "confirmed": null
      },
      "submittedAt": "2026-01-16T10:00:00Z",
      "resolvedAt": null
    },
    "comments": [
      {
        "commentId": 30001,
        "authorAccountId": 1001,
        "commentType": 0,
        "body": "よろしくお願いします",
        "createdAt": "2026-01-16T10:01:00Z"
      }
    ],
    "attachments": [
      {
        "attachmentId": 40001,
        "filePath": "requests/9001/resume.pdf",
        "fileType": 0,
        "description": "履歴書"
      }
    ]
  },
  "operation": "request_get_mine"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "requestId", "reason": "not_owner" }
    ]
  },
  "operation": "request_get_mine"
}
```

---

## 6. コメント投稿

パス: `/api/v1/request/requests/{request_id}/comments`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生/教員/管理者  
説明: 申請にコメント（メッセージ）を追加する。  
- 学生：自分の申請のみ投稿可  
- 教員：審査対象（担当範囲）の申請のみ投稿可  
- 管理者：全件投稿可

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateRequestCommentRequest`

```json
{
  "body": "候補日を追加しました。ご確認ください。"
}
```

レスポンスモデル: `ApiResponse[RequestCommentInDB]`

```json
{
  "success": true,
  "code": 201,
  "message": "Comment created",
  "data": {
    "commentId": 30002,
    "requestId": 9001,
    "authorAccountId": 1001,
    "commentType": 0,
    "body": "候補日を追加しました。ご確認ください。",
    "createdAt": "2026-01-16T10:05:00Z"
  },
  "operation": "request_comment_create"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "requestId", "reason": "not_allowed" }
    ]
  },
  "operation": "request_comment_create"
}
```

---

## 7. コメント一覧

パス: `/api/v1/request/requests/{request_id}/comments`  
メソッド: `GET`  
認証: 必要  
対象ロール: 学生/教員/管理者  
説明: 申請コメント一覧を返す。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[RequestCommentsResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "requestId": 9001,
    "items": [
      {
        "commentId": 30001,
        "authorAccountId": 1001,
        "commentType": 0,
        "body": "よろしくお願いします",
        "createdAt": "2026-01-16T10:01:00Z"
      },
      {
        "commentId": 30003,
        "authorAccountId": 2001,
        "commentType": 0,
        "body": "候補日確認します",
        "createdAt": "2026-01-16T10:06:00Z"
      }
    ]
  },
  "operation": "request_comment_list"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Request not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "request_comment_list"
}
```

---

## 8. 添付アップロード

パス: `/api/v1/request/requests/{request_id}/attachments`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 申請に添付ファイルを追加する（Storageへ保存し、`request_attachments` にパスを保存）。  
※ `request_id` は「自分の下書き or 自分の申請中/差し戻し」のみ許可（承認済みは原則不可。運用で変更するなら明記）。

リクエストヘッダー:
- `Content-Type: multipart/form-data`

リクエストモデル: `UploadRequestAttachmentRequest`
- `file`: binary（必須）
- `fileType`: int（0:書類／1:画像／9:その他）
- `description`: string（任意）

レスポンスモデル: `ApiResponse[RequestAttachmentInDB]`

```json
{
  "success": true,
  "code": 201,
  "message": "Attachment uploaded",
  "data": {
    "attachmentId": 40002,
    "requestId": 9001,
    "filePath": "requests/9001/portfolio.pdf",
    "fileType": 0,
    "description": "ポートフォリオ"
  },
  "operation": "request_attachment_upload"
}
```

エラーレスポンス:
- 400 / 401 / 403 / 404 / 409 / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Attachment already exists",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "filePath", "reason": "duplicate_for_request" }
    ]
  },
  "operation": "request_attachment_upload"
}
```

---

## 9. 添付削除

パス: `/api/v1/request/requests/{request_id}/attachments/{attachment_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 学生  
説明: 添付レコードを削除する。Storage実体の削除は方針に従う（即時削除 or 遅延削除）。  
※自分の申請のみ対象。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[DeleteAttachmentResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Attachment deleted",
  "data": {
    "attachmentId": 40002,
    "deletedAt": "2026-01-16T10:12:00Z"
  },
  "operation": "request_attachment_delete"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 404,
  "message": "Attachment not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "request_attachment_delete"
}
```

---

## 10. 審査対象申請一覧（教員）

パス: `/api/v1/request/reviewer/requests`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 審査対象の申請一覧を返す。  
- 教員：担当範囲（クラス/学生）に限定（担当判定は School の責務だが、本APIでは「審査可能か」を必ず検証する）  
- 管理者：全件可（運用方針に合わせて絞り込み可能）

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `classId` (任意): BIGINT（担当クラスで絞り込み）
- `type` (任意): `0|1|2`
- `status` (任意): `1|2|3`（教員ビューでは下書きは対象外を推奨）
- `q` (任意): string（title部分一致）
- `from` (任意): string（ISO8601）
- `to` (任意): string（ISO8601）
- `sort` (任意, default=`submittedAt:desc`): `submittedAt:desc|asc`
- `page` (任意, default=1)
- `pageSize` (任意, default=20)

レスポンスモデル: `ApiResponse[Paged[ReviewerRequestRow]]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "requestId": 9001,
        "requesterAccountId": 1001,
        "reviewerAccountId": null,
        "type": 1,
        "status": 1,
        "title": "面談予約申請",
        "submittedAt": "2026-01-16T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 12
  },
  "operation": "request_reviewer_list"
}
```

エラーレスポンス:
- 401 / 403 / 422 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "role", "reason": "reviewer_required" }
    ]
  },
  "operation": "request_reviewer_list"
}
```

---

## 11. 審査対象申請詳細（教員）

パス: `/api/v1/request/reviewer/requests/{request_id}`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 審査対象の申請詳細を返す（コメント/添付含む）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[ReviewerRequestDetail]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "request": {
      "requestId": 9001,
      "requesterAccountId": 1001,
      "reviewerAccountId": null,
      "type": 1,
      "status": 1,
      "title": "面談予約申請",
      "payload": {
        "kind": "interview",
        "topic": "ES相談",
        "messageToTeacher": "面談希望です",
        "candidateWindows": [
          { "from": "2026-01-20T01:00:00Z", "to": "2026-01-20T03:00:00Z" }
        ],
        "preferredMeetingPlace": "201号室",
        "confirmed": null
      },
      "submittedAt": "2026-01-16T10:00:00Z",
      "resolvedAt": null
    },
    "comments": [],
    "attachments": []
  },
  "operation": "request_reviewer_get"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "requestId", "reason": "not_assigned_scope" }
    ]
  },
  "operation": "request_reviewer_get"
}
```

---

## 12. 担当者割当（自分に割当）

パス: `/api/v1/request/reviewer/requests/{request_id}/assign`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: reviewer_account_id を自分に設定する（運用で「担当者を明示」したい場合）。  
※ 既に reviewer がいる場合は 409（上書き禁止）を推奨。運用で上書きを許すなら別APIで明示。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[AssignReviewerResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Reviewer assigned",
  "data": {
    "requestId": 9001,
    "reviewerAccountId": 2001,
    "updatedAt": "2026-01-16T10:20:00Z"
  },
  "operation": "request_reviewer_assign"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Reviewer already assigned",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "reviewerAccountId", "reason": "already_set" }
    ]
  },
  "operation": "request_reviewer_assign"
}
```

---

## 13. 承認

パス: `/api/v1/request/reviewer/requests/{request_id}/approve`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 申請を承認し status=2、resolved_at を設定する。承認コメント（comment_type=1）を必ず残す。  
面談予約(type=1)の場合、承認時に `payload.confirmed` を設定できる。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `ApproveRequestRequest`

```json
{
  "comment": "承認します。1/21 15:30に201号室で実施します。",
  "payloadPatch": {
    "confirmed": {
      "scheduledAt": "2026-01-21T06:30:00Z",
      "meetingPlace": "201号室",
      "note": "時間厳守でお願いします"
    }
  }
}
```

レスポンスモデル: `ApiResponse[RequestDecisionResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Request approved",
  "data": {
    "requestId": 9001,
    "status": 2,
    "reviewerAccountId": 2001,
    "resolvedAt": "2026-01-16T10:25:00Z",
    "commentId": 30010
  },
  "operation": "request_reviewer_approve"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

```json
{
  "success": false,
  "code": 409,
  "message": "Request cannot be approved",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "status", "reason": "only_pending_approvable" }
    ]
  },
  "operation": "request_reviewer_approve"
}
```

---

## 14. 差し戻し

パス: `/api/v1/request/reviewer/requests/{request_id}/return`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 申請を差し戻しし status=3、resolved_at を設定する。差し戻しコメント（comment_type=2）を必ず残す。  
学生は差し戻し後に下書き相当の編集を行い、再送信する想定（運用：差し戻し時に status=0 に戻す設計も可能だが、本設計では status=3 を維持）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `ReturnRequestRequest`

```json
{
  "comment": "候補日の幅が狭いので、別日程も追加してください。"
}
```

レスポンスモデル: `ApiResponse[RequestDecisionResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Request returned",
  "data": {
    "requestId": 9001,
    "status": 3,
    "reviewerAccountId": 2001,
    "resolvedAt": "2026-01-16T10:30:00Z",
    "commentId": 30011
  },
  "operation": "request_reviewer_return"
}
```

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

```json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "comment", "reason": "required" }
    ]
  },
  "operation": "request_reviewer_return"
}
```

---

## 15. 管理者：申請一覧（全件）

パス: `/api/v1/request/admin/requests`  
メソッド: `GET`  
認証: 必要  
対象ロール: 管理者  
説明: 全申請を検索・ページングして返す（監査・運用向け）。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `type` (任意): `0|1|2`
- `status` (任意): `0|1|2|3`
- `requesterAccountId` (任意): BIGINT
- `reviewerAccountId` (任意): BIGINT
- `from` (任意): string（ISO8601, submittedAt）
- `to` (任意): string（ISO8601, submittedAt）
- `sort` (任意, default=`submittedAt:desc`)
- `page` (任意, default=1)
- `pageSize` (任意, default=50)

レスポンスモデル: `ApiResponse[Paged[AdminRequestRow]]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "requestId": 9001,
        "requesterAccountId": 1001,
        "reviewerAccountId": 2001,
        "type": 1,
        "status": 2,
        "title": "面談予約申請",
        "submittedAt": "2026-01-16T10:00:00Z",
        "resolvedAt": "2026-01-16T10:25:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "total": 120
  },
  "operation": "request_admin_list"
}
```

エラーレスポンス:
- 401 / 403 / 422 / 500

```json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": {
    "type": "FORBIDDEN",
    "details": []
  },
  "operation": "request_admin_list"
}
```
