import apiClient from "./client";

export interface AdminUser {
  username: string;
}

export async function login(
  username: string,
  password: string
): Promise<AdminUser> {
  const response = await apiClient.post<AdminUser>("/auth/login", {
    username,
    password,
  });

  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post("/auth/logout");
}

// Returns null when the auth cookie is missing or expired.
export async function getCurrentUser(): Promise<AdminUser | null> {
  try {
    const response = await apiClient.get<AdminUser>("/auth/me");
    return response.data;
  } catch {
    return null;
  }
}
