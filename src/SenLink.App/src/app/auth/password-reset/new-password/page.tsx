"use client";

import React, { useState, useEffect, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Loader2Icon, LockIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Logo } from "@/components/brand/Logo";
import apiClient from "@/lib/api-client";
import { ApiResponse } from "@/types/api";

const resetSchema = z.object({
  password: z.string().min(8, "パスワードは8文字以上で入力してください"),
  confirmPassword: z.string().min(1, "確認用パスワードを入力してください"),
}).refine((data) => data.password === data.confirmPassword, {
  message: "パスワードが一致しません",
  path: ["confirmPassword"],
});

type ResetFormValues = z.infer<typeof resetSchema>;

function NewPasswordContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get("token") || "";
  const email = searchParams.get("email") || "";
  
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!token || !email) {
      toast.error("無効なアクセスです。最初からやり直してください。");
      router.push("/auth/password-reset");
    }
  }, [token, email, router]);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetFormValues>({
    resolver: zodResolver(resetSchema),
  });

  const onSubmit = async (values: ResetFormValues) => {
    setIsLoading(true);
    try {
      const response = await apiClient.post<ApiResponse>("/auth/password-reset/reset", {
        token, // ここにはOTPコードが入る
        email,
        newPassword: values.password,
      });

      if (response.data.success) {
        toast.success("パスワードを更新しました。新しいパスワードでログインしてください。");
        router.push("/auth/login");
      }
    } catch (error: any) {
      const message = error.response?.data?.message || "パスワードの更新に失敗しました。";
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 p-4">
      <div className="w-full max-w-md space-y-8">
        <div className="flex flex-col items-center justify-center">
          <Logo size="lg" className="mb-2" />
        </div>

        <Card className="border-none shadow-xl shadow-slate-200/50 rounded-2xl overflow-hidden pt-4">
          <CardHeader className="space-y-1 pb-8 pt-4">
            <div className="flex justify-center mb-4">
              <div className="bg-green-50 text-green-600 p-3 rounded-full">
                <LockIcon size={24} />
              </div>
            </div>
            <CardTitle className="text-2xl font-bold text-center">新しいパスワードの設定</CardTitle>
            <CardDescription className="text-center px-6">
              安全なパスワードを設定してください
            </CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="grid gap-6 px-8">
              <div className="grid gap-2">
                <Label htmlFor="password">新しいパスワード</Label>
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
                <Label htmlFor="confirmPassword">新しいパスワード（確認）</Label>
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
            <CardFooter className="px-8 pb-10 mt-6">
              <Button 
                type="submit" 
                className="w-full h-12 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-base font-semibold shadow-lg shadow-indigo-200"
                disabled={isLoading}
              >
                {isLoading ? <Loader2Icon className="animate-spin" /> : "パスワードを更新する"}
              </Button>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  );
}

export default function NewPasswordPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <NewPasswordContent />
    </Suspense>
  );
}
