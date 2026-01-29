# API設計：Jobサービス

## 概要

Jobサービスは、SenLink の「求人情報」を中心に、以下を提供する。
- 求人（jobs）の検索／一覧／詳細
- 求人のブックマーク（job_bookmarks）
- 求人の公開範囲（publish_scope + job_target_classes / job_target_students）
- 求人レコメンド（job_recommendations）
- 求人タグ（job_tags / job_tag_relations）
- 求人アンケート（job_surveys / survey_responses）
- ToDoテンプレート（todo_templates / todo_steps）※活動側で利用するための参照・管理

前提：
- 認証方式：Cookie（HttpOnly） + JWT
- ロール：0=学生 / 1=教員 / 2=管理者
- セキュリティ前提：学内IP制限（学外は 403）

補足（責務境界）：
- 「応募操作（活動作成／ToDo生成）」は Activity サービスが主責務（Jobは求人情報とテンプレを提供）
- ただし Job サービスは、画面表示に必要な「応募可否」や「テンプレ概要」などの参照情報を返す

---

## API一覧（Job）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 求人一覧（検索/絞り込み） | /api/v1/job/jobs | GET | 必要 | 学生/教員/管理者 | 種別・期間・タグ・公開状態等で求人一覧を取得 |
| 2 | 求人詳細 | /api/v1/job/jobs/{job_id} | GET | 必要 | 学生/教員/管理者 | 求人 + 企業 + タグ + 公開範囲（教員以上） + アンケート概要等 |
| 3 | タグ一覧（検索用） | /api/v1/job/tags | GET | 必要 | 学生/教員/管理者 | 絞り込みUI用のタグ一覧 |
| 4 | ブックマーク一覧 | /api/v1/job/bookmarks | GET | 必要 | 学生 | 自分のブックマーク求人一覧 |
| 5 | ブックマーク追加 | /api/v1/job/bookmarks | POST | 必要 | 学生 | 求人をブックマーク登録 |
| 6 | ブックマーク解除 | /api/v1/job/bookmarks/{job_id} | DELETE | 必要 | 学生 | 求人のブックマーク解除 |
| 7 | 求人アンケート取得 | /api/v1/job/jobs/{job_id}/survey | GET | 必要 | 学生/教員/管理者 | 求人のアンケート（質問）を取得 |
| 8 | 求人アンケート回答 | /api/v1/job/jobs/{job_id}/survey/response | POST | 必要 | 学生 | アンケート回答を送信 |

| 9 | 求人一覧（管理） | /api/v1/job/admin/jobs | GET | 必要 | 教員/管理者 | 作成者/状態/公開期間など管理向けに取得 |
| 10 | 求人作成 | /api/v1/job/admin/jobs | POST | 必要 | 教員/管理者 | 求人を新規作成（初期は下書き） |
| 11 | 求人更新 | /api/v1/job/admin/jobs/{job_id} | PATCH | 必要 | 教員/管理者 | 求人情報を更新 |
| 12 | 求人削除 | /api/v1/job/admin/jobs/{job_id} | DELETE | 必要 | 教員/管理者 | 求人を削除（論理削除は実装方針に従う） |
| 13 | 求人コピー | /api/v1/job/admin/jobs/{job_id}/copy | POST | 必要 | 教員/管理者 | 既存求人をコピーして新規作成 |
| 14 | 公開状態・公開期間更新 | /api/v1/job/admin/jobs/{job_id}/publish | PATCH | 必要 | 教員/管理者 | status（下書き/公開/募集終了）と期間を更新 |
| 15 | 公開範囲更新 | /api/v1/job/admin/jobs/{job_id}/scope | PATCH | 必要 | 教員/管理者 | publish_scope と対象クラス/学生を更新 |
| 16 | レコメンド付与/解除 | /api/v1/job/admin/jobs/{job_id}/recommendations | PATCH | 必要 | 教員/管理者 | 特定学生/クラスにおすすめ付与・解除 |
| 17 | ToDoテンプレ管理（取得/更新） | /api/v1/job/admin/todo-templates/{template_id} | GET/PATCH | 必要 | 教員/管理者 | todo_templates と todo_steps を管理 |
| 18 | ToDoテンプレ割当（求人） | /api/v1/job/admin/jobs/{job_id}/todo-template | PATCH | 必要 | 教員/管理者 | 求人に todo_template_id を割当 |
| 19 | タグ管理（作成/更新/削除） | /api/v1/job/admin/tags | POST / PATCH / DELETE | 必要 | 教員/管理者 | job_tags の管理 |
| 20 | 企業管理（一覧/作成/更新/削除） | /api/v1/job/admin/companies | GET/POST/PATCH/DELETE | 必要 | 教員/管理者 | companies の管理 |

---

## 1. 求人一覧（検索/絞り込み）

パス: `/api/v1/job/jobs`  
メソッド: `GET`  
認証: 必要  
説明: 求人一覧を取得する。学生は「公開中のみ」を基本とし、教員/管理者は `status` 指定で下書き等も参照できる。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `q`: string（会社名/求人名部分一致）
- `jobType`: int（jobs.job_type）
- `fromDate`: string（YYYY-MM-DD）
- `toDate`: string（YYYY-MM-DD）
- `deadlineFrom`: string（YYYY-MM-DD）
- `deadlineTo`: string（YYYY-MM-DD）
- `tagIds`: string（例: "1,2,3"）
- `onlyRecommended`: boolean（学生のみ：自分宛おすすめを優先表示）
- `status`: int（教員/管理者のみ：0=下書き/1=公開/9=募集終了）
- `page`: int（default=1）
- `pageSize`: int（default=20）
- `sort`: string（例: `eventStartDate:asc`, `deadline:asc`, `createdAt:desc`）
- `tz`: string（default="UTC"）※日付境界のため

リクエストモデル: `ListJobsQuery`（query）

~~~json
{
  "q": "サンプル",
  "jobType": 0,
  "fromDate": "2026-01-01",
  "toDate": "2026-02-01",
  "tagIds": "101,102",
  "onlyRecommended": false,
  "page": 1,
  "pageSize": 20,
  "sort": "eventStartDate:asc",
  "tz": "UTC"
}
~~~

レスポンスモデル: `ApiResponse[Paged[JobListItem]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "jobId": 4001,
        "companyId": 2001,
        "companyName": "株式会社AAA",
        "title": "会社説明会",
        "jobType": 0,
        "format": 1,
        "place": "Zoom",
        "eventStartDate": "2026-01-25",
        "deadline": "2026-02-01",
        "status": 1,
        "isBookmarked": true,
        "isRecommended": false,
        "tags": [
          { "tagId": 101, "name": "バックエンド", "type": 0 }
        ],
        "links": {
          "detail": "/student/jobs/4001"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 120
  },
  "operation": "job_list"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（学内IP外）
- 422: Unprocessable Entity（クエリ不正）
- 500: Internal Server Error

~~~json
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
  "operation": "job_list"
}
~~~

---

## 2. 求人詳細

パス: `/api/v1/job/jobs/{job_id}`  
メソッド: `GET`  
認証: 必要  
説明: 求人詳細を取得する。学生は公開範囲に含まれる求人のみ参照可。教員/管理者は管理情報（公開範囲やテンプレ）も返す。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `job_id`: BIGINT

クエリパラメータ（任意）:
- `includeScope`: boolean（default=false）※教員/管理者のみ意味あり
- `includeTodoTemplate`: boolean（default=true）※概要のみ返す

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[JobDetailView]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "job": {
      "jobId": 4001,
      "companyId": 2001,
      "title": "会社説明会",
      "jobType": 0,
      "format": 1,
      "place": "Zoom",
      "eventStartDate": "2026-01-25",
      "deadline": "2026-02-01",
      "target": 1,
      "content": "募集要項...",
      "status": 1,
      "publishedAt": "2026-01-10T00:00:00Z",
      "closedAt": null
    },
    "company": {
      "companyId": 2001,
      "name": "株式会社AAA",
      "websiteUrl": "https://example.com",
      "address": "東京都..."
    },
    "tags": [
      { "tagId": 101, "name": "バックエンド", "type": 0 }
    ],
    "viewer": {
      "isBookmarked": true,
      "isRecommended": false,
      "canApply": true,
      "denyReason": null
    },
    "survey": {
      "surveyId": 6001,
      "title": "参加アンケート",
      "isRequired": true,
      "hasQuestions": true
    },
    "todoTemplate": {
      "templateId": 7001,
      "name": "説明会テンプレ",
      "stepsCount": 5,
      "verificationRequiredCount": 1
    },
    "scope": {
      "publishScope": 1,
      "targetClassIds": [301, 302],
      "targetStudentIds": []
    }
  },
  "operation": "job_detail"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（公開範囲外 / 学内IP外）
- 404: Not Found
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 403,
  "message": "Job is not visible for this user",
  "error": {
    "type": "FORBIDDEN",
    "details": []
  },
  "operation": "job_detail"
}
~~~

---

## 3. タグ一覧（検索用）

パス: `/api/v1/job/tags`  
メソッド: `GET`  
認証: 必要  
説明: 求人検索の絞り込みで利用するタグ一覧を返す。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `type`: int（job_tags.type）
- `q`: string（name部分一致）

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[JobTagListResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      { "tagId": 101, "name": "バックエンド", "type": 0 },
      { "tagId": 201, "name": "東京", "type": 1 }
    ]
  },
  "operation": "job_tags_list"
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
  "operation": "job_tags_list"
}
~~~

---

## 4. ブックマーク一覧

パス: `/api/v1/job/bookmarks`  
メソッド: `GET`  
認証: 必要  
対象ロール: 学生  
説明: 自分がブックマークした求人一覧を返す。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `page`: int（default=1）
- `pageSize`: int（default=20）
- `sort`: string（default=`bookmarkedAt:desc`）

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[Paged[JobBookmarkItem]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "jobId": 4001,
        "companyName": "株式会社AAA",
        "title": "会社説明会",
        "eventStartDate": "2026-01-25",
        "deadline": "2026-02-01",
        "bookmarkedAt": "2026-01-16T10:00:00Z",
        "links": {
          "detail": "/student/jobs/4001"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 8
  },
  "operation": "job_bookmarks_list"
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
  "operation": "job_bookmarks_list"
}
~~~

---

## 5. ブックマーク追加

パス: `/api/v1/job/bookmarks`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 求人をブックマークする（job_bookmarks に upsert）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateJobBookmarkRequest`

~~~json
{
  "jobId": 4001
}
~~~

レスポンスモデル: `ApiResponse[JobBookmarkResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Bookmarked",
  "data": {
    "jobId": 4001,
    "bookmarkedAt": "2026-01-16T10:00:00Z"
  },
  "operation": "job_bookmark_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（求人なし） / 409（すでに登録） / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Job not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_bookmark_create"
}
~~~

---

## 6. ブックマーク解除

パス: `/api/v1/job/bookmarks/{job_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 学生  
説明: 求人のブックマークを解除する。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `job_id`: BIGINT

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[JobBookmarkDeleteResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Unbookmarked",
  "data": {
    "jobId": 4001,
    "deletedAt": "2026-01-16T10:01:00Z"
  },
  "operation": "job_bookmark_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（ブックマークなし） / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Bookmark not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_bookmark_delete"
}
~~~

---

## 7. 求人アンケート取得

パス: `/api/v1/job/jobs/{job_id}/survey`  
メソッド: `GET`  
認証: 必要  
説明: 求人のアンケート（質問定義）を返す。学生は公開範囲内のみ参照可。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `job_id`: BIGINT

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[JobSurveyView]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "surveyId": 6001,
    "jobId": 4001,
    "title": "参加アンケート",
    "isRequired": true,
    "questions": [
      {
        "key": "q1",
        "type": "single_select",
        "label": "満足度",
        "required": true,
        "options": ["1", "2", "3", "4", "5"]
      },
      {
        "key": "q2",
        "type": "text",
        "label": "感想",
        "required": false
      }
    ]
  },
  "operation": "job_survey_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Survey not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_survey_get"
}
~~~

---

## 8. 求人アンケート回答

パス: `/api/v1/job/jobs/{job_id}/survey/response`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 求人アンケートへ回答する（survey_responses を作成）。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `job_id`: BIGINT

リクエストモデル: `SubmitJobSurveyResponseRequest`

~~~json
{
  "surveyId": 6001,
  "answers": {
    "q1": "5",
    "q2": "とても良かったです"
  }
}
~~~

レスポンスモデル: `ApiResponse[SurveyResponseResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Submitted",
  "data": {
    "responseId": 90001,
    "surveyId": 6001,
    "submittedAt": "2026-01-16T10:05:00Z"
  },
  "operation": "job_survey_submit"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（surveyなし） / 409（既回答） / 422（回答不正） / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Already submitted",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "surveyId", "reason": "already_submitted" }
    ]
  },
  "operation": "job_survey_submit"
}
~~~

---

## 9. 求人一覧（管理）

パス: `/api/v1/job/admin/jobs`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 管理用の求人一覧を取得する。作成者や状態、公開期間、公開範囲を含む。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ（任意）:
- `q`: string（会社名/求人名）
- `status`: int（0/1/9）
- `teacherAccountId`: BIGINT（管理者のみ任意：作成者絞り込み）
- `publishFrom`: string（YYYY-MM-DD）
- `publishTo`: string（YYYY-MM-DD）
- `page`: int（default=1）
- `pageSize`: int（default=20）
- `sort`: string（例: `updatedAt:desc`）

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[Paged[AdminJobRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "jobId": 4001,
        "title": "会社説明会",
        "companyName": "株式会社AAA",
        "status": 1,
        "publishScope": 1,
        "publishedAt": "2026-01-10T00:00:00Z",
        "closedAt": null,
        "teacherAccountId": 2001,
        "updatedAt": "2026-01-16T09:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 30
  },
  "operation": "job_admin_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

~~~json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "job_admin_list"
}
~~~

---

## 10. 求人作成

パス: `/api/v1/job/admin/jobs`  
メソッド: `POST`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 求人を作成する。初期 status は 0（下書き）を推奨。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateJobRequest`

~~~json
{
  "companyId": 2001,
  "title": "会社説明会",
  "jobType": 0,
  "format": 1,
  "place": "Zoom",
  "eventStartDate": "2026-01-25",
  "deadline": "2026-02-01",
  "target": 1,
  "content": "募集要項...",
  "tagIds": [101, 201],
  "todoTemplateId": 7001
}
~~~

レスポンスモデル: `ApiResponse[CreateJobResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Created",
  "data": {
    "jobId": 4002,
    "status": 0,
    "createdAt": "2026-01-16T10:10:00Z"
  },
  "operation": "job_admin_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（company/tag/templateなし） / 422 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Company not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_admin_create"
}
~~~

---

## 11. 求人更新

パス: `/api/v1/job/admin/jobs/{job_id}`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 求人を更新する。下書き/公開中いずれも更新可能だが、運用上の制限（公開中の一部項目ロック等）は実装方針に従う。

リクエストヘッダー:
- `Content-Type: application/json`

パスパラメータ:
- `job_id`: BIGINT

リクエストモデル: `UpdateJobRequest`（部分更新）

~~~json
{
  "title": "会社説明会（改）",
  "deadline": "2026-02-03",
  "tagIds": [101, 202],
  "content": "更新した募集要項..."
}
~~~

レスポンスモデル: `ApiResponse[UpdateJobResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "jobId": 4001,
    "updatedAt": "2026-01-16T10:12:00Z"
  },
  "operation": "job_admin_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409（募集終了などで更新不可） / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Job cannot be updated in current status",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "status", "reason": "closed" }
    ]
  },
  "operation": "job_admin_update"
}
~~~

---

## 12. 求人削除

パス: `/api/v1/job/admin/jobs/{job_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 求人を削除する。参照整合性のため、実装は論理削除（deleted_at）を推奨（DB定義に合わせる）。

リクエストヘッダー:
- `Accept: application/json`

リクエストモデル: なし

~~~json
{}
~~~

レスポンスモデル: `ApiResponse[DeleteJobResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Deleted",
  "data": {
    "jobId": 4001,
    "deletedAt": "2026-01-16T10:15:00Z"
  },
  "operation": "job_admin_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409（応募実績があり削除不可等） / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Job cannot be deleted",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "jobId", "reason": "has_related_activities" }
    ]
  },
  "operation": "job_admin_delete"
}
~~~

---

## 13. 求人コピー

パス: `/api/v1/job/admin/jobs/{job_id}/copy`  
メソッド: `POST`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 指定求人をコピーして新規求人（下書き）として作成する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CopyJobRequest`（任意）

~~~json
{
  "title": "会社説明会（コピー）",
  "copyScope": true,
  "copySurvey": true,
  "copyTodoTemplate": true
}
~~~

レスポンスモデル: `ApiResponse[CopyJobResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Copied",
  "data": {
    "sourceJobId": 4001,
    "newJobId": 4010,
    "status": 0,
    "createdAt": "2026-01-16T10:20:00Z"
  },
  "operation": "job_admin_copy"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Job not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_admin_copy"
}
~~~

---

## 14. 公開状態・公開期間更新

パス: `/api/v1/job/admin/jobs/{job_id}/publish`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 求人の status（0/1/9）と公開に関する日時を更新する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateJobPublishRequest`

~~~json
{
  "status": 1,
  "publishedAt": "2026-01-16T10:30:00Z",
  "closedAt": null
}
~~~

レスポンスモデル: `ApiResponse[UpdateJobPublishResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Publish status updated",
  "data": {
    "jobId": 4001,
    "status": 1,
    "publishedAt": "2026-01-16T10:30:00Z",
    "closedAt": null,
    "updatedAt": "2026-01-16T10:30:00Z"
  },
  "operation": "job_admin_publish_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409（不正な状態遷移） / 422 / 500

~~~json
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
  "operation": "job_admin_publish_update"
}
~~~

---

## 15. 公開範囲更新

パス: `/api/v1/job/admin/jobs/{job_id}/scope`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: `publish_scope` と対象クラス/学生を更新する（job_target_classes / job_target_students を同期）。

publish_scope（例）:
- 0: 全体公開
- 1: クラス指定
- 2: 学生指定

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateJobScopeRequest`

~~~json
{
  "publishScope": 1,
  "targetClassIds": [301, 302],
  "targetStudentAccountIds": []
}
~~~

レスポンスモデル: `ApiResponse[UpdateJobScopeResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Scope updated",
  "data": {
    "jobId": 4001,
    "publishScope": 1,
    "targetClassIds": [301, 302],
    "targetStudentAccountIds": [],
    "updatedAt": "2026-01-16T10:35:00Z"
  },
  "operation": "job_admin_scope_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

~~~json
{
  "success": false,
  "code": 422,
  "message": "Validation error",
  "error": {
    "type": "VALIDATION_ERROR",
    "details": [
      { "field": "publishScope", "reason": "target_required" }
    ]
  },
  "operation": "job_admin_scope_update"
}
~~~

---

## 16. レコメンド付与/解除

パス: `/api/v1/job/admin/jobs/{job_id}/recommendations`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 特定の学生に「おすすめ」を付与/解除する（job_recommendations を upsert/delete）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpsertJobRecommendationsRequest`

~~~json
{
  "action": "set",
  "targetStudentAccountIds": [1001, 1002],
  "note": "進路希望に合うため"
}
~~~

解除の場合:

~~~json
{
  "action": "unset",
  "targetStudentAccountIds": [1002]
}
~~~

レスポンスモデル: `ApiResponse[JobRecommendationsResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Recommendations updated",
  "data": {
    "jobId": 4001,
    "action": "set",
    "affectedCount": 2,
    "updatedAt": "2026-01-16T10:40:00Z"
  },
  "operation": "job_admin_recommendations"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Job not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_admin_recommendations"
}
~~~

---

## 17. ToDoテンプレ管理（取得/更新）

パス: `/api/v1/job/admin/todo-templates/{template_id}`  
メソッド: `GET` / `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: todo_templates と todo_steps を管理する。Activity が生成に使うため、テンプレの整合性が重要。

### 17-1. 取得（GET）

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[TodoTemplateDetail]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "templateId": 7001,
    "name": "説明会テンプレ",
    "description": "説明会参加前後の標準タスク",
    "steps": [
      {
        "stepId": 7101,
        "name": "履歴書ドラフト作成",
        "description": "テンプレに沿って作成",
        "stepOrder": 1,
        "daysDeadline": 3,
        "isVerificationRequired": false
      }
    ]
  },
  "operation": "job_admin_todo_template_get"
}
~~~

### 17-2. 更新（PATCH）

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateTodoTemplateRequest`

~~~json
{
  "name": "説明会テンプレ（改）",
  "description": "更新版",
  "steps": [
    {
      "stepId": 7101,
      "name": "履歴書ドラフト作成",
      "description": "テンプレに沿って作成",
      "stepOrder": 1,
      "daysDeadline": 3,
      "isVerificationRequired": false
    },
    {
      "stepId": null,
      "name": "面談予約",
      "description": "教員面談を予約する",
      "stepOrder": 2,
      "daysDeadline": 7,
      "isVerificationRequired": true
    }
  ]
}
~~~

レスポンスモデル: `ApiResponse[UpdateTodoTemplateResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "templateId": 7001,
    "updatedAt": "2026-01-16T10:45:00Z"
  },
  "operation": "job_admin_todo_template_update"
}
~~~

エラーレスポンス（共通）:
- 401 / 403 / 404 / 409（stepOrder重複など） / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Duplicate stepOrder",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "steps.stepOrder", "reason": "duplicated" }
    ]
  },
  "operation": "job_admin_todo_template_update"
}
~~~

---

## 18. ToDoテンプレ割当（求人）

パス: `/api/v1/job/admin/jobs/{job_id}/todo-template`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 求人に `todo_template_id` を割り当てる。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `AssignTodoTemplateToJobRequest`

~~~json
{
  "todoTemplateId": 7001
}
~~~

レスポンスモデル: `ApiResponse[AssignTodoTemplateToJobResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Assigned",
  "data": {
    "jobId": 4001,
    "todoTemplateId": 7001,
    "updatedAt": "2026-01-16T10:50:00Z"
  },
  "operation": "job_admin_job_todo_template_assign"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Todo template not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "job_admin_job_todo_template_assign"
}
~~~

---

## 19. タグ管理（作成/更新/削除）

パス: `/api/v1/job/admin/tags`  
メソッド: `POST` / `PATCH` / `DELETE`  
認証: 必要  
対象ロール: 教員/管理者  
説明: job_tags を管理する（絞り込み・求人作成に利用）。

### 19-1. 作成（POST）

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateJobTagRequest`

~~~json
{
  "name": "リモート可",
  "type": 3
}
~~~

レスポンスモデル: `ApiResponse[CreateJobTagResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Created",
  "data": {
    "tagId": 999,
    "createdAt": "2026-01-16T10:55:00Z"
  },
  "operation": "job_admin_tag_create"
}
~~~

### 19-2. 更新（PATCH）

リクエストモデル: `UpdateJobTagRequest`

~~~json
{
  "tagId": 999,
  "name": "リモートワーク可",
  "type": 3
}
~~~

レスポンスモデル: `ApiResponse[UpdateJobTagResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "tagId": 999,
    "updatedAt": "2026-01-16T10:56:00Z"
  },
  "operation": "job_admin_tag_update"
}
~~~

### 19-3. 削除（DELETE）

クエリパラメータ:
- `tagId`: BIGINT

レスポンスモデル: `ApiResponse[DeleteJobTagResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Deleted",
  "data": {
    "tagId": 999,
    "deletedAt": "2026-01-16T10:57:00Z"
  },
  "operation": "job_admin_tag_delete"
}
~~~

エラーレスポンス（共通）:
- 401 / 403 / 404 / 409（求人に紐付いていて削除不可） / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Tag is in use",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "tagId", "reason": "referenced_by_jobs" }
    ]
  },
  "operation": "job_admin_tag_delete"
}
~~~

---

## 20. 企業管理（一覧/作成/更新/削除）

パス: `/api/v1/job/admin/companies`  
メソッド: `GET` / `POST` / `PATCH` / `DELETE`  
認証: 必要  
対象ロール: 教員/管理者  
説明: companies を管理する（求人作成で参照する）。

### 20-1. 一覧（GET）

クエリパラメータ（任意）:
- `q`: string（name部分一致）
- `page`: int（default=1）
- `pageSize`: int（default=20）

レスポンスモデル: `ApiResponse[Paged[CompanyRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "companyId": 2001,
        "name": "株式会社AAA",
        "websiteUrl": "https://example.com",
        "address": "東京都..."
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 15
  },
  "operation": "job_admin_companies_list"
}
~~~

### 20-2. 作成（POST）

リクエストモデル: `CreateCompanyRequest`

~~~json
{
  "name": "株式会社BBB",
  "websiteUrl": "https://bbb.example.com",
  "address": "大阪府..."
}
~~~

レスポンスモデル: `ApiResponse[CreateCompanyResult]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Created",
  "data": {
    "companyId": 2010,
    "createdAt": "2026-01-16T11:00:00Z"
  },
  "operation": "job_admin_company_create"
}
~~~

### 20-3. 更新（PATCH）

リクエストモデル: `UpdateCompanyRequest`

~~~json
{
  "companyId": 2010,
  "websiteUrl": "https://bbb.example.com/careers"
}
~~~

レスポンスモデル: `ApiResponse[UpdateCompanyResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Updated",
  "data": {
    "companyId": 2010,
    "updatedAt": "2026-01-16T11:01:00Z"
  },
  "operation": "job_admin_company_update"
}
~~~

### 20-4. 削除（DELETE）

クエリパラメータ:
- `companyId`: BIGINT

レスポンスモデル: `ApiResponse[DeleteCompanyResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Deleted",
  "data": {
    "companyId": 2010,
    "deletedAt": "2026-01-16T11:02:00Z"
  },
  "operation": "job_admin_company_delete"
}
~~~

エラーレスポンス（共通）:
- 401 / 403 / 404 / 409（求人が紐付いていて削除不可） / 422 / 500

~~~json
{
  "success": false,
  "code": 409,
  "message": "Company is in use",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "companyId", "reason": "referenced_by_jobs" }
    ]
  },
  "operation": "job_admin_company_delete"
}
~~~