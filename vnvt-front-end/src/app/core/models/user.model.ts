export interface User {
  id: number;
  username: string;
  email: string;
  passwordHash: string;
  role?: string | null; // Role could be optional or nullable
  status?: string | null; // Status could be optional or nullable
  token: string;
  firstname?: string | null; // Firstname could be optional or nullable
  lastname?: string | null; // Lastname could be optional or nullable
  createdDate: string; // ISO 8601 string for the date
  updatedDate: string; // ISO 8601 string for the date
  lastlogindate?: string | null; // Optional or nullable last login date
}
