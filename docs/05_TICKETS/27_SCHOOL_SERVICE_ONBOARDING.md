# 【開発】School：マスタデータ取得および初回プロフィール登録の実装

## 目的
ユーザーがログイン後に自身の所属（学科・クラス）を選択し、学生または教員としての詳細情報を登録できる「オンボーディング機能」および、その前提となるマスタデータ取得機能を実装する。

## 作業内容
- [x] **SF-01: 学科・クラス一覧取得の実装**
    - `GET /api/v1/school/departments` の実装
    - `GET /api/v1/school/classes` の実装
- [x] **SF-02: 初回プロフィール登録（オンボーディング）の実装**
    - `POST /api/v1/school/onboarding/student-profile` (学生専用)
    - `POST /api/v1/school/onboarding/teacher-profile` (教員専用)
- [x] **SF-03: 自分のプロフィール取得・更新の実装**
    - `GET /api/v1/school/students/me`, `GET /api/v1/school/teachers/me` の実装
    - `PATCH` によるプロフィールの更新機能の実装
- [x] **追加: バリデーションと認可の適用**
    - FluentValidation による形式チェックの導入
    - `[Authorize]` によるロールベースのアクセス制御 (RBAC) の実装

## 完了条件
- [x] 認証済みユーザーが学科・クラスの一覧を正常に取得できること
- [x] 学生・教員がそれぞれのプロフィールを一度だけ登録できること
- [x] プロフィール情報が取得でき、許可された項目のみ更新可能であること
- [x] 一連のフローの統合テストが Pass すること
- [x] 更新時に `AuditInterceptor` を介して監査ログが自動発行されること
