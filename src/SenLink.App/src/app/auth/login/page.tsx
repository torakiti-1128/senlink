"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Loader2Icon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Logo } from "@/components/brand/Logo";
import apiClient from "@/lib/api-client";
import { ApiResponse, AuthResponse } from "@/types/api";

const loginSchema = z.object({
  email: z.string().email("有効なメールアドレスを入力してください"),
  password: z.string().min(1, "パスワードを入力してください"),
  remember: z.boolean().optional(),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      remember: false,
    },
  });

  const onSubmit = async (values: LoginFormValues) => {
    setIsLoading(true);
    try {
      const response = await apiClient.post<ApiResponse<AuthResponse>>("/auth/login", {
        email: values.email,
        password: values.password,
      });

      if (response.data && response.data.success) {
        const { token, role } = response.data.data;
        localStorage.setItem("auth_token", token);
        localStorage.setItem("user_role", role);
        
        toast.success("ログインしました");
        router.push("/dashboard");
      }
    } catch (error: any) {
      const message = error.response?.data?.message || "ログインに失敗しました";
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
          <p className="text-slate-500 text-sm">キャリアへの一歩を、ここから繋ぐ。</p>
        </div>

        <Card className="border-none shadow-xl shadow-slate-200/50 rounded-2xl overflow-hidden">
          <CardHeader className="space-y-1 pb-6 pt-8">
            <CardTitle className="text-2xl font-bold text-center">ログイン</CardTitle>
            <CardDescription className="text-center">
              登録済みのメールアドレスでログインしてください
            </CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="grid gap-6 px-8">
              <div className="grid gap-2">
                <Label htmlFor="email">メールアドレス</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="example@senlink.dev"
                  className={`rounded-xl h-11 border-slate-200 ${errors.email ? "border-red-500" : ""}`}
                  {...register("email")}
                  disabled={isLoading}
                />
                {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
              </div>
              <div className="grid gap-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password">パスワード</Label>
                  <Link
                    href="/auth/password-reset"
                    className="text-xs text-indigo-600 hover:text-indigo-500 font-medium"
                  >
                    パスワードをお忘れですか？
                  </Link>
                </div>
                <Input
                  id="password"
                  type="password"
                  className={`rounded-xl h-11 border-slate-200 ${errors.password ? "border-red-500" : ""}`}
                  {...register("password")}
                  disabled={isLoading}
                />
                {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
              </div>
              <div className="flex items-center space-x-2 pb-2">
                <Checkbox id="remember" className="rounded-md border-slate-300" />
                <Label htmlFor="remember" className="text-sm font-normal text-slate-600 cursor-pointer">
                  ログイン状態を保持する
                </Label>
              </div>
            </CardContent>
            <CardFooter className="flex flex-col gap-4 px-8 pb-10 mt-4">
              <Button 
                type="submit" 
                className="w-full h-12 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-base font-semibold shadow-lg shadow-indigo-200"
                disabled={isLoading}
              >
                {isLoading ? <Loader2Icon className="animate-spin" /> : "ログイン"}
              </Button>
              <div className="text-center text-sm text-slate-500 pt-2">
                アカウントをお持ちでないですか？{" "}
                <Link href="/auth/register" className="text-indigo-600 hover:text-indigo-500 font-bold underline-offset-4 hover:underline">
                  新規登録
                </Link>
              </div>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  );
}
