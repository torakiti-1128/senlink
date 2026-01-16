# API設計：Backend For Frontend（BFFAPI）

## 概要
BFFAPI は **Next.js 画面に最適化した集約API** を提供する。
- 役割：複数サービス（CoreAPI内の Auth / School / Job / Activity / Request / Notification 等）を **呼び分けて集計・結合**し、画面に必要な形で返す
- 前提：認証は **Cookie(HttpOnly)+JWT**（共通仕様に従う）
- DB：BFFAPI は **DBへ直接アクセスしない**（CoreAPI 経由で取得）
- 原則：
  - CoreAPI の正規モデルを破壊しない（BFFは“画面DTO”を返す）
  - 画面表示に必要な項目を不足なく返す（フロントで複数API結合を減らす）
  - 権限チェックは BFF でも必須（特に教員/管理者）

---

## API一覧（BFF）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 教員ダッシュボード | /api/v1/bff/teacher/dashboard | GET | 必要 | 教員/管理者 | クラス状況サマリ + 未処理件数 + 要支援抽出 + 直近ログ等を集約 |
| 2 | 教員クラス学生一覧 | /api/v1/bff/teacher/classes/{class_id}/students | GET | 必要 | 教員/管理者 | 学生一覧 + 活動状況サマリ（応募数/未対応タスク等）を結合 |
| 3 | 教員学生詳細 | /api/v1/bff/teacher/students/{student_account_id} | GET | 必要 | 教員/管理者 | 学生プロフィール + 応募/ToDo + 申請履歴を集約 |
| 4 | 学生ホーム | /api/v1/bff/student/home | GET | 必要 | 学生 | 求人おすすめ + 直近イベント + 要対応ToDo/申請状況を集約 |
| 5 | 求人詳細 | /api/v1/bff/jobs/{job_id} | GET | 必要 | 学生/教員/管理者 | 求人 + 企業 + タグ + 公開範囲 + アンケート概要等を集約 |
| 6 | 申請一覧 | /api/v1/bff/requests | GET | 必要 | 学生/教員/管理者 | 申請一覧に必要な検索条件を統一し、表示用DTOを返す |

---

## 1. 教員ダッシュボード集約

パス: /api/v1/bff/teacher/dashboard  
メソッド: GET  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員ダッシュボード表示に必要な情報を集約して返す（クラス状況、未処理件数、要支援学生、直近ログ、指導予定など）

リクエストヘッダー:
- Accept: application/json

クエリパラメータ:
- `classId` (optional) : 対象クラスID（未指定なら「前回選択」または「担当の先頭」）
- `rangeDays` (optional, default=30) : KPI推移の対象期間（7/30/90など）
- `tz` (optional, default="UTC") : 表示用タイムゾーン（返却はISO8601だが、日付集計の境界に利用）


リクエストモデル: なし

レスポンスモデル: ApiResponse[TeacherDashboard]

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "filters": {
      "classId": 101,
      "rangeDays": 30,
      "timezone": "UTC"
    },
    "classes": [
      { "id": 101, "name": "A組", "fiscalYear": 2026, "grade": 3, "departmentName": "情報" },
      { "id": 102, "name": "B組", "fiscalYear": 2026, "grade": 3, "departmentName": "情報" }
    ],
    "classSummary": {
      "classId": 101,
      "totalStudents": 40,
      "jobHuntingStudents": 35,
      "offerStudents": 10,
      "offerRate": 0.2857,
      "activeStudents": 30,
      "activityRate": 0.8571,
      "noOfferStudents": 25,
      "trend": {
        "rangeDays": 30,
        "offerRateSeries": [
          { "date": "2025-12-18", "value": 0.20 },
          { "date": "2026-01-01", "value": 0.25 },
          { "date": "2026-01-16", "value": 0.2857 }
        ],
        "activityRateSeries": [
          { "date": "2025-12-18", "value": 0.80 },
          { "date": "2026-01-01", "value": 0.83 },
          { "date": "2026-01-16", "value": 0.8571 }
        ],
        "pendingTotalSeries": [
          { "date": "2025-12-18", "value": 12 },
          { "date": "2026-01-01", "value": 18 },
          { "date": "2026-01-16", "value": 9 }
        ]
      }
    },
    "pendingRequests": {
      "documents": 3,
      "interviews": 5,
      "offers": 1,
      "total": 9
    },
    "studentAlerts": {
      "inactiveLogin": [
        {
          "studentId": 9001,
          "studentNumber": "1234567",
          "name": "山田 太郎",
          "lastLoginAt": "2025-12-20T09:00:00Z",
          "daysSinceLastLogin": 27,
          "priorityScore": 82,
          "priorityReasons": ["未ログイン日数が多い"]
        }
      ],
      "inactiveActivity": [
        {
          "studentId": 9002,
          "studentNumber": "2345678",
          "name": "佐藤 花子",
          "lastActivityAt": "2025-12-25T09:00:00Z",
          "daysSinceLastActivity": 22,
          "priorityScore": 74,
          "priorityReasons": ["活動停止が継続"]
        }
      ],
      "overdueTodos": [
        {
          "studentId": 9003,
          "studentNumber": "3456789",
          "name": "鈴木 次郎",
          "overdueTodoCount": 4,
          "nearestDeadline": "2026-01-10",
          "priorityScore": 91,
          "priorityReasons": ["期限超過ToDoが多い", "最短期限が過去日付"]
        }
      ]
    },
    "activityFeed": [
      {
        "type": "REQUEST_APPROVED",
        "occurredAt": "2026-01-16T09:20:00Z",
        "studentId": 9001,
        "studentName": "山田 太郎",
        "jobId": 3001,
        "companyName": "株式会社サンプル",
        "jobTitle": "インターン",
        "link": "/teacher/requests/12001"
      }
    ],
    "interviewSchedule": {
      "pendingCount": 5,
      "upcomingConfirmed": [
        {
          "id": 80001,
          "studentId": 9002,
          "studentName": "佐藤 花子",
          "teacherName": "田中 教員",
          "scheduledAt": "2026-01-18T04:00:00Z",
          "meetingPlace": "201号室",
          "link": "/teacher/interviews/80001"
        }
      ]
    },
    "teacherKpi": {
      "pendingCount": 9,
      "pendingByType": { "documents": 3, "interviews": 5, "offers": 1 },
      "avgResponseHours": { "documents": 18.2, "interviews": 9.5, "offers": 22.1 },
      "delayed": [
        {
          "type": "INTERVIEW",
          "id": 12001,
          "studentName": "山田 太郎",
          "requestedAt": "2026-01-14T01:00:00Z",
          "elapsedHours": 56.3,
          "link": "/teacher/requests/12001"
        }
      ]
    },
    "drilldowns": {
      "endpoints": {
        "alerts_inactive_login": "/api/v1/teacher/students?classId=101&alert=inactiveLogin",
        "alerts_overdue_todos": "/api/v1/teacher/students?classId=101&alert=overdueTodos",
        "pending_requests": "/api/v1/teacher/requests?classId=101&status=pending"
      }
    },
    "meta": {
      "generatedAt": "2026-01-16T10:00:00Z",
      "ttlSeconds": 30
    }
  },
  "operation": "teacher_dashboard_view"
}
~~~
---

## 2. 教員：クラス学生一覧（集約）

パス: /api/v1/bff/teacher/classes/{class_id}/students  
メソッド: GET  
認証: 必要  
対象ロール: 教員/管理者  
説明: 学生一覧に「活動サマリ（応募数/未対応ToDo/申請状況など）」を結合して返す

リクエストヘッダー:
- Accept: application/json

パスパラメータ:
- class_id: BIGINT

クエリパラメータ:
- q (任意): string（氏名/学籍番号部分一致）
- status (任意): string（例: active / inactive / all）
- sort (任意): string（例: nameKana:asc, overdueTodos:desc）
- page (任意): int（default=1）
- pageSize (任意): int（default=20）

レスポンスモデル: ApiResponse[Paged[TeacherStudentRow]]

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "studentAccountId": 1001,
        "studentId": 5001,
        "studentNumber": "1234567",
        "name": "山田 太郎",
        "nameKana": "やまだ たろう",
        "className": "2年A組",
        "isJobHunting": true,
        "summary": {
          "applications": 3,
          "activeActivities": 2,
          "overdueTodos": 1,
          "pendingRequests": 1,
          "lastLoginAt": "2026-01-16T09:00:00Z"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 40
  },
  "operation": "bff_teacher_class_students"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

~~~json
{
  "success": false,
  "code": 404,
  "message": "Class not found",
  "error": { "type": "NOT_FOUND", "details": [] },
  "operation": "bff_teacher_class_students"
}
~~~

---

## 3. 教員：学生詳細（集約）

パス: /api/v1/bff/teacher/students/{student_account_id}  
メソッド: GET  
認証: 必要  
対象ロール: 教員/管理者  
説明: 学生詳細画面用に、プロフィール + 応募/ToDo + 申請履歴を集約する

リクエストヘッダー:
- Accept: application/json

パスパラメータ:
- student_account_id: BIGINT（accounts.id）

クエリパラメータ:
- include (任意): string（例: profile,activities,requests,todos / default=all）
- rangeDays (任意): int（default=90）

レスポンスモデル: ApiResponse[TeacherStudentDetail]

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "profile": {
      "studentAccountId": 1001,
      "studentId": 5001,
      "studentNumber": "1234567",
      "name": "山田 太郎",
      "classId": 301,
      "className": "2年A組",
      "admissionYear": 2024,
      "isJobHunting": true,
      "profileData": {
        "desiredJobTypes": ["バックエンド"],
        "portfolioUrl": "https://example.com"
      }
    },
    "activities": [
      {
        "activityId": 7001,
        "jobId": 4001,
        "jobTitle": "株式会社AAA 会社説明会",
        "status": 0,
        "todoProgress": { "done": 3, "total": 7 }
      }
    ],
    "todos": [
      {
        "activityTodoId": 8001,
        "activityId": 7001,
        "name": "履歴書作成",
        "status": 0,
        "deadline": "2026-01-20"
      }
    ],
    "requests": [
      {
        "requestId": 9001,
        "type": 1,
        "status": 1,
        "title": "面接予約申請",
        "submittedAt": "2026-01-16T10:00:00Z",
        "latestComment": {
          "commentType": 0,
          "body": "候補日は1/22〜1/24です",
          "createdAt": "2026-01-16T10:01:00Z"
        }
      }
    ]
  },
  "operation": "bff_teacher_student_detail"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（担当外学生）
- 404: Not Found（学生なし）
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 403,
  "message": "Forbidden",
  "error": {
    "type": "FORBIDDEN",
    "details": [
      { "field": "studentAccountId", "reason": "not_assigned" }
    ]
  },
  "operation": "bff_teacher_student_detail"
}
~~~

---

## クエリパラメータ（検索/ソート/ページング）
- `status` (optional): `all | upcoming | past | withdrawn | withdrawalRequested`
- `type` (optional): `seminar | intern | exam`（または数値enumでもOK）
- `q` (optional): フリーワード（会社名/案件名）
- `sort` (optional, default="eventStartAt:asc"): `eventStartAt:asc|desc`, `progressRate:desc`, `deadline:asc`
- `page` (optional, default=1)
- `pageSize` (optional, default=20)
- `tz` (optional, default="UTC")

---

## レスポンスモデル
`ApiResponse[StudentActivitiesOverviewView]`

### Response Example
~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "filters": {
      "status": "all",
      "type": null,
      "q": null,
      "sort": "eventStartAt:asc",
      "page": 1,
      "pageSize": 20,
      "timezone": "UTC"
    },
    "summary": {
      "counts": { "seminar": 3, "intern": 2, "exam": 1, "offer": 0 },
      "nextEvent": {
        "jobId": 3001,
        "companyName": "株式会社サンプル",
        "title": "会社説明会",
        "startAt": "2026-01-20T01:00:00Z"
      },
      "urgentTodoCount": 2,
      "overallProgressRate": 0.42,
      "notifications": {
        "hasUnread": true,
        "unreadCount": 5
      },
      "trend": {
        "rangeDays": 30,
        "progressRateSeries": [
          { "date": "2025-12-18", "value": 0.30 },
          { "date": "2026-01-01", "value": 0.38 },
          { "date": "2026-01-16", "value": 0.42 }
        ]
      }
    },
    "items": [
      {
        "activityId": 70001,
        "jobId": 3001,
        "companyName": "株式会社サンプル",
        "jobTitle": "会社説明会",
        "jobType": "seminar",
        "eventStartAt": "2026-01-20T01:00:00Z",
        "cancelDeadline": "2026-01-19",
        "status": "normal",
        "progressRate": 0.25,
        "todoCounts": { "done": 1, "total": 4 },
        "nearestTodoDeadline": "2026-01-17",
        "currentTodo": {
          "todoId": 91001,
          "title": "履歴書ドラフト作成",
          "content": "テンプレに沿って作成",
          "deadline": "2026-01-17",
          "isUrgent": true,
          "daysToDeadline": 1,
          "priorityReason": "期限が近い"
        },
        "canWithdraw": true,
        "links": {
          "detail": "/student/activities/70001",
          "withdraw": "/student/activities/70001/withdraw"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 12,
    "meta": {
      "generatedAt": "2026-01-16T10:00:00Z",
      "ttlSeconds": 30
    }
  },
  "operation": "student_activities_overview"
}
~~~

---

## エラーレスポンス
- 401: Unauthorized（未ログイン/トークン期限切れ）
- 403: Forbidden（学内IP外）
- 422: Unprocessable Entity（クエリ不正）
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": {
    "type": "AUTH_REQUIRED",
    "details": []
  },
  "operation": "student_activities_overview"
}
~~~

---

## 5. 求人詳細（集約）

パス: /api/v1/bff/jobs/{job_id}  
メソッド: GET  
認証: 必要  
対象ロール: 学生/教員/管理者  
説明: 求人詳細画面用に、求人 + 企業 + タグ + 公開範囲 + アンケート概要を集約する

リクエストヘッダー:
- Accept: application/json

パスパラメータ:
- job_id: BIGINT

レスポンスモデル: ApiResponse[JobDetailView]

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
      "type": 0,
      "format": 1,
      "place": "Zoom",
      "eventStartDate": "2026-01-25",
      "deadline": "2026-02-01",
      "target": 1,
      "content": "募集要項..."
    },
    "company": {
      "companyId": 2001,
      "name": "株式会社AAA",
      "url": "https://example.com"
    },
    "tags": [
      { "tagId": 101, "name": "バックエンド", "type": 0 }
    ],
    "visibility": {
      "target": 1,
      "targetClasses": [301, 302],
      "targetStudents": []
    },
    "survey": {
      "surveyId": 6001,
      "title": "参加アンケート",
      "hasQuestions": true
    }
  },
  "operation": "bff_job_detail"
}
~~~

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（公開対象外）
- 404: Not Found
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 403,
  "message": "Job is not visible for this user",
  "error": { "type": "FORBIDDEN", "details": [] },
  "operation": "bff_job_detail"
}
~~~

---

## 6. 申請一覧（集約）

パス: /api/v1/bff/requests  
メソッド: GET  
認証: 必要  
対象ロール: 学生/教員/管理者  
説明: 申請一覧表示用に、検索・並び替え・ページングを統一し、表示用DTOを返す  
- 学生：自分の申請のみ
- 教員：担当範囲の申請（クラス/学生）＋自分が reviewer のもの
- 管理者：全件可（運用方針に合わせて調整）

リクエストヘッダー:
- Accept: application/json

クエリパラメータ:
- type (任意): 0|1|2
- status (任意): 0|1|2|3
- q (任意): string（タイトル/学生名/学籍番号）
- from (任意): string（ISO8601）
- to (任意): string（ISO8601）
- page (任意): int（default=1）
- pageSize (任意): int（default=20）
- sort (任意): string（例: submittedAt:desc）

レスポンスモデル: ApiResponse[Paged[RequestRow]]

~~~json
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
        "title": "面接予約申請",
        "requester": {
          "accountId": 1001,
          "name": "山田 太郎",
          "className": "2年A組"
        },
        "reviewerAccountId": null,
        "submittedAt": "2026-01-16T10:00:00Z",
        "resolvedAt": null,
        "latestComment": {
          "commentType": 0,
          "body": "候補日は1/22〜1/24です",
          "createdAt": "2026-01-16T10:01:00Z"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 12
  },
  "operation": "bff_requests_list"
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
      { "field": "sort", "reason": "invalid_format" }
    ]
  },
  "operation": "bff_requests_list"
}
~~~

---

## 補足：BFFで集約すべき粒度（設計判断の基準）
- 画面が「1回の描画」で複数サービスを必要とする場合はBFFへ寄せる  
  例：教員ダッシュボード、学生詳細、求人詳細（タグ/企業/公開範囲）
- トランザクション更新（作成/更新）は **原則 CoreAPI に寄せる**（BFFは薄く）  
  例外：画面都合で “複数更新を同時実行” したい場合は、BFFでオーケストレーションする
- 権限・可視性は BFF 側も必ず再確認（CoreAPI任せにしない）