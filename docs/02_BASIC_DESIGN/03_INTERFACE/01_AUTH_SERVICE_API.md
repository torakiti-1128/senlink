# API設計：Authサービス

## 概要

Authサービスは、SenLink の認証・認可を提供する。
- 認証方式：Cookie（HttpOnly） + JWT
- ロール：0=学生 / 1=教員 / 2=管理者
- セキュリティ前提：学内IP制限（学外は 403）

## API一覧（Auth）

| No | API名 | パス | メソッド | 認証 | 対象ロール | 概要 |
|---:|---|---|---|---|---|---|
| 1 | ログイン（パスワード） | /api/v1/auth/login/password | POST | 不要 | ALL | メール+パスワードでログインし、access/refresh Cookieを発行 |
| 2 | ログイン（OTP要求） | /api/v1/auth/login/otp/request | POST | 不要 | ALL | OTPをメール送信（ログイン用） |
| 3 | ログイン（OTP検証） | /api/v1/auth/login/otp/verify | POST | 不要 | ALL | OTP検証し、access/refresh Cookieを発行 |
| 4 | 新規登録（OTP要求） | /api/v1/auth/register/otp/request | POST | 不要 | ALL | メール宛にOTPを送信（登録開始） |
| 5 | 新規登録（OTP検証） | /api/v1/auth/register/otp/verify | POST | 不要 | ALL | OTP検証（登録許可トークン発行 or 検証フラグ） |
| 6 | 新規登録（アカウント作成） | /api/v1/auth/register | POST | 不要 | ALL | 学校メールでアカウント作成（学生/教員ロール決定） |
| 7 | パスワード再設定（OTP要求） | /api/v1/auth/password/reset/otp/request | POST | 不要 | ALL | パスワード再設定用OTP送信 |
| 8 | パスワード再設定（OTP検証） | /api/v1/auth/password/reset/otp/verify | POST | 不要 | ALL | OTP検証（再設定許可） |
| 9 | パスワード再設定（更新） | /api/v1/auth/password/reset | POST | 不要 | ALL | 新パスワードを設定 |
| 10 | トークン更新（Refresh） | /api/v1/auth/refresh | POST | 必要（refresh cookie） | ALL | refresh_tokenでaccess_tokenを再発行 |
| 11 | ログアウト | /api/v1/auth/logout | POST | 必要 | ALL | Cookie削除 + refresh失効 |
| 12 | セッション確認（me） | /api/v1/auth/me | GET | 必要 | ALL | 自分のアカウント概要を返す |
| 13 | 権限変更（教員→管理者） | /api/v1/auth/admin/accounts/{account_id}/role | PATCH | 必要 | 管理者 | 管理者が教員のroleを変更（付与/剥奪） |
| 14 | アカウント無効化 | /api/v1/auth/admin/accounts/{account_id}/deactivate | PATCH | 必要 | 管理者 | is_active=false（ログイン禁止） |

---

## 1. ログイン（パスワード）

パス: `/api/v1/auth/login/password`  
メソッド: `POST`  
認証: 不要  
説明: メール/パスワードでログインし、Cookie（HttpOnly）に `access_token`（必要なら `refresh_token`）を発行する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `LoginWithPasswordRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "password": "user_password123"
}
```

レスポンスモデル: `ApiResponse[AuthSession]`

Set-Cookie:
- `access_token=...; HttpOnly; Secure; SameSite=Lax; Path=/`
- `refresh_token=...; HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth/refresh（後続フェーズで有効化検討）`

```json
{
  "success": true,
  "code": 200,
  "message": "Login successful",
  "data": {
    "accountId": 1001,
    "email": "1234567@school.ac.jp",
    "role": 0,
    "isActive": true,
    "lastLoginAt": "2026-01-16T10:00:00Z"
  },
  "operation": "auth_login_password"
}
```

エラーレスポンス:
- 400: Bad Request（形式不正）
- 401: Unauthorized（メール/パスワード不一致）
- 403: Forbidden（学内IP外）
- 422: Unprocessable Entity（バリデーション不正）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Invalid credentials",
  "error": {
    "type": "AUTH_INVALID_CREDENTIALS",
    "details": []
  },
  "operation": "auth_login_password"
}
```

---

## 2. ログイン（OTP要求）

パス: `/api/v1/auth/login/otp/request`  
メソッド: `POST`  
認証: 不要  
説明: ログイン用OTPをメール送信する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `RequestLoginOtpRequest`

```json
{
  "email": "1234567@school.ac.jp"
}
```

レスポンスモデル: `ApiResponse[OtpRequestResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "OTP sent",
  "data": {
    "email": "1234567@school.ac.jp",
    "expiresInSeconds": 300,
    "cooldownSeconds": 60
  },
  "operation": "auth_login_otp_request"
}
```

エラーレスポンス:
- 400: Bad Request
- 403: Forbidden（学内IP外）
- 404: Not Found（アカウントなし）
- 429: Too Many Requests（連打対策）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 429,
  "message": "Too many requests",
  "error": {
    "type": "RATE_LIMITED",
    "details": [
      {
        "field": "email",
        "reason": "cooldown_not_elapsed"
      }
    ]
  },
  "operation": "auth_login_otp_request"
}
```

---

## 3. ログイン（OTP検証）

パス: `/api/v1/auth/login/otp/verify`  
メソッド: `POST`  
認証: 不要  
説明: OTPを検証し、成功したら Cookie（HttpOnly）に `access_token`（必要なら `refresh_token`）を発行する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `VerifyLoginOtpRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "otp": "123456"
}
```

レスポンスモデル: `ApiResponse[AuthSession]`

Set-Cookie:
- `access_token=...; HttpOnly; Secure; SameSite=Lax; Path=/`
- `refresh_token=...; HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth/refresh（後続フェーズで有効化検討）`

```json
{
  "success": true,
  "code": 200,
  "message": "Login successful",
  "data": {
    "accountId": 1001,
    "email": "1234567@school.ac.jp",
    "role": 0,
    "isActive": true,
    "lastLoginAt": "2026-01-16T10:00:00Z"
  },
  "operation": "auth_login_otp_verify"
}
```

エラーレスポンス:
- 400: Bad Request
- 401: Unauthorized（OTP不一致/期限切れ）
- 403: Forbidden（学内IP外）
- 404: Not Found（アカウントなし）
- 429: Too Many Requests（試行回数制限）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Invalid or expired OTP",
  "error": {
    "type": "OTP_INVALID",
    "details": [
      {
        "field": "otp",
        "reason": "invalid_or_expired"
      }
    ]
  },
  "operation": "auth_login_otp_verify"
}
```

---

## 4. 新規登録（OTP要求）

パス: `/api/v1/auth/register/otp/request`  
メソッド: `POST`  
認証: 不要  
説明: 新規登録開始。メールにOTPを送信する（登録用）。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `RequestRegisterOtpRequest`

```json
{
  "email": "1234567@school.ac.jp"
}
```

レスポンスモデル: `ApiResponse[OtpRequestResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "OTP sent",
  "data": {
    "email": "1234567@school.ac.jp",
    "expiresInSeconds": 300,
    "cooldownSeconds": 60
  },
  "operation": "auth_register_otp_request"
}
```

エラーレスポンス:
- 400: Bad Request（メール形式不正）
- 403: Forbidden（学内IP外）
- 409: Conflict（既に登録済み）
- 429: Too Many Requests
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 409,
  "message": "Account already exists",
  "error": {
    "type": "CONFLICT",
    "details": []
  },
  "operation": "auth_register_otp_request"
}
```

---

## 5. 新規登録（OTP検証）

パス: `/api/v1/auth/register/otp/verify`  
メソッド: `POST`  
認証: 不要  
説明: OTPを検証し、成功したら 登録許可トークン（`registration_token`）を返す。  
このトークンを `/api/v1/auth/register` に渡すことでアカウント作成が可能となる。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `VerifyRegisterOtpRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "otp": "123456"
}
```

レスポンスモデル: `ApiResponse[RegisterOtpVerified]`

```json
{
  "success": true,
  "code": 200,
  "message": "OTP verified",
  "data": {
    "email": "1234567@school.ac.jp",
    "registrationToken": "regtok_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresInSeconds": 600
  },
  "operation": "auth_register_otp_verify"
}
```

エラーレスポンス:
- 400: Bad Request
- 401: Unauthorized（OTP不一致/期限切れ）
- 403: Forbidden（学内IP外）
- 409: Conflict（既に登録済み）
- 429: Too Many Requests（試行回数制限）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Invalid or expired OTP",
  "error": {
    "type": "OTP_INVALID",
    "details": []
  },
  "operation": "auth_register_otp_verify"
}
```

---

## 6. 新規登録（アカウント作成）

パス: `/api/v1/auth/register`  
メソッド: `POST`  
認証: 不要  
説明: 学校メールでアカウント作成を行う。ロールはメール形式で自動決定する。  
- 学生：`7桁数字@学校ドメイン` → role=0  
- 教員：`myouzi_namae@学校ドメイン` → role=1  
- 管理者：初期登録では付与しない（管理者が後から変更）

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `RegisterAccountRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "password": "user_password123",
  "registrationToken": "regtok_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

レスポンスモデル: `ApiResponse[RegisterResult]`

Set-Cookie（任意：登録後に自動ログインする場合）:
- `access_token=...; HttpOnly; Secure; SameSite=Lax; Path=/`
- `refresh_token=...; HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth/refresh（後続フェーズで有効化検討）`

```json
{
  "success": true,
  "code": 201,
  "message": "Registration successful",
  "data": {
    "accountId": 1002,
    "email": "1234567@school.ac.jp",
    "role": 0,
    "isActive": true
  },
  "operation": "auth_register"
}
```

エラーレスポンス:
- 400: Bad Request（形式不正、registrationToken不足など）
- 401: Unauthorized（registrationToken無効/期限切れ）
- 403: Forbidden（学内IP外）
- 409: Conflict（既に登録済み）
- 422: Unprocessable Entity（パスワードポリシー違反など）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Registration token invalid or expired",
  "error": {
    "type": "TOKEN_INVALID",
    "details": [
      {
        "field": "registrationToken",
        "reason": "invalid_or_expired"
      }
    ]
  },
  "operation": "auth_register"
}
```

---

## 7. パスワード再設定（OTP要求）

パス: `/api/v1/auth/password/reset/otp/request`  
メソッド: `POST`  
認証: 不要  
説明: パスワード再設定用OTPを送信する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `RequestPasswordResetOtpRequest`

```json
{
  "email": "1234567@school.ac.jp"
}
```

レスポンスモデル: `ApiResponse[OtpRequestResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "OTP sent",
  "data": {
    "email": "1234567@school.ac.jp",
    "expiresInSeconds": 300,
    "cooldownSeconds": 60
  },
  "operation": "auth_password_reset_otp_request"
}
```

エラーレスポンス:
- 400: Bad Request
- 403: Forbidden（学内IP外）
- 404: Not Found（アカウントなし）
- 429: Too Many Requests
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 404,
  "message": "Account not found",
  "error": {
    "type": "NOT_FOUND",
    "details": []
  },
  "operation": "auth_password_reset_otp_request"
}
```

---

## 8. パスワード再設定（OTP検証）

パス: `/api/v1/auth/password/reset/otp/verify`  
メソッド: `POST`  
認証: 不要  
説明: OTPを検証し、成功したら 再設定許可トークン（`password_reset_token`）を返す。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `VerifyPasswordResetOtpRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "otp": "123456"
}
```

レスポンスモデル: `ApiResponse[PasswordResetOtpVerified]`

```json
{
  "success": true,
  "code": 200,
  "message": "OTP verified",
  "data": {
    "email": "1234567@school.ac.jp",
    "passwordResetToken": "pwrtok_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresInSeconds": 600
  },
  "operation": "auth_password_reset_otp_verify"
}
```

エラーレスポンス:
- 400: Bad Request
- 401: Unauthorized（OTP不一致/期限切れ）
- 403: Forbidden（学内IP外）
- 404: Not Found（アカウントなし）
- 429: Too Many Requests
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Invalid or expired OTP",
  "error": {
    "type": "OTP_INVALID",
    "details": []
  },
  "operation": "auth_password_reset_otp_verify"
}
```

---

## 9. パスワード再設定（更新）

パス: `/api/v1/auth/password/reset`  
メソッド: `POST`  
認証: 不要  
説明: `password_reset_token` を用いて新パスワードを設定する。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: `PasswordResetRequest`

```json
{
  "email": "1234567@school.ac.jp",
  "newPassword": "new_password_456",
  "passwordResetToken": "pwrtok_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

レスポンスモデル: `ApiResponse[PasswordResetResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Password updated",
  "data": {
    "email": "1234567@school.ac.jp",
    "updatedAt": "2026-01-16T10:10:00Z"
  },
  "operation": "auth_password_reset"
}
```

エラーレスポンス:
- 400: Bad Request
- 401: Unauthorized（token無効/期限切れ）
- 403: Forbidden（学内IP外）
- 422: Unprocessable Entity（パスワードポリシー違反）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 422,
  "message": "Password policy violation",
  "error": {
    "type": "PASSWORD_POLICY",
    "details": [
      {
        "field": "newPassword",
        "reason": "too_short"
      }
    ]
  },
  "operation": "auth_password_reset"
}
```

---

## 10. トークン更新（Refresh）

パス: `/api/v1/auth/refresh`  
メソッド: `POST`  
認証: 必要（refresh cookie）  
説明: refresh_token により access_token を再発行する。  
※ refresh_token導入は後続フェーズで検討。現フェーズで未導入の場合は本APIは無効化（404または501）でもよい。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし（Cookieのみ）

```json
{}
```

レスポンスモデル: `ApiResponse[RefreshResult]`

Set-Cookie:
- `access_token=...; HttpOnly; Secure; SameSite=Lax; Path=/`

```json
{
  "success": true,
  "code": 200,
  "message": "Token refreshed",
  "data": {
    "refreshedAt": "2026-01-16T10:15:00Z"
  },
  "operation": "auth_refresh"
}
```

エラーレスポンス:
- 401: Unauthorized（refresh_token不正/期限切れ）
- 403: Forbidden（学内IP外）
- 500: Internal Server Error
- 501: Not Implemented（現フェーズで無効化する場合）

```json
{
  "success": false,
  "code": 401,
  "message": "Refresh token invalid or expired",
  "error": {
    "type": "TOKEN_INVALID",
    "details": []
  },
  "operation": "auth_refresh"
}
```

---

## 11. ログアウト

パス: `/api/v1/auth/logout`  
メソッド: `POST`  
認証: 必要  
説明: Cookieを削除し、（refresh導入時は）refreshを失効させる。

リクエストヘッダー:
- `Content-Type: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[LogoutResult]`

Set-Cookie（削除）:
- `access_token=; Max-Age=0; HttpOnly; Secure; SameSite=Lax; Path=/`
- `refresh_token=; Max-Age=0; HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth/refresh`

```json
{
  "success": true,
  "code": 200,
  "message": "Logged out",
  "data": {
    "loggedOutAt": "2026-01-16T10:20:00Z"
  },
  "operation": "auth_logout"
}
```

エラーレスポンス:
- 401: Unauthorized（未ログイン）
- 403: Forbidden（学内IP外）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": {
    "type": "AUTH_REQUIRED",
    "details": []
  },
  "operation": "auth_logout"
}
```

---

## 12. セッション確認（me）

パス: `/api/v1/auth/me`  
メソッド: `GET`  
認証: 必要  
説明: 現在ログイン中のアカウント概要を返す。

リクエストヘッダー:
- `Accept: application/json`

リクエストモデル: なし

```json
{}
```

レスポンスモデル: `ApiResponse[AccountMe]`

```json
{
  "success": true,
  "code": 200,
  "message": "OK",
  "data": {
    "accountId": 1001,
    "email": "1234567@school.ac.jp",
    "role": 0,
    "isActive": true
  },
  "operation": "auth_me"
}
```

エラーレスポンス:
- 401: Unauthorized（未ログイン/トークン期限切れ）
- 403: Forbidden（学内IP外）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 401,
  "message": "Not authenticated",
  "error": {
    "type": "AUTH_REQUIRED",
    "details": []
  },
  "operation": "auth_me"
}
```

---

## 13. 権限変更（教員→管理者）

パス: `/api/v1/auth/admin/accounts/{account_id}/role`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 管理者  
説明: 管理者が指定アカウントの role を変更する（付与/剥奪）。  
※運用：教員→管理者への昇格/降格を想定（学生は対象外にする方針でもよい）

リクエストヘッダー:
- `Content-Type: application/json`
- `Authorization: (Cookie access_token)`

リクエストモデル: `UpdateAccountRoleRequest`

```json
{
  "role": 2
}
```

レスポンスモデル: `ApiResponse[UpdateAccountRoleResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Role updated",
  "data": {
    "accountId": 2001,
    "newRole": 2,
    "updatedAt": "2026-01-16T10:25:00Z"
  },
  "operation": "admin_update_account_role"
}
```

エラーレスポンス:
- 400: Bad Request（role値不正）
- 401: Unauthorized（未ログイン）
- 403: Forbidden（管理者権限なし / 学内IP外）
- 404: Not Found（対象アカウントなし）
- 409: Conflict（無効化済み等で変更不可）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 403,
  "message": "Admin privilege required",
  "error": {
    "type": "FORBIDDEN",
    "details": []
  },
  "operation": "admin_update_account_role"
}
```

---

## 14. アカウント無効化

パス: `/api/v1/auth/admin/accounts/{account_id}/deactivate`  
メソッド: `PATCH`  
認証: 必要  
対象ロール: 管理者  
説明: 指定アカウントを無効化する（is_active=false）。以降ログイン不可。

リクエストヘッダー:
- `Content-Type: application/json`
- `Authorization: (Cookie access_token)`

リクエストモデル: `DeactivateAccountRequest`

```json
{
  "reason": "graduated"
}
```

レスポンスモデル: `ApiResponse[DeactivateAccountResult]`

```json
{
  "success": true,
  "code": 200,
  "message": "Account deactivated",
  "data": {
    "accountId": 2001,
    "isActive": false,
    "updatedAt": "2026-01-16T10:30:00Z"
  },
  "operation": "admin_deactivate_account"
}
```

エラーレスポンス:
- 401: Unauthorized
- 403: Forbidden（管理者権限なし / 学内IP外）
- 404: Not Found
- 409: Conflict（既に無効化済み）
- 500: Internal Server Error

```json
{
  "success": false,
  "code": 409,
  "message": "Account already deactivated",
  "error": {
    "type": "CONFLICT",
    "details": []
  },
  "operation": "admin_deactivate_account"
}
```
