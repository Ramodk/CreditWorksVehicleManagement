import {
  BrowserRouter,
  Routes,
  Route,
} from "react-router-dom";

import AuthProvider from "./auth/AuthProvider";
import RequireAuth from "./auth/RequireAuth";
import MainLayout from "./components/layout/MainLayout";
import Dashboard from "./pages/Dashboard";
import VehicleList from "./pages/VehicleList";
import AddVehicle from "./pages/AddVehicle";
import Categories from "./pages/Categories";
import Login from "./pages/Login";

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/vehicles" element={<VehicleList />} />
            <Route
              path="/vehicles/new"
              element={
                <RequireAuth>
                  <AddVehicle />
                </RequireAuth>
              }
            />
            <Route
              path="/categories"
              element={
                <RequireAuth>
                  <Categories />
                </RequireAuth>
              }
            />
            <Route path="/login" element={<Login />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
