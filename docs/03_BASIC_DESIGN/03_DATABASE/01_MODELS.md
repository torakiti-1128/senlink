# データベース設計書：SenLink（センリンク）

## 目次
1. [テーブル一覧](#テーブル一覧)
2. [ER図](#er図)   
3. [Auth サービス](#3-auth-service)
4. [School サービス](#4-school-service)
5. [Job サービス](#5-job-service)
6. [Activity サービス](#6-activity-service)
7. [Request サービス](#7-request-service)
8. [Notification サービス](#8-notification-service)
9. [Audit サービス](9-audit-service) 
10. [Maintenance サービス](#10-maintenance-service)

## 1. テーブル一覧
| No | サービス名 | テーブル名 | 論理名 | 説明 |
| :--- | :---- | :--- | :--- | :--- |
| 1 | `Auth` | `accounts` | アカウント | 認証基盤、ロール管理 |
| 2 | | `login_histories` | ログイン履歴 | アクセス元情報の記録 |
| 3 | `School` | `teachers` | 教員 | 教員プロフィール情報 |
| 4 | | `students` | 学生 | 学生プロフィール、学籍情報 |
| 5 | | `departments` | 学科 | 学科マスタ |
| 6 | | `classes` | クラス | クラスマスタ |
| 7 | | `class_teachers` | クラス担任 | 教員とクラスの紐付け |
| 8 | `Job` | `companies` | 企業 | 企業基本情報 |
| 9 | | `jobs` | 求人 | 求人、説明会、試験情報 |
| 10 | | `tags` | タグ | 特徴、職種分類 |
| 11 | | `job_tags` | 求人タグ | 求人とタグの中間テーブル |
| 12 | | `job_recommendations`| レコメンド | 教員から学生への推奨 |
| 13 | | `bookmarks` | ブックマーク | 学生の気になるリスト |
| 14 | | `surveys` | アンケート定義 | 案件ごとの質問項目 |
| 15 | | `survey_responses` | アンケート回答 | 学生の回答データ |
| 16 | | `todo_templates` | ToDo型 | ToDoリストの雛形 |
| 17 | | `todo_steps` | ToDoステップ | ToDoリストごとの具体的タスク |
| 18 | `Activity` | `activities` | 就職活動 | 応募状況、選考ステータス |
| 19 | | `activity_todos` | 個別ToDo | 学生ごとの進捗管理 |
| 20 | `Request` | `student_documents` | 書類提出 | 添削依頼、確認ステータス |
| 21 | | `interview_appointments`| 面接練習予約 | 教員への予約申請 |
| 22 | | `offer_reports` | 内定報告 | 内定実績と証跡画像 |
| 23 | `Notification` | `notifications` | 通知 | 受信履歴、既読管理 |
| 24 | `Audit` | `audit_logs` | 監査ログ | ユーザーの操作履歴 |
| 25 | | `error_logs` | エラーログ | システム例外、アプリバグの記録 |
| 26 | | `system_metrics` | システムメトリクス | コンテナ死活、CPU/メモリ、応答速度 |
| 27 | `Maintenance` | `system_settings` | システム設定 | マスタデータ、定数、管理フラグ |

## 共通ルール
全てのテーブルには、以下の2つのカラムが含まれるものとします（表では省略します）。
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---|:---|:---|:---|:---|
| created_at | TIMESTAMP | NN | CURRENT_TIMESTAMP | 作成日時 |
| updated_at | TIMESTAMP | NN | CURRENT_TIMESTAMP | 更新日時 |

## 制約ルール
* **PK**: 主キー
* **FK**: 外部キー（**同一サービス内のテーブルのみ**参照可能）
* **NOFK**: 論理参照（**他サービスのID**を参照。DB上の制約はなし）
* **NN**: Not Null制約


## 2. ER図

```mermaid
erDiagram
    %% Auth Service
    accounts ||--o{ login_histories : "ログイン記録"

    %% School Service
    departments ||--o{ classes : "所属"
    classes ||--o{ students : "所属"
    classes ||--o{ class_teachers : "構成"
    teachers ||--o{ class_teachers : "担当"

    %% Job Service
    companies ||--o{ jobs : "掲載"
    jobs ||--o{ job_tags : "タグ付け"
    tags ||--o{ job_tags : "定義"
    jobs ||--o{ surveys : "所持"
    surveys ||--o{ survey_responses : "回答受付"
    todo_templates ||--o{ todo_steps : "構成"
    jobs }|--|| todo_templates : "使用"
    jobs ||--o{ job_recommendations : "レコメンド"
    jobs ||--o{ bookmarks : "ブックマーク"

    %% Activity Service
    activities ||--o{ activity_todos : "ToDo"

    %% Request Service（論理的には学生が主）
    students ||--o{ student_documents : "書類提出"
    students ||--o{ interview_appointments : "面接予約"
    students ||--o{ offer_reports : "内定報告"

    %% Notification Service
    accounts ||--o{ notifications : "受信"

    %% Audit Service（論理参照）
    accounts ||--o{ audit_logs : "操作"
    accounts ||--o{ error_logs : "エラー発生"

    %% Cross-Service（論理参照・NOFK）
    accounts ||--|| teachers : "教員アカウント"
    accounts ||--|| students : "学生アカウント"

    students ||--o{ activities : "活動実施"
    jobs ||--o{ activities : "応募対象"

    accounts ||--o{ job_recommendations : "学生/教員"
    accounts ||--o{ bookmarks : "学生"

    accounts ||--o{ student_documents : "提出者"
    accounts ||--o{ interview_appointments : "学生/教員"
    accounts ||--o{ offer_reports : "報告者/承認者"
```

## 3. Auth サービス
認証／認可、JWTの発行／検証、ロールベースアクセス制御（RBAC）

### 3-1. accounts (アカウント)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| email | VARCHAR(255) | NN | - | メールアドレス |
| password | VARCHAR(255) | NN | - | パスワード(Hash) |
| role | SMALLINT | NN | - | 0:学生／1:教員／2:管理者 |
| is_active | BOOLEAN | NN | - | 有効フラグ |
| deleted_at | TIMESTAMP | - | NULL | 論理削除 |

### 3-2. login_histories (ログイン履歴)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| account_id | BIGINT | FK, NN | - | アカウントID (accounts.id) |
| ip_address | VARCHAR(45) | - | NULL | IPアドレス |
| user_agent | TEXT | - | NULL | ブラウザ情報 |
| status | SMALLINT | NN | - | 0:失敗／1:成功 |

## 4. School サービス
学校情報の管理（学科、クラス、教員／学生データ）

### 4-1. teachers (教員)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| account_id | BIGINT | NOFK, NN | - | アカウントID (accounts.id) |
| name | VARCHAR(100) | NN | - | 氏名 |
| name_kana | VARCHAR(100) | NN | - | 氏名カナ (検索／ソート用) |
| title | VARCHAR(50) | - | NULL | 役職 |
| office_location | VARCHAR(100) | - | NULL | オフィス場所 (例: 201号室) |
| profile_data | JSONB | - | NULL | 詳細プロフィール (経歴、専門分野など) |

### 4-2. students (学生)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| account_id | BIGINT | NOFK, NN | - | アカウントID (accounts.id) |
| class_id | BIGINT | FK, NN | - | クラスID |
| student_number | VARCHAR(20) | UQ, NN | - | 学籍番号 |
| name | VARCHAR(100) | NN | - | 氏名 |
| name_kana | VARCHAR(100) | NN | - | 氏名カナ (検索／ソート用) |
| date_of_birth | DATE | NN | - | 生年月日 |
| gender | SMALLINT | NN | 0 | 0:不明／1:男／2:女／9:その他 |
| admission_year | INT | NN | - | 入学年度 |
| is_job_hunting | BOOLEAN | NN | TRUE | 就活中フラグ |
| profile_data | JSONB | - | NULL | PR、資格、リンクなど |

### 4-3. departments (学科)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| name | VARCHAR(100) | NN | - | 学科名 |
| code | VARCHAR(20) | NN | - | 学科コード |

### 4-4. classes (クラス)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| department_id | BIGINT | FK, NN | - | 学科ID (departments.id) |
| fiscal_year | SMALLINT | NN | - | 年度  |
| grade | SMALLINT | NN | - | 学年 |
| name | VARCHAR(50) | NN | - | クラス名 (例: A組／午前クラス) |

### 4-5. class_teachers (クラス担任)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| class_id | BIGINT | FK, NN | - | クラスID |
| teacher_id | BIGINT | FK, NN | - | 教員ID (teachers.id) |
| role | SMALLINT | NN | TRUE | 0:担任／1:副担任／3:キャリアセンター／9:その他 |

## 5. Job サービス
求人情報の管理、企業情報、求人ToDoテンプレート

### 5-1. companies (企業)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| name | VARCHAR(255) | NN | - | 企業名 |
| address | VARCHAR(255) | - | NULL | 所在地 |
| url | VARCHAR(255) | - | NULL | URL |

### 5-2. jobs (求人)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| company_id | BIGINT | FK, NN | - | 企業ID (companies.id) |
| todo_template_id | BIGINT | FK, NN | - | ToDoテンプレートID(todo_templates.id) |
| teacher_account_id | BIGINT | NOFK, NN | - | 教員ID (accounts.id) |
| title | VARCHAR(255) | NN | - | 管理用案件名 |
| type | SMALLINT | NN | - | 0:説明会／1:インターン／2:採用試験 |
| capacity | INT | - | NULL | 定員数 |
| format | SMALLINT | NN | - | 0:対面／1:オンライン／2:ハイブリッド |
| place | VARCHAR(255) | - | NULL | 開催場所・URL |
| contact_info | VARCHAR(255) | - | NULL | 緊急連絡先 |
| event_start_date | DATE | - | NULL | 開催日／開始日 |
| event_end_date | DATE | - | NULL | 終了日 |
| cancel_deadline | DATE | - | NULL | キャンセル期限日 |
| status | SMALLINT | - | - | 0:下書き／1:公開／9:終了 |
| content | TEXT | NN | - | 企業紹介／募集要項 |
| deadline | DATE | - | NULL | 掲載終了日 |
| deleted_at | TIMESTAMP | - | NULL | 論理削除 |

### 5-3. tags (タグ)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| name | VARCHAR(50) | NN | - | タグ名 |
| type | SMALLINT | NN | - | 0:職種／1:勤務地／3:特徴／4:必要なもの |

### 5-4. job_tags (求人タグ中間テーブル)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| job_id | BIGINT | FK, NN | - | 求人ID (jobs.id) |
| tag_id | BIGINT | FK, NN | - | タグID (tags.id) |

### 5-5. job_recommendations (求人レコメンド)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| job_id | BIGINT | FK, NN | - | 求人ID (jobs.id) |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (accounts.id) |
| recommender_account_id | BIGINT | NOFK, NN | - | 教員ID (accounts.id) |

### 5-6. bookmarks (ブックマーク)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| job_id | BIGINT | FK, NN | - | 求人ID (jobs.id)|
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (account.id) |

### 5-7. surveys (アンケート定義)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| job_id | BIGINT | NN | - | 求人ID (jobs.id) |
| title | VARCHAR(255) | NN | - | アンケート名 |
| questions | JSONB | NN | - | 質問項目 |

### 5-8. survey_responses (アンケート回答)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| survey_id | BIGINT | FK, NN | - | アンケート定義ID (surveys.id) |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (account.id) |
| answers | JSONB | NN | - | 回答内容 |

### 5-9. todo_templates (ToDoテンプレート)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| name | VARCHAR(100) | NN | - | テンプレート名 |
| description | TEXT | - | NULL | 説明 |

### 5-10. todo_steps (ToDoステップ)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| template_id | BIGINT | FK, NN | - | 親テンプレートID (todo_templates.id) |
| name | VARCHAR(100) | NN | - | タスク名 |
| description | TEXT | - | NULL | 指示内容 |
| step_order | INT | NN | - | 順序 |
| days_deadline | INT | - | 0 | 相対期限 (日) |
| is_verification_required | BOOLEAN | - | FALSE | 承認必須フラグ 

## 6. Activity サービス
学生の応募状況と活動実績

### 6-1. activities (就職活動)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| job_id | BIGINT | NOFK, NN | - | 求人ID (jobs.id) |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (accounts.id) |
| status | SMALLINT | NN | 0 | 0:参加前／1:参加済 |
| cancellation_status | SMALLINT | - | 0 | 0:なし／1:申請中／2:承認済 |
| reviewed_by_account_id | BIGINT | NOFK | NULL | 教員ID (accounts.id) |

### 6-2. activity_todos (就職活動ToDo)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| activity_id | BIGINT | FK, NN | - | 就活ID (activities.id) |
| name | VARCHAR(100) | NN | - | タスク名 |
| description | TEXT | - | NULL | 指示内容 |
| step_order | INT | NN | - | 順序 |
| status | SMALLINT | NN | 0 | 0:未完了／1:完了／ |
| deadline | DATE | NN | - | 期限日 |
| completed_at | TIMESTAMP | - | NULL | 完了日時 |

## 7. Request サービス
学生の各種申請と教員のステート管理

### 7-1. student_documents (提出書類・添削依頼)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (accounts.id) |
| name | VARCHAR(100) | NN | - | 書類名 |
| file_path | VARCHAR(255) | NN | - | ファイルパス |
| type | SMALLINT | NN | - | 0:履歴書／1:ES／2:ポートフォリオ／9:その他 |
| status | SMALLINT | NN | 0 | 0:下書き／1:確認依頼中／2:確認済(OK)／3:要修正 |
| feedback_account_id | BIGINT | NOFK | NULL | 教員ID (accounts.id) |
| feedback | TEXT | - | NULL | コメント |

### 7-2. interview_appointments (面接練習予約)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (accounts.id) |
| teacher_account_id | BIGINT | NOFK | NULL | 教員ID (accounts.id) |
| status | SMALLINT | - | 0 | 0:申請中／1:確定／2:実施済 |
| requested_at | TIMESTAMP | NN | - | 学生希望日時 |
| scheduled_at | TIMESTAMP | - | NULL | 確定日時 |
| meeting_place | VARCHAR(255) | - | NULL | 実施場所 |
| feedback | TEXT | - | NULL | 実施後アドバイス |

### 7-3. offer_reports (内定報告)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| student_account_id | BIGINT | NOFK, NN | - | 学生ID (accounts.id) |
| job_id | BIGINT | NOFK | NULL | 求人ID (jobs.id) |
| company_name | VARCHAR(255) | NN | - | 企業名 |
| offered_at | DATE | NN | - | 内定日 |
| image_path | VARCHAR(255) | NN | - | 通知書画像 |
| status | SMALLINT | NN | 0 | 0:確認待ち／1:承認済 |
| verified_by_account_id | BIGINT | FK | NULL | 承認者ID (accounts.id) |

### 8. Notification サービス
プッシュ通知とメール送信の制御

### 8-1. notifications (通知)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| recipient_account_id | BIGINT | NOFK, NN | - | 受信者ID (accounts.id) |
| title | VARCHAR(255) | NN | - | タイトル |
| body | TEXT | - | NULL | 本文 |
| link_url | VARCHAR(255) | NULL | URL |
| type | SMALLINT | NN | 0 | 0:情報（青）／1:重要（赤）／2:承認（緑） |
| is_read | BOOLEAN | NN | FALSE | 既読フラグ |

## 9. Audit サービス
操作ログの記録、証跡管理

### 9-1. audit_logs (監査ログ)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| actor_id | BIGINT | NOFK, NN | - | 操作者ID (accounts.id) |
| target_table | VARCHAR(50) | NN | - | 対象テーブル名 |
| target_id | BIGINT | NN | - | 対象ID |
| method | VARCHAR(50) | NN | - | CRUDのいずれか |
| details | JSONB | - | NULL | 変更前後のデータ |
| ip_address | VARCHAR(45) | - | NULL | IPアドレス |

### 9-2. error_logs (エラーログ)
| カラム名 | データ型 | 制約 | 説明 |
|:---------|:---------|:-----|:-----|
| id | BIGINT | PK | エラーID |
| service_name| VARCHAR(50) | NN | 発生元サービス (Auth, Job, Worker等) |
| severity | SMALLINT | NN | 0:Warn／2:Error／3:Critical |
| message | TEXT | NN | エラーメッセージの要約 |
| stack_trace | TEXT | - | 詳細なスタックトレース（デバッグ用） |
| request_url | TEXT | - | 発生時のAPIエンドポイントURL |
| request_params| JSONB | - | 発生時のリクエストボディ、クエリ等 |
| account_id | BIGINT | NOFK | 発生時にログインしていたアカウント (任意) |

### 9-3. system_metrics (システムメトリクス)
| カラム名 | データ型 | 制約 | 説明 |
|:---------|:---------|:-----|:-----|
| id | BIGINT | PK | メトリクスID |
| component | VARCHAR(50) | NN | Apache／FastAPI／Worker / RabbitMQ |
| status | SMALLINT | NN | 0:Down／1:Healthy／2:HighLoad |
| response_time| INT | - | 平均レスポンス速度 (ms) |
| cpu_usage | NUMERIC(5,2)| - | CPU使用率 (%) |
| mem_usage | NUMERIC(5,2)| - | メモリ使用率 (%) |
| disk_usage | NUMERIC(5,2)| - | ディスク使用率 (%) |

## 10. Maintenance サービス
システムの設定値変更と状態管理

### 10-1. system_settings (システム設定)
| カラム名 | データ型 | 制約 | デフォルト値 | 説明 |
|:---------|:---------|:-----|:-------------|:-----|
| id | BIGINT | PK | GENERATED | ID |
| key | VARCHAR(50) | NN | - | 設定キー |
| value | TEXT | NN | - | 設定値 |
| description | VARCHAR(255) | - | NULL | 説明 |