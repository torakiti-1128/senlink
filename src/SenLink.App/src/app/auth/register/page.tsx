"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Loader2Icon, ShieldCheckIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Logo } from "@/components/brand/Logo";
import apiClient from "@/lib/api-client";
import { ApiResponse } from "@/types/api";

const registerSchema = z.object({
  email: z.string().email("有効なメールアドレスを入力してください"),
  password: z.string().min(8, "パスワードは8文字以上で入力してください"),
  confirmPassword: z.string().min(1, "確認用パスワードを入力してください"),
}).refine((data) => data.password === data.confirmPassword, {
  message: "パスワードが一致しません",
  path: ["confirmPassword"],
});

type RegisterFormValues = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setIsLoading(true);
    try {
      const response = await apiClient.post<ApiResponse>("/auth/register", {
        email: values.email,
        password: values.password,
      });

      if (response.data.success) {
        toast.success("アカウントを作成しました。認証を行ってください。");
        await apiClient.post("/auth/otp/request", { email: values.email, purpose: "Register" });
        router.push(`/auth/otp?email=${encodeURIComponent(values.email)}&purpose=Register`);
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || "登録に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 p-4">
      <div className="w-full max-w-md space-y-8">
        <div className="flex flex-col items-center justify-center">
          <Logo size="lg" className="mb-2" />
          <p className="text-slate-500 text-sm">新しいキャリアの扉を開きましょう。</p>
        </div>

        <Card className="border-none shadow-xl shadow-slate-200/50 rounded-2xl overflow-hidden pt-4">
          <CardHeader className="space-y-1 pb-6 pt-8">
            <CardTitle className="text-2xl font-bold text-center">アカウント作成</CardTitle>
            <CardDescription className="text-center px-6">
              学校から配布されたメールアドレスを入力してください
            </CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit(onSubmit)}>
            {/* gap-6 で余白を確保 */}
            <CardContent className="grid gap-6 px-8">
              <div className="grid gap-2">
                <Label htmlFor="email">メールアドレス</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="s1234567@senlink.dev"
                  className={`rounded-xl h-11 border-slate-200 ${errors.email ? "border-red-500" : ""}`}
                  {...register("email")}
                  disabled={isLoading}
                />
                {errors.email ? (
                  <p className="text-xs text-red-500">{errors.email.message}</p>
                ) : (
                  <p className="text-[10px] text-slate-400 flex items-center gap-1 mt-0.5">
                    <ShieldCheckIcon size={12} />
                    特定のドメインのみ登録可能です
                  </p>
                )}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="password">パスワード</Label>
                <Input
                  id="password"
                  type="password"
                  placeholder="8文字以上の英数字"
                  className={`rounded-xl h-11 border-slate-200 ${errors.password ? "border-red-500" : ""}`}
                  {...register("password")}
                  disabled={isLoading}
                />
                {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="confirmPassword">パスワード（確認）</Label>
                <Input
                  id="confirmPassword"
                  type="password"
                  className={`rounded-xl h-11 border-slate-200 ${errors.confirmPassword ? "border-red-500" : ""}`}
                  {...register("confirmPassword")}
                  disabled={isLoading}
                />
                {errors.confirmPassword && <p className="text-xs text-red-500">{errors.confirmPassword.message}</p>}
              </div>
            </CardContent>
            {/* mt-4 でボタンとの間隔を確保 */}
            <CardFooter className="flex flex-col gap-4 px-8 pb-10 mt-4">
              <Button 
                type="submit" 
                className="w-full h-12 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-base font-semibold shadow-lg shadow-indigo-200"
                disabled={isLoading}
              >
                {isLoading ? <Loader2Icon className="animate-spin" /> : "アカウントを作成する"}
              </Button>
              <div className="text-center text-sm text-slate-500 pt-2">
                すでにアカウントをお持ちですか？{" "}
                <Link href="/auth/login" className="text-indigo-600 hover:text-indigo-500 font-bold underline-offset-4 hover:underline">
                  ログイン
                </Link>
              </div>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  );
}
