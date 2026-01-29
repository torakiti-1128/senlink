# API設計：Schoolサービス

## 概要

Schoolサービスは、学校情報（学科・クラス）および学校ユーザー（学生・教員プロフィール）を提供する。  
本サービスは「初回設定（初期プロフィール必須入力）」を支えるため、**学生/教員の初回プロフィール登録**も責務として持つ。

- 認証方式：Cookie（HttpOnly） + JWT（共通仕様に従う）
- ロール：0=学生 / 1=教員 / 2=管理者
- データモデル（抜粋）
  - 学生：students（account_id, class_id, student_number, name, date_of_birth, gender, admission_year, is_job_hunting, profile_data）
  - 教員：teachers（account_id, name, title, office_location, profile_data）
  - 学科/クラス：departments, classes, class_teachers
- 初回設定：ログイン後にロール判定し、初期プロフィール入力を必須とする前提

---

## API一覧（School）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 学科一覧取得 | /api/v1/school/departments | GET | 必要 | ALL | 学科マスタ一覧を返す |
| 2 | クラス一覧取得 | /api/v1/school/classes | GET | 必要 | ALL | クラス一覧（年度/学科で絞り込み可）を返す |
| 3 | 初回プロフィール登録（学生） | /api/v1/school/onboarding/student-profile | POST | 必要 | 学生 | students を作成（初回設定用） |
| 4 | 初回プロフィール登録（教員） | /api/v1/school/onboarding/teacher-profile | POST | 必要 | 教員 | teachers を作成（初回設定用） |
| 5 | 自分の学生プロフィール取得 | /api/v1/school/students/me | GET | 必要 | 学生 | 学生の基本情報+所属を返す（編集不可項目含む） |
| 6 | 自分の学生就活プロフィール更新 | /api/v1/school/students/me/profile | PATCH | 必要 | 学生 | profile_data を更新（希望職種/PR等） |
| 7 | 自分の活動状況更新 | /api/v1/school/students/me/job-hunting | PATCH | 必要 | 学生 | is_job_hunting を更新 |
| 8 | 自分の教員プロフィール取得 | /api/v1/school/teachers/me | GET | 必要 | 教員/管理者 | 教員の公開情報を返す |
| 9 | 自分の教員プロフィール更新 | /api/v1/school/teachers/me/profile | PATCH | 必要 | 教員/管理者 | 役職/専門/オフィス等を更新 |
| 10 | 担当クラス一覧（教員） | /api/v1/school/teacher/classes | GET | 必要 | 教員/管理者 | class_teachers に基づき担当クラスを返す |
| 11 | クラス所属学生一覧 | /api/v1/school/classes/{class_id}/students | GET | 必要 | 教員/管理者 | 担当クラスの学生一覧（検索/ページング）を返す |
| 12 | 学科作成（管理） | /api/v1/school/admin/departments | POST | 必要 | 管理者 | 学科マスタ作成 |
| 13 | 学科更新（管理） | /api/v1/school/admin/departments/{department_id} | PATCH | 必要 | 管理者 | 学科名/コード更新 |
| 14 | 学科削除（管理） | /api/v1/school/admin/departments/{department_id} | DELETE | 必要 | 管理者 | 学科削除（参照整合性により制限あり） |
| 15 | クラス作成（管理） | /api/v1/school/admin/classes | POST | 必要 | 管理者 | 年度別クラス作成 |
| 16 | クラス更新（管理） | /api/v1/school/admin/classes/{class_id} | PATCH | 必要 | 管理者 | 学科/年度/学年/クラス名更新 |
| 17 | クラス削除（管理） | /api/v1/school/admin/classes/{class_id} | DELETE | 必要 | 管理者 | クラス削除（所属学生/担当教員がいる場合は制限） |
| 18 | 担当教員割当（管理） | /api/v1/school/admin/classes/{class_id}/teachers | PUT | 必要 | 管理者 | 担当教員を追加/更新（role含む） |
| 19 | 担当教員解除（管理） | /api/v1/school/admin/classes/{class_id}/teachers/{teacher_id} | DELETE | 必要 | 管理者 | 担当教員を解除 |
| 20 | 所属学生CSV一括操作（管理） | /api/v1/school/admin/classes/{class_id}/students/csv | POST | 必要 | 管理者 | 名簿CSVで所属変更を一括実行 |

---

## 1. 学科一覧取得

パス: `/api/v1/school/departments`  
メソッド: `GET`  
認証: 必要  
説明: 学科（departments）一覧を返す。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ: なし

レスポンスモデル: `ApiResponse[DepartmentList]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      { "departmentId": 1, "name": "情報", "code": "INF" },
      { "departmentId": 2, "name": "機械", "code": "MEC" }
    ]
  },
  "operation": "school_departments_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

---

## 2. クラス一覧取得

パス: `/api/v1/school/classes`  
メソッド: `GET`  
認証: 必要  
説明: クラス（classes）一覧を返す。年度/学科で絞り込み可能。

リクエストヘッダー:
- `Accept: application/json`

クエリパラメータ:
- `departmentId` (optional): BIGINT
- `fiscalYear` (optional): int
- `grade` (optional): int

レスポンスモデル: `ApiResponse[ClassList]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "classId": 301,
        "departmentId": 1,
        "departmentName": "情報",
        "fiscalYear": 2026,
        "grade": 3,
        "name": "A組"
      }
    ]
  },
  "operation": "school_classes_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 422 / 500

---

## 3. 初回プロフィール登録（学生）

パス: `/api/v1/school/onboarding/student-profile`  
メソッド: `POST`  
認証: 必要  
対象ロール: 学生  
説明: 初回設定で学生プロフィール（students）を作成する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateStudentProfileOnboardingRequest`

~~~json
{
  "classId": 301,
  "studentNumber": "1234567",
  "name": "山田 太郎",
  "nameKana": "やまだ たろう",
  "dateOfBirth": "2007-04-01",
  "gender": 1,
  "admissionYear": 2026
}
~~~

レスポンスモデル: `ApiResponse[StudentProfileCreated]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Student profile created",
  "data": {
    "studentId": 5001,
    "accountId": 1001,
    "classId": 301,
    "studentNumber": "1234567",
    "isJobHunting": true
  },
  "operation": "school_onboarding_student_profile_create"
}
~~~

エラーレスポンス:
- 400: Bad Request
- 401: Unauthorized
- 403: Forbidden（ロール不一致 / 学内IP外）
- 409: Conflict（studentNumber重複、または既にプロフィール作成済み）
- 422: Unprocessable Entity（バリデーション不正）
- 500: Internal Server Error

~~~json
{
  "success": false,
  "code": 409,
  "message": "Student profile already exists",
  "error": {
    "type": "CONFLICT",
    "details": [
      { "field": "accountId", "reason": "already_initialized" }
    ]
  },
  "operation": "school_onboarding_student_profile_create"
}
~~~

---

## 4. 初回プロフィール登録（教員）

パス: `/api/v1/school/onboarding/teacher-profile`  
メソッド: `POST`  
認証: 必要  
対象ロール: 教員  
説明: 初回設定で教員プロフィール（teachers）を作成する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateTeacherProfileOnboardingRequest`

~~~json
{
  "name": "田中 教員",
  "nameKana": "たなか きょういん",
  "title": "担任",
  "officeLocation": "201号室",
  "profileData": {
    "specialties": ["キャリア支援", "面接対策"],
    "bio": "キャリアセンター担当です。"
  }
}
~~~

レスポンスモデル: `ApiResponse[TeacherProfileCreated]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Teacher profile created",
  "data": {
    "teacherId": 7001,
    "accountId": 2001
  },
  "operation": "school_onboarding_teacher_profile_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 409 / 422 / 500

---

## 5. 自分の学生プロフィール取得

パス: `/api/v1/school/students/me`  
メソッド: `GET`  
認証: 必要  
対象ロール: 学生  
説明: 学生の基本情報を返す（編集不可項目ありを想定し、編集可否ヒントも返す）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[StudentMe]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "studentId": 5001,
    "accountId": 1001,
    "studentNumber": "1234567",
    "name": "山田 太郎",
    "nameKana": "やまだ たろう",
    "class": {
      "classId": 301,
      "name": "A組",
      "grade": 3,
      "fiscalYear": 2026,
      "department": { "departmentId": 1, "name": "情報", "code": "INF" }
    },
    "dateOfBirth": "2007-04-01",
    "gender": 1,
    "admissionYear": 2026,
    "isJobHunting": true,
    "profileData": {
      "desiredJobTypes": ["バックエンド"],
      "desiredLocations": ["東京"],
      "selfPr": "粘り強く改善できます。",
      "qualifications": ["基本情報技術者"],
      "portfolioUrl": "https://example.com"
    },
    "editable": {
      "basicInfo": false,
      "jobProfile": true,
      "jobHuntingStatus": true
    }
  },
  "operation": "school_students_me_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（プロフィール未作成）/ 500

---

## 6. 自分の学生就活プロフィール更新

パス: `/api/v1/school/students/me/profile`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 学生  
説明: 就活プロフィールを更新する（profile_dataの更新）。希望職種、希望勤務地、自己PR、資格、ポートフォリオURL等を想定。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateStudentJobProfileRequest`

~~~json
{
  "profileData": {
    "desiredJobTypes": ["バックエンド"],
    "desiredLocations": ["東京", "神奈川"],
    "selfPr": "継続的に改善できます。",
    "qualifications": ["基本情報技術者"],
    "portfolioUrl": "https://example.com"
  }
}
~~~

レスポンスモデル: `ApiResponse[UpdateStudentJobProfileResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Profile updated",
  "data": {
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "school_students_me_profile_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

---

## 7. 自分の活動状況更新

パス: `/api/v1/school/students/me/job-hunting`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 学生  
説明: 活動状況（is_job_hunting）を更新する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateJobHuntingStatusRequest`

~~~json
{
  "isJobHunting": false
}
~~~

レスポンスモデル: `ApiResponse[UpdateJobHuntingStatusResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Status updated",
  "data": {
    "isJobHunting": false,
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "school_students_me_job_hunting_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

---

## 8. 自分の教員プロフィール取得

パス: `/api/v1/school/teachers/me`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員の公開情報を返す（役職、専門分野、オフィス等）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[TeacherMe]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "teacherId": 7001,
    "accountId": 2001,
    "name": "田中 教員",
    "nameKana": "たなか きょういん",
    "title": "担任",
    "officeLocation": "201号室",
    "profileData": {
      "specialties": ["キャリア支援", "面接対策"],
      "bio": "キャリアセンター担当です。"
    }
  },
  "operation": "school_teachers_me_get"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

---

## 9. 自分の教員プロフィール更新

パス: `/api/v1/school/teachers/me/profile`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員プロフィールを更新する（氏名は原則維持、役職/専門/オフィス等を更新）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateTeacherProfileRequest`

~~~json
{
  "title": "副担任",
  "officeLocation": "202号室",
  "profileData": {
    "specialties": ["ES添削"],
    "bio": "ES添削を担当しています。"
  }
}
~~~

レスポンスモデル: `ApiResponse[UpdateTeacherProfileResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Profile updated",
  "data": {
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "school_teachers_me_profile_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422 / 500

---

## 10. 担当クラス一覧（教員）

パス: `/api/v1/school/teacher/classes`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 教員が担当するクラス一覧を返す（class_teachers を参照）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[TeacherClassList]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "classId": 301,
        "departmentName": "情報",
        "fiscalYear": 2026,
        "grade": 3,
        "name": "A組",
        "teacherRole": 0
      }
    ]
  },
  "operation": "school_teacher_classes_list"
}
~~~

エラーレスポンス:
- 401 / 403 / 500

---

## 11. クラス所属学生一覧

パス: `/api/v1/school/classes/{class_id}/students`  
メソッド: `GET`  
認証: 必要  
対象ロール: 教員/管理者  
説明: 指定クラスの学生一覧を返す。教員の場合は「担当クラスのみ」アクセス可能。

リクエストヘッダー:
- `Accept: application/json`

パスパラメータ:
- `class_id`: BIGINT

クエリパラメータ:
- `q` (optional): string（氏名/学籍番号 部分一致）
- `sort` (optional, default="nameKana:asc"): `nameKana:asc|desc`, `studentNumber:asc|desc`
- `page` (optional, default=1): int
- `pageSize` (optional, default=20): int

レスポンスモデル: `ApiResponse[Paged[ClassStudentRow]]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "items": [
      {
        "studentId": 5001,
        "accountId": 1001,
        "studentNumber": "1234567",
        "name": "山田 太郎",
        "nameKana": "やまだ たろう",
        "isJobHunting": true
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 40
  },
  "operation": "school_class_students_list"
}
~~~

エラーレスポンス:
- 401
- 403（担当外クラス）
- 404（クラスなし）
- 422
- 500

~~~json
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
  "operation": "school_class_students_list"
}
~~~

---

## 12. 学科作成（管理）

パス: `/api/v1/school/admin/departments`  
メソッド: `POST`  
認証: 必要  
対象ロール: 管理者  
説明: 学科（departments）を作成する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateDepartmentRequest`

~~~json
{
  "name": "情報",
  "code": "INF"
}
~~~

レスポンスモデル: `ApiResponse[DepartmentCreated]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Department created",
  "data": {
    "departmentId": 1
  },
  "operation": "admin_departments_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 409（code重複など）/ 422 / 500

---

## 13. 学科更新（管理）

パス: `/api/v1/school/admin/departments/{department_id}`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 管理者  
説明: 学科名/コードを更新する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateDepartmentRequest`

~~~json
{
  "name": "情報工学",
  "code": "INF"
}
~~~

レスポンスモデル: `ApiResponse[DepartmentUpdated]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Department updated",
  "data": {
    "departmentId": 1,
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_departments_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

---

## 14. 学科削除（管理）

パス: `/api/v1/school/admin/departments/{department_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 管理者  
説明: 学科を削除する。配下にクラスが存在する場合は削除不可（409）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[DepartmentDeleted]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Department deleted",
  "data": {
    "departmentId": 1,
    "deletedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_departments_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 500

---

## 15. クラス作成（管理）

パス: `/api/v1/school/admin/classes`  
メソッド: `POST`  
認証: 必要  
対象ロール: 管理者  
説明: 年度別クラスを作成する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `CreateClassRequest`

~~~json
{
  "departmentId": 1,
  "fiscalYear": 2026,
  "grade": 3,
  "name": "A組"
}
~~~

レスポンスモデル: `ApiResponse[ClassCreated]`

~~~json
{
  "success": true,
  "code": 201,
  "message": "Class created",
  "data": {
    "classId": 301
  },
  "operation": "admin_classes_create"
}
~~~

エラーレスポンス:
- 401 / 403 / 409（同一年度・同一学科・同一名称の重複を禁止する運用の場合）/ 422 / 500

---

## 16. クラス更新（管理）

パス: `/api/v1/school/admin/classes/{class_id}`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 管理者  
説明: 学科/年度/学年/クラス名を更新する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpdateClassRequest`

~~~json
{
  "departmentId": 1,
  "fiscalYear": 2027,
  "grade": 1,
  "name": "新A組"
}
~~~

レスポンスモデル: `ApiResponse[ClassUpdated]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Class updated",
  "data": {
    "classId": 301,
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_classes_update"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 422 / 500

---

## 17. クラス削除（管理）

パス: `/api/v1/school/admin/classes/{class_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 管理者  
説明: クラスを削除する。所属学生/担当教員がいる場合は削除不可（409）。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[ClassDeleted]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Class deleted",
  "data": {
    "classId": 301,
    "deletedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_classes_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 409 / 500

---

## 18. 担当教員割当（管理）

パス: `/api/v1/school/admin/classes/{class_id}/teachers`  
メソッド: `PUT`  
認証: 必要  
対象ロール: 管理者  
説明: クラスに担当教員を追加/更新する（同一 class_id × teacher_id は一意）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `UpsertClassTeacherRequest`

~~~json
{
  "teacherId": 7001,
  "role": 0
}
~~~

レスポンスモデル: `ApiResponse[ClassTeacherUpserted]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Class teacher assigned",
  "data": {
    "classId": 301,
    "teacherId": 7001,
    "role": 0,
    "updatedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_class_teachers_upsert"
}
~~~

エラーレスポンス:
- 401 / 403 / 404（class/teacherなし）/ 422 / 500

---

## 19. 担当教員解除（管理）

パス: `/api/v1/school/admin/classes/{class_id}/teachers/{teacher_id}`  
メソッド: `DELETE`  
認証: 必要  
対象ロール: 管理者  
説明: クラス担当を解除する。

リクエストヘッダー:
- `Accept: application/json`

レスポンスモデル: `ApiResponse[ClassTeacherDeleted]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "Class teacher removed",
  "data": {
    "classId": 301,
    "teacherId": 7001,
    "deletedAt": "2026-01-29T10:00:00Z"
  },
  "operation": "admin_class_teachers_delete"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 500

---

## 20. 所属学生CSV一括操作（管理）

パス: `/api/v1/school/admin/classes/{class_id}/students/csv`  
メソッド: `POST`  
認証: 必要  
対象ロール: 管理者  
説明: クラス所属学生をCSVで一括操作する（名簿確認＋CSVによる一括操作を想定）。  
※「既に students が存在する学生」を対象に、class_id を更新する方式を基本とする。

リクエストヘッダー:
- `Content-Type: multipart/form-data`

フォームデータ:
- `file`: CSV
- `mode`: `assign | unassign`（assign=指定クラスへ所属変更 / unassign=所属解除（運用で必要なら））

CSVフォーマット（例）:
- header: `studentNumber`
- rows: 学籍番号（students.student_number）

レスポンスモデル: `ApiResponse[CsvBatchResult]`

~~~json
{
  "success": true,
  "code": 200,
  "message": "CSV processed",
  "data": {
    "classId": 301,
    "mode": "assign",
    "processed": 40,
    "updated": 38,
    "skipped": 2,
    "errors": [
      { "row": 12, "studentNumber": "9999999", "reason": "student_not_found" }
    ]
  },
  "operation": "admin_class_students_csv"
}
~~~

エラーレスポンス:
- 401 / 403 / 404 / 422（CSV形式不正）/ 500
