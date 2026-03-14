"use client";

import React, { useState, useEffect, useRef, Suspense } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { toast } from "sonner";
import { Loader2Icon, ArrowLeftIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Logo } from "@/components/brand/Logo";
import apiClient from "@/lib/api-client";
import { ApiResponse, AuthResponse } from "@/types/api";

function OtpContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const email = searchParams.get("email") || "";
  const purpose = searchParams.get("purpose") || "Register";
  
  const [otp, setOtp] = useState(["", "", "", "", "", ""]);
  const [isLoading, setIsLoading] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  useEffect(() => {
    if (!email) {
      toast.error("メールアドレスが見つかりません。最初からやり直してください。");
      router.push("/auth/login");
    }
  }, [email, router]);

  const handleChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;

    const newOtp = [...otp];
    newOtp[index] = value.substring(value.length - 1);
    setOtp(newOtp);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace" && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleVerify = async () => {
    const code = otp.join("");
    if (code.length < 6) return;

    setIsLoading(true);
    try {
      const response = await apiClient.post<ApiResponse<AuthResponse>>("/auth/otp/verify", {
        email,
        otp: code,
        purpose
      });

      // response.data は ApiResponse オブジェクト
      if (response.data && response.data.success) {
        if (purpose === "PasswordReset") {
          toast.success("認証に成功しました。パスワードを更新してください。");
          router.push(`/auth/password-reset/new-password?token=${code}&email=${encodeURIComponent(email)}`);
        } else {
          const authData = response.data.data;
          if (authData && authData.token) {
            localStorage.setItem("auth_token", authData.token);
            localStorage.setItem("user_role", authData.role);
            localStorage.setItem("user_email", authData.email); // 追加
            toast.success("認証が完了しました。プロフィールの設定を始めましょう。");
            router.push("/auth/onboarding");
          } else {
            toast.success("認証が完了しました。ログインしてください。");
            router.push("/auth/login");
          }
        }
      } else {
        throw new Error("Verification failed");
      }
    } catch (error: any) {
      const message = error.response?.data?.message || "認証に失敗しました。";
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResend = async () => {
    try {
      await apiClient.post("/auth/otp/request", { email, purpose });
      toast.success("認証コードを再送しました");
    } catch (error) {
      toast.error("再送に失敗しました");
    }
  };

  const maskedEmail = email.replace(/^(..)(.*)(@.*)$/, "$1***$3");
  const title = purpose === "PasswordReset" ? "パスワード再設定の認証" : "認証コードの入力";

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 p-4">
      <div className="w-full max-w-md space-y-8">
        <div className="flex flex-col items-center justify-center">
          <Logo size="lg" className="mb-2" />
        </div>

        <Card className="border-none shadow-xl shadow-slate-200/50 rounded-2xl overflow-hidden pt-4">
          <CardHeader className="space-y-1 pb-8">
            <div className="flex justify-center mb-4">
              <div className="bg-indigo-50 text-indigo-600 p-3 rounded-full">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m12 14 4-4"/><path d="M3.34 19a10 10 0 1 1 17.32 0"/><path d="m9.05 14.82 2.95-2.82 2.95 2.82"/><path d="M12 12v9"/></svg>
              </div>
            </div>
            <CardTitle className="text-2xl font-bold text-center">{title}</CardTitle>
            <CardDescription className="text-center px-6">
              <span className="font-semibold text-slate-900">{maskedEmail}</span> 宛に送信された<br />6桁のコードを入力してください
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-8 px-8">
            <div className="flex justify-between gap-2">
              {otp.map((digit, i) => (
                <Input
                  key={i}
                  ref={(el) => (inputRefs.current[i] = el)}
                  className="w-12 h-14 text-center text-xl font-bold rounded-xl border-slate-200 focus:border-indigo-500 focus:ring-indigo-500"
                  value={digit}
                  onChange={(e) => handleChange(i, e.target.value)}
                  onKeyDown={(e) => handleKeyDown(i, e)}
                  maxLength={1}
                  disabled={isLoading}
                />
              ))}
            </div>
            <Button 
              onClick={handleVerify}
              className="w-full h-12 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-base font-semibold shadow-lg shadow-indigo-200 mt-2"
              disabled={isLoading || otp.some(d => !d)}
            >
              {isLoading ? <Loader2Icon className="animate-spin" /> : "認証する"}
            </Button>
          </CardContent>
          <CardFooter className="flex flex-col gap-4 px-8 pb-10 mt-4">
            <div className="text-center text-sm text-slate-500">
              コードが届かないですか？{" "}
              <button 
                onClick={handleResend}
                disabled={isLoading}
                className="text-indigo-600 hover:text-indigo-500 font-bold disabled:opacity-50"
              >
                再送する
              </button>
            </div>
            <Link href="/auth/login" className="flex items-center justify-center text-sm text-slate-500 hover:text-slate-700 font-medium pt-2">
              <ArrowLeftIcon size={16} className="mr-1" />
              ログイン画面に戻る
            </Link>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}

export default function OtpPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <OtpContent />
    </Suspense>
  );
}
