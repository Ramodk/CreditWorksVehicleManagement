
import { NavLink, useNavigate } from "react-router-dom";

import {
  LayoutDashboard,
  CarFront,
  PlusCircle,
  Layers3,
  LogIn,
  LogOut,
} from "lucide-react";

import { useAuth } from "../../auth/useAuth";

const navigation = [
  {
    name: "Dashboard",
    path: "/",
    icon: LayoutDashboard,
    adminOnly: false,
  },
  {
    name: "Vehicles",
    path: "/vehicles",
    icon: CarFront,
    adminOnly: false,
  },
  {
    name: "Add Vehicle",
    path: "/vehicles/new",
    icon: PlusCircle,
    adminOnly: true,
  },
  {
    name: "Categories",
    path: "/categories",
    icon: Layers3,
    adminOnly: true,
  },
];

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 rounded-lg px-4 py-3
  transition-colors ${
    isActive
      ? "bg-red-600 text-white"
      : "text-slate-300 hover:bg-slate-800"
  }`;

export default function Sidebar() {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await signOut();
    navigate("/");
  }

  return (
    <aside className="flex w-64 min-h-screen flex-col bg-slate-900 text-white p-5">
      <div className="flex items-center gap-3 mb-10">
        <CarFront className="text-red-400" size={30} />
        <div>
          <h1 className="text-xl font-bold">CreditWorks</h1>
          <p className="text-xs text-slate-400">
            Vehicle Management
          </p>
        </div>
      </div>

      <nav className="space-y-2">
        {navigation
          .filter((item) => !item.adminOnly || user)
          .map((item) => {
          const Icon = item.icon;

          return (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === "/"}
              className={linkClass}
            >
              <Icon size={20} />
              <span>{item.name}</span>
            </NavLink>
          );
        })}
      </nav>

      <div className="mt-auto border-t border-slate-800 pt-4">
        {user ? (
          <>
            <p className="px-4 pb-2 text-xs text-slate-400">
              Signed in as{" "}
              <span className="font-medium text-slate-200">{user.username}</span>
            </p>
            <button
              type="button"
              onClick={() => void handleLogout()}
              className="flex w-full items-center gap-3 rounded-lg px-4 py-3
                text-slate-300 transition-colors hover:bg-slate-800"
            >
              <LogOut size={20} />
              <span>Log out</span>
            </button>
          </>
        ) : (
          <NavLink to="/login" className={linkClass}>
            <LogIn size={20} />
            <span>Admin login</span>
          </NavLink>
        )}
      </div>
    </aside>
  );
}
