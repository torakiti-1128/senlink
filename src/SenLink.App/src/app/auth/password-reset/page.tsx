"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Loader2Icon, ArrowLeftIcon, KeyIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Logo } from "@/components/brand/Logo";
import apiClient from "@/lib/api-client";
import { ApiResponse } from "@/types/api";
import { useRouter } from "next/navigation";

const requestSchema = z.object({
  email: z.string().email("有効なメールアドレスを入力してください"),
});

type RequestFormValues = z.infer<typeof requestSchema>;

export default function PasswordResetRequestPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RequestFormValues>({
    resolver: zodResolver(requestSchema),
  });

  const onSubmit = async (values: RequestFormValues) => {
    setIsLoading(true);
    try {
      const response = await apiClient.post<ApiResponse>("/auth/password-reset/request", {
        email: values.email,
      });

      if (response.data.success) {
        toast.success("認証コードを送信しました");
        router.push(`/auth/otp?email=${encodeURIComponent(values.email)}&purpose=PasswordReset`);
      }
    } catch (error: any) {
      toast.error("リクエストに失敗しました。メールアドレスを確認してください。");
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
              <div className="bg-amber-50 text-amber-600 p-3 rounded-full">
                <KeyIcon size={24} />
              </div>
            </div>
            <CardTitle className="text-2xl font-bold text-center">パスワードの再設定</CardTitle>
            <CardDescription className="text-center px-6">
              ご登録済みのメールアドレスを入力してください。<br />本人確認用の認証コードをお送りします。
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
            </CardContent>
            <CardFooter className="flex flex-col gap-4 px-8 pb-10 mt-6">
              <Button 
                type="submit" 
                className="w-full h-12 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-base font-semibold shadow-lg shadow-indigo-200"
                disabled={isLoading}
              >
                {isLoading ? <Loader2Icon className="animate-spin" /> : "認証コードを送信"}
              </Button>
              <Link href="/auth/login" className="flex items-center justify-center text-sm text-slate-500 hover:text-slate-700 font-medium pt-2">
                <ArrowLeftIcon size={16} className="mr-1" />
                ログイン画面に戻る
              </Link>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  );
}
