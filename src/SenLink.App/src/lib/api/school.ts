import apiClient from "@/lib/api-client";
import { ApiResponse } from "@/types/api";
import { DepartmentListResponse, ClassListResponse, StudentProfileOnboardingRequest, TeacherProfileOnboardingRequest } from "@/types/school";

export const schoolApi = {
  /**
   * 学科一覧を取得する
   */
  getDepartments: async () => {
    const response = await apiClient.get<ApiResponse<DepartmentListResponse>>("/school/departments");
    return response.data;
  },

  /**
   * クラス一覧を取得する（学科ID等で絞り込み可能）
   */
  getClasses: async (departmentId?: number) => {
    const params = departmentId ? { departmentId } : {};
    const response = await apiClient.get<ApiResponse<ClassListResponse>>("/school/classes", { params });
    return response.data;
  },

  /**
   * 学生プロフィールを登録する
   */
  createStudentProfile: async (data: StudentProfileOnboardingRequest) => {
    const response = await apiClient.post<ApiResponse>("/school/students/onboarding", data);
    return response.data;
  },

  /**
   * 教員プロフィールを登録する
   */
  createTeacherProfile: async (data: TeacherProfileOnboardingRequest) => {
    const response = await apiClient.post<ApiResponse>("/school/teachers/onboarding", data);
    return response.data;
  }
};
