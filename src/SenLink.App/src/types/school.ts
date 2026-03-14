export interface Department {
  departmentId: number;
  name: string;
  code: string;
}

export interface DepartmentListResponse {
  items: Department[];
}

export interface ClassItem {
  classId: number;
  departmentId: number;
  departmentName: string;
  fiscalYear: number;
  grade: number;
  name: string;
}

export interface ClassListResponse {
  items: ClassItem[];
}

export interface StudentProfileOnboardingRequest {
  classId: number;
  studentNumber: string;
  name: string;
  nameKana: string;
  dateOfBirth: string; // ISO string
  gender: number;
  admissionYear: number;
}

export interface TeacherProfileOnboardingRequest {
  name: string;
  nameKana: string;
  title?: string;
  officeLocation?: string;
  profileData?: any;
}
