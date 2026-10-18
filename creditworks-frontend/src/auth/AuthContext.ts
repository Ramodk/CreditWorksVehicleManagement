import { createContext } from "react";

import type { AdminUser } from "../api/auth";

export interface AuthContextValue {
  user: AdminUser | null;
  loading: boolean;
  signIn: (username: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
