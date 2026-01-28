# API設計：Schoolサービス

## 概要
Schoolサービスは、学校組織情報（学科・クラス）および人物マスタ（学生・教員）と、それらの紐付け（担任/担当クラス）を管理する。
- 対象エンティティ：departments / classes / students / teachers / class_teachers
- 参照方針：
  - `students.class_id` は School 内FK（classes.id）
  - `teachers.account_id` / `students.account_id` は Auth への論理参照（accounts.id）
- 権限制御（基本方針）
  - 学生：自分のプロフィールのみ閲覧/更新可（他学生は不可）
  - 教員：担当クラスの学生のみ閲覧可（運用上、担任/副担任/キャリアセンター等の割当で制御）
  - 管理者：全学生/全教員を閲覧・管理可
- セキュリティ前提：学内IP制限（学外は 403）
- 認証方式：Cookie（HttpOnly） + JWT（access_token）
- ロール：0=学生 / 1=教員 / 2=管理者

## API一覧（School）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | 学科一覧取得 | /api/v1/school/departments | GET | 必要 | 教員/管理者 | 学科一覧を返す |
| 2 | 学科作成 | /api/v1/admin/school/departments | POST | 必要 | 管理者 | 学科を新規作成 |
| 3 | 学科更新 | /api/v1/admin/school/departments/{department_id} | PATCH | 必要 | 管理者 | 学科情報を更新 |
| 4 | クラス一覧取得 | /api/v1/school/classes | GET | 必要 | 教員/管理者 | クラス一覧（フィルタ可）を返す |
| 5 | クラス作成 | /api/v1/admin/school/classes | POST | 必要 | 管理者 | クラスを新規作成 |
| 6 | クラス更新 | /api/v1/admin/school/classes/{class_id} | PATCH | 必要 | 管理者 | クラス情報を更新 |
| 7 | クラス担当教員一覧 | /api/v1/school/classes/{class_id}/teachers | GET | 必要 | 教員/管理者 | 指定クラスの担当教員（担任等）を返す |
| 8 | クラス担当教員割当 | /api/v1/admin/school/classes/{class_id}/teachers | POST | 必要 | 管理者 | クラスに教員を割り当てる（class_teachers作成） |
| 9 | クラス担当教員解除 | /api/v1/admin/school/classes/{class_id}/teachers/{teacher_id} | DELETE | 必要 | 管理者 | クラスと教員の紐付けを解除 |
| 10 | 学生一覧 | /api/v1/school/students | GET | 必要 | 教員/管理者 | 学生一覧（教員は担当クラスのみ、管理者は全件） |
| 11 | 学生詳細 | /api/v1/school/students/{student_id} | GET | 必要 | 教員/管理者 | 学生詳細（教員は担当クラスのみ） |
| 12 | 自分の学生プロフィール取得 | /api/v1/school/students/me | GET | 必要 | 学生 | 自分（学生）のプロフィールを返す |
| 13 | 自分の学生プロフィール更新 | /api/v1/school/students/me | PATCH | 必要 | 学生 | 自分（学生）のプロフィールを更新（許可項目のみ） |
| 14 | 教員一覧 | /api/v1/school/teachers | GET | 必要 | 教員/管理者 | 教員一覧（学校内の教員）を返す |
| 15 | 教員詳細 | /api/v1/school/teachers/{teacher_id} | GET | 必要 | 教員/管理者 | 教員詳細を返す |
| 16 | 自分の教員プロフィール取得 | /api/v1/school/teachers/me | GET | 必要 | 教員/管理者 | 自分（教員）のプロフィールを返す |
| 17 | 自分の教員プロフィール更新 | /api/v1/school/teachers/me | PATCH | 必要 | 教員/管理者 | 自分（教員）のプロフィールを更新 |
| 18 | 学生作成（管理） | /api/v1/admin/school/students | POST | 必要 | 管理者 | 学生プロフィールを作成（必要なら初期投入用途） |
| 19 | 学生更新（管理） | /api/v1/admin/school/students/{student_id} | PATCH | 必要 | 管理者 | 学生プロフィールを更新（class付替等） |
| 20 | 教員作成（管理） | /api/v1/admin/school/teachers | POST | 必要 | 管理者 | 教員プロフィールを作成（教員アカウントと紐付け） |
| 21 | 教員更新（管理） | /api/v1/admin/school/teachers/{teacher_id} | PATCH | 必要 | 管理者 | 教員プロフィールを更新 |