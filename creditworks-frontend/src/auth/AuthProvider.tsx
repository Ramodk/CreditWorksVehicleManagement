import { useEffect, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import apiClient from "../api/client";
import {
  getCurrentUser,
  login,
  logout,
  type AdminUser,
} from "../api/auth";
import { AuthContext } from "./AuthContext";

export default function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();

  const [user, setUser] = useState<AdminUser | null>(null);
  const [loading, setLoading] = useState(true);

  // Restore the session from the cookie after a page refresh.
  useEffect(() => {
    let active = true;

    void getCurrentUser().then((current) => {
      if (active) {
        setUser(current);
        setLoading(false);
      }
    });

    return () => {
      active = false;
    };
  }, []);

  // If the session expires, a protected request returns 401:
  // send the admin back to the login page.
  useEffect(() => {
    const interceptor = apiClient.interceptors.response.use(
      (response) => response,
      (error) => {
        if (
          axios.isAxiosError(error) &&
          error.response?.status === 401 &&
          !error.config?.url?.startsWith("/auth/")
        ) {
          setUser(null);
          navigate("/login", {
            replace: true,
            state: { from: window.location.pathname },
          });
        }

        return Promise.reject(error);
      }
    );

    return () => apiClient.interceptors.response.eject(interceptor);
  }, [navigate]);

  async function signIn(username: string, password: string) {
    setUser(await login(username, password));
  }

  async function signOut() {
    await logout();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, loading, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}
