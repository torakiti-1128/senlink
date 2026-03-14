"use client";

import React, { useState, useEffect, Suspense } from "react";
import { useRouter } from "next/navigation";
import { useForm, useFieldArray, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { 
  Loader2Icon, UserIcon, GraduationCapIcon, BriefcaseIcon, 
  PlusIcon, Trash2Icon, ChevronRightIcon, 
  ChevronLeftIcon, GlobeIcon, GithubIcon, AwardIcon, InfoIcon
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Logo } from "@/components/brand/Logo";
import { schoolApi } from "@/lib/api/school";
import { Department, ClassItem } from "@/types/school";

const studentSchema = z.object({
  name: z.string().min(1, "氏名を入力してください"),
  nameKana: z.string().min(1, "氏名カナを入力してください"),
  studentNumber: z.string().length(8, "学籍番号は8桁である必要があります"),
  departmentId: z.string().min(1, "学科を選択してください"),
  classId: z.string().min(1, "クラスを選択してください"),
  dateOfBirth: z.string().min(1, "生年月日を入力してください"),
  gender: z.string(),
  admissionYear: z.number(),
  pr: z.string().optional(),
  academicHistories: z.array(z.object({
    schoolName: z.string(),
    startDate: z.string(),
    status: z.string(),
  })).optional(),
  skills: z.object({
    languages: z.string().optional(),
    frameworks: z.string().optional(),
  }).optional(),
  socialLinks: z.object({
    github: z.string().url().optional().or(z.literal("")),
    portfolio: z.string().url().optional().or(z.literal("")),
  }).optional(),
});

function OnboardingContent() {
  const router = useRouter();
  const [step, setStep] = useState(1);
  const [role, setRole] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [classes, setClasses] = useState<ClassItem[]>([]);

  const studentForm = useForm<z.infer<typeof studentSchema>>({
    resolver: zodResolver(studentSchema),
    defaultValues: {
      name: "",
      nameKana: "",
      studentNumber: "",
      departmentId: "",
      classId: "",
      gender: "0",
      admissionYear: new Date().getFullYear(),
      academicHistories: [],
      skills: { languages: "", frameworks: "" },
      socialLinks: { github: "", portfolio: "" }
    }
  });

  const { errors } = studentForm.formState;

  const { fields: academicFields, append: appendAcademic, remove: removeAcademic } = useFieldArray({
    control: studentForm.control,
    name: "academicHistories"
  });

  useEffect(() => {
    const savedRole = localStorage.getItem("user_role");
    const email = localStorage.getItem("user_email");

    if (!savedRole) {
      router.push("/auth/login");
      return;
    }
    setRole(savedRole);

    if (savedRole === "Student") {
      fetchMasterData();
      if (email) {
        const studentNum = email.split("@")[0];
        if (/^\d{8}$/.test(studentNum)) {
          studentForm.setValue("studentNumber", studentNum, { shouldValidate: true });
        }
      }
    }
  }, [router, studentForm]);

  const fetchMasterData = async () => {
    try {
      const deptRes = await schoolApi.getDepartments();
      setDepartments(deptRes.data.items);
    } catch (error) {
      toast.error("マスタデータの取得に失敗しました");
    }
  };

  const fetchClasses = async (deptId: string) => {
    if (!deptId) return;
    try {
      const classRes = await schoolApi.getClasses(Number(deptId));
      setClasses(classRes.data.items);
    } catch (error) {
      toast.error("クラス一覧の取得に失敗しました");
    }
  };

  const nextStep = async () => {
    const result = await studentForm.trigger(["name", "nameKana", "studentNumber", "departmentId", "classId", "dateOfBirth"]);
    if (result) {
      setStep(2);
      window.scrollTo(0, 0);
    } else {
      toast.error("未入力または不正な項目があります");
    }
  };

  const onStudentSubmit = async (values: z.infer<typeof studentSchema>) => {
    setIsLoading(true);
    try {
      const profileData = {
        academicHistories: values.academicHistories,
        skills: {
          languages: values.skills?.languages?.split(",").map(s => s.trim()).filter(s => s),
          frameworks: values.skills?.frameworks?.split(",").map(s => s.trim()).filter(s => s),
        },
        socialLinks: values.socialLinks
      };

      await schoolApi.createStudentProfile({
        ...values,
        classId: Number(values.classId),
        gender: Number(values.gender),
        dateOfBirth: new Date(values.dateOfBirth).toISOString(),
        profileData: profileData as any
      });

      toast.success("プロフィールを登録しました");
      router.push("/dashboard");
    } catch (error: any) {
      toast.error(error.response?.data?.message || "登録に失敗しました");
    } finally {
      setIsLoading(false);
    }
  };

  if (!role) return null;

  return (
    <div className="min-h-screen bg-slate-50 py-12 px-4 text-slate-900 font-sans">
      <div className="max-w-2xl mx-auto space-y-8">
        <div className="flex flex-col items-center justify-center">
          <Logo size="md" className="mb-4" />
          <h1 className="text-2xl font-bold">初期プロフィールの設定</h1>
          <div className="flex items-center gap-2 mt-4">
            <div className={`h-2 w-16 rounded-full ${step >= 1 ? 'bg-indigo-600' : 'bg-slate-200'}`} />
            <div className={`h-2 w-16 rounded-full ${step >= 2 ? 'bg-indigo-600' : 'bg-slate-200'}`} />
          </div>
        </div>

        {role === "Student" ? (
          <Card className="border-none shadow-xl shadow-slate-200/50 rounded-2xl overflow-hidden">
            <form onSubmit={studentForm.handleSubmit(onStudentSubmit)}>
              {step === 1 ? (
                <>
                  <CardHeader className="bg-white border-b border-slate-100 py-6 px-8">
                    <div className="flex items-center gap-2 text-indigo-600 mb-1">
                      <UserIcon size={20} />
                      <span className="font-bold text-sm uppercase tracking-wider">Step 1: 基本情報</span>
                    </div>
                    <CardTitle>あなたについて教えてください</CardTitle>
                  </CardHeader>
                  <CardContent className="p-8 space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-2">
                        <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">氏名</Label>
                        <Input {...studentForm.register("name")} placeholder="専修 太郎" className={`rounded-xl h-11 ${errors.name ? 'border-red-500 ring-red-100' : 'border-slate-200'}`} />
                        {errors.name && <p className="text-[10px] text-red-500 font-medium px-1">{errors.name.message}</p>}
                      </div>
                      <div className="space-y-2">
                        <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">氏名（カナ）</Label>
                        <Input {...studentForm.register("nameKana")} placeholder="センシュウ タロウ" className={`rounded-xl h-11 ${errors.nameKana ? 'border-red-500 ring-red-100' : 'border-slate-200'}`} />
                        {errors.nameKana && <p className="text-[10px] text-red-500 font-medium px-1">{errors.nameKana.message}</p>}
                      </div>
                    </div>
                    <div className="space-y-2">
                      <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">学籍番号</Label>
                      <Input {...studentForm.register("studentNumber")} readOnly className="rounded-xl h-11 bg-slate-50 text-slate-500 cursor-not-allowed border-slate-200" />
                      <p className="text-[10px] text-slate-400 px-1">※学籍番号はメールアドレスから自動設定されます</p>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-2">
                        <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">学科</Label>
                        <Controller
                          name="departmentId"
                          control={studentForm.control}
                          render={({ field }) => (
                            <Select 
                              onValueChange={(val) => { 
                                field.onChange(val); 
                                studentForm.setValue("classId", ""); 
                                fetchClasses(val); 
                              }} 
                              value={field.value || ""}
                            >
                              <SelectTrigger className={`rounded-xl h-11 ${errors.departmentId ? 'border-red-500 ring-red-100' : 'border-slate-200'}`}>
                                <SelectValue>
                                  {departments.find(d => d.departmentId.toString() === field.value)?.name || "学科を選択"}
                                </SelectValue>
                              </SelectTrigger>
                              <SelectContent>
                                {departments.map((d) => (
                                  <SelectItem key={d.departmentId} value={d.departmentId.toString()}>{d.name}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        />
                        {errors.departmentId && <p className="text-[10px] text-red-500 font-medium px-1">{errors.departmentId.message}</p>}
                      </div>
                      <div className="space-y-2">
                        <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">クラス</Label>
                        <Controller
                          name="classId"
                          control={studentForm.control}
                          render={({ field }) => (
                            <Select 
                              onValueChange={field.onChange} 
                              value={field.value || ""} 
                              disabled={!studentForm.getValues("departmentId")}
                            >
                              <SelectTrigger className={`rounded-xl h-11 ${errors.classId ? 'border-red-500 ring-red-100' : 'border-slate-200'}`}>
                                <SelectValue>
                                  {classes.find(c => c.classId.toString() === field.value)?.name || "クラスを選択"}
                                </SelectValue>
                              </SelectTrigger>
                              <SelectContent>
                                {classes.map((c) => (
                                  <SelectItem key={c.classId} value={c.classId.toString()}>{c.name}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        />
                        {errors.classId && <p className="text-[10px] text-red-500 font-medium px-1">{errors.classId.message}</p>}
                      </div>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-2">
                        <Label className="after:content-['*'] after:ml-0.5 after:text-red-500 text-slate-700">生年月日</Label>
                        <Input type="date" {...studentForm.register("dateOfBirth")} className={`rounded-xl h-11 ${errors.dateOfBirth ? 'border-red-500 ring-red-100' : 'border-slate-200'}`} />
                        {errors.dateOfBirth && <p className="text-[10px] text-red-500 font-medium px-1">{errors.dateOfBirth.message}</p>}
                      </div>
                      <div className="space-y-2">
                        <Label className="text-slate-700">性別</Label>
                        <Controller
                          name="gender"
                          control={studentForm.control}
                          render={({ field }) => (
                            <RadioGroup onValueChange={field.onChange} value={field.value || "0"} className="flex gap-4 pt-2">
                              <div className="flex items-center space-x-2"><RadioGroupItem value="1" id="m" /><Label htmlFor="m" className="cursor-pointer">男性</Label></div>
                              <div className="flex items-center space-x-2"><RadioGroupItem value="2" id="f" /><Label htmlFor="f" className="cursor-pointer">女性</Label></div>
                              <div className="flex items-center space-x-2"><RadioGroupItem value="0" id="o" /><Label htmlFor="o" className="cursor-pointer">回答しない</Label></div>
                            </RadioGroup>
                          )}
                        />
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter className="p-8 bg-slate-50 border-t border-slate-100 flex justify-end">
                    <Button type="button" onClick={nextStep} className="rounded-xl px-8 h-12 bg-indigo-600 font-bold hover:bg-indigo-700 text-white shadow-lg shadow-indigo-200 transition-all active:scale-95">
                      次へ進む <ChevronRightIcon className="ml-2" size={18} />
                    </Button>
                  </CardFooter>
                </>
              ) : (
                <>
                  <CardHeader className="bg-white border-b border-slate-100 py-6 px-8">
                    <div className="flex items-center gap-2 text-indigo-600 mb-1">
                      <AwardIcon size={20} />
                      <span className="font-bold text-sm uppercase tracking-wider">Step 2: 詳細情報</span>
                    </div>
                    <CardTitle>就活に役立つ情報を追加</CardTitle>
                    <CardDescription>これらの項目は<span className="font-bold text-indigo-600">任意入力</span>です。後からいつでも編集できます。</CardDescription>
                  </CardHeader>
                  <CardContent className="p-8 space-y-8">
                    <Alert className="bg-amber-50 border-amber-100 text-amber-800 rounded-xl border-none">
                      <div className="flex gap-2">
                        <InfoIcon size={18} className="text-amber-600 shrink-0" />
                        <AlertDescription className="text-xs font-medium">入力された情報は自動履歴書作成ツール等で活用されます。スキップして登録を完了することも可能です。</AlertDescription>
                      </div>
                    </Alert>
                    <div className="space-y-4">
                      <div className="flex items-center justify-between"><Label className="text-base font-bold text-slate-800">最終学歴</Label><Button type="button" variant="outline" size="sm" onClick={() => appendAcademic({ schoolName: "", startDate: "", status: "Graduated" })} className="rounded-lg h-8 px-3 border-indigo-200 text-indigo-600 hover:bg-indigo-50">追加</Button></div>
                      <div className="grid gap-4">
                        {academicFields.map((field, index) => (
                          <div key={field.id} className="grid grid-cols-1 md:grid-cols-12 gap-4 p-4 border border-slate-200 rounded-xl relative bg-white shadow-sm transition-all hover:border-indigo-200">
                            <div className="md:col-span-6"><Input {...studentForm.register(`academicHistories.${index}.schoolName`)} placeholder="学校名" className="h-10 border-slate-200" /></div>
                            <div className="md:col-span-4"><Input {...studentForm.register(`academicHistories.${index}.startDate`)} type="month" className="h-10 border-slate-200" /></div>
                            <div className="md:col-span-2 flex justify-end items-end"><Button type="button" variant="ghost" size="icon" onClick={() => removeAcademic(index)} className="text-red-400 hover:text-red-600 hover:bg-red-50 h-10 w-10"><Trash2Icon size={18} /></Button></div>
                          </div>
                        ))}
                      </div>
                    </div>
                    <div className="space-y-4">
                      <Label className="text-base font-bold text-slate-800">保有スキル</Label>
                      <div className="grid gap-4">
                        <div className="space-y-1.5"><Label className="text-[10px] text-slate-400 ml-1">言語 (カンマ区切り)</Label><Input {...studentForm.register("skills.languages")} placeholder="TypeScript, C#, Python" className="border-slate-200" /></div>
                        <div className="space-y-1.5"><Label className="text-[10px] text-slate-400 ml-1">FW・ツール</Label><Input {...studentForm.register("skills.frameworks")} placeholder="Next.js, ASP.NET Core, Docker" className="border-slate-200" /></div>
                      </div>
                    </div>
                    <div className="space-y-4">
                      <Label className="text-base font-bold text-slate-800">外部リンク</Label>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="flex items-center gap-2 px-3 border border-slate-200 rounded-lg bg-white h-10 focus-within:ring-1 focus-within:ring-indigo-500 transition-all"><GithubIcon size={18} className="text-slate-400" /><input {...studentForm.register("socialLinks.github")} placeholder="GitHub URL" className="w-full outline-none text-sm bg-transparent" /></div>
                        <div className="flex items-center gap-2 px-3 border border-slate-200 rounded-lg bg-white h-10 focus-within:ring-1 focus-within:ring-indigo-500 transition-all"><GlobeIcon size={18} className="text-slate-400" /><input {...studentForm.register("socialLinks.portfolio")} placeholder="ポートフォリオ URL" className="w-full outline-none text-sm bg-transparent" /></div>
                      </div>
                    </div>
                    <div className="space-y-2"><Label className="text-base font-bold text-slate-800">自己PR</Label><Textarea {...studentForm.register("pr")} placeholder="あなたの強みを自由に記入してください。" className="rounded-xl min-h-[120px] resize-none border-slate-200 transition-all focus:border-indigo-500" /></div>
                  </CardContent>
                  <CardFooter className="p-8 bg-slate-50 border-t border-slate-100 flex justify-between">
                    <Button type="button" variant="ghost" onClick={() => setStep(1)} className="font-bold text-slate-500 hover:text-slate-700 transition-all">
                      <ChevronLeftIcon className="mr-2" size={18} /> 前に戻る
                    </Button>
                    <Button type="submit" disabled={isLoading} className="rounded-xl px-10 h-12 bg-indigo-600 font-bold text-white shadow-lg shadow-indigo-200 hover:bg-indigo-700 transition-all active:scale-95">
                      {isLoading ? <Loader2Icon className="animate-spin mr-2" /> : "登録を完了する"}
                    </Button>
                  </CardFooter>
                </>
              )}
            </form>
          </Card>
        ) : (
          <div className="p-12 text-center bg-white rounded-2xl shadow-xl shadow-slate-200/50">
            <GraduationCapIcon className="mx-auto text-indigo-600 mb-4" size={48} />
            <p className="text-slate-600 font-medium text-lg">教員用オンボーディングは別途実装中です</p>
            <Button onClick={() => router.push("/dashboard")} className="mt-6 bg-slate-100 text-slate-600 hover:bg-slate-200 rounded-xl">ダッシュボードへ移動</Button>
          </div>
        )}
      </div>
    </div>
  );
}

export default function OnboardingPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center bg-slate-50"><Loader2Icon className="animate-spin text-indigo-600" size={48} /></div>}>
      <OnboardingContent />
    </Suspense>
  );
}
