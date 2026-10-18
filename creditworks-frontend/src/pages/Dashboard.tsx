import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  ArrowRight,
  CarFront,
  Factory,
  Layers3,
  LoaderCircle,
  Plus,
} from "lucide-react";

import { getCategories } from "../api/categories";
import { getManufacturers } from "../api/manufacturers";
import { getVehicles } from "../api/vehicles";
import { useAuth } from "../auth/useAuth";
import CategoryIcon from "../components/CategoryIcon";
import type { VehicleCategory } from "../types/category";
import type { Manufacturer, Vehicle } from "../types/vehicle";

const RECENT_COUNT = 5;

const numberFormat = new Intl.NumberFormat(undefined, {
  maximumFractionDigits: 2,
});

function formatRange(category: VehicleCategory) {
  const min = numberFormat.format(category.minWeight);

  return category.maxWeight === null
    ? `${min} kg and above`
    : `${min} – ${numberFormat.format(category.maxWeight)} kg`;
}

function mostCommonManufacturer(vehicles: Vehicle[]) {
  const counts = new Map<string, number>();

  for (const vehicle of vehicles) {
    counts.set(
      vehicle.manufacturerName,
      (counts.get(vehicle.manufacturerName) ?? 0) + 1
    );
  }

  let top: string | null = null;
  let topCount = 0;

  for (const [name, count] of counts) {
    if (count > topCount) {
      top = name;
      topCount = count;
    }
  }

  return top;
}

export default function Dashboard() {
  const { user } = useAuth();
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([]);
  const [categories, setCategories] = useState<VehicleCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const [vehicleData, manufacturerData, categoryData] =
          await Promise.all([
            getVehicles(),
            getManufacturers(),
            getCategories(),
          ]);

        if (active) {
          setVehicles(vehicleData);
          setManufacturers(manufacturerData);
          setCategories(categoryData);
        }
      } catch {
        if (active) {
          setError("Unable to load dashboard data. Please refresh the page.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => {
      active = false;
    };
  }, []);

  const totalWeight = vehicles.reduce((sum, v) => sum + v.weight, 0);
  const averageWeight = vehicles.length ? totalWeight / vehicles.length : 0;
  const topManufacturer = mostCommonManufacturer(vehicles);

  const categoryCounts = categories.map((category) => ({
    category,
    count: vehicles.filter((v) => v.categoryName === category.name).length,
  }));
  const maxCount = Math.max(1, ...categoryCounts.map((c) => c.count));

  // Vehicles come back ordered by Id, so the newest are at the end.
  const recentVehicles = vehicles.slice(-RECENT_COUNT).reverse();

  const stats = [
    {
      title: "Total Vehicles",
      value: vehicles.length,
      detail: vehicles.length
        ? `Average weight ${numberFormat.format(averageWeight)} kg`
        : "No vehicles yet",
      icon: CarFront,
    },
    {
      title: "Manufacturers",
      value: manufacturers.length,
      detail: topManufacturer
        ? `Most common: ${topManufacturer}`
        : "No vehicles yet",
      icon: Factory,
    },
    {
      title: "Weight Categories",
      value: categories.length,
      detail: categories.map((c) => c.name).join(" · "),
      icon: Layers3,
    },
  ];

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Dashboard</h1>
          <p className="mt-2 text-slate-500">
            Manage vehicles and weight categories.
          </p>
        </div>

        {user && (
          <Link
            to="/vehicles/new"
            className="flex items-center gap-2 rounded-lg bg-red-600 px-5 py-3
              font-medium text-white hover:bg-red-700"
          >
            <Plus size={18} />
            Add Vehicle
          </Link>
        )}
      </div>

      {error && (
        <div role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">
          {error}
        </div>
      )}

      {loading ? (
        <p className="flex items-center gap-2 text-slate-500">
          <LoaderCircle size={18} className="animate-spin" />
          Loading dashboard...
        </p>
      ) : (
        !error && (
          <>
            <div className="grid gap-6 md:grid-cols-3">
              {stats.map((stat) => {
                const Icon = stat.icon;

                return (
                  <div
                    key={stat.title}
                    className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-slate-500">{stat.title}</span>
                      <Icon className="text-red-600" size={24} aria-hidden="true" />
                    </div>

                    <p className="mt-5 text-3xl font-bold tabular-nums text-slate-900">
                      {stat.value}
                    </p>

                    <p className="mt-2 truncate text-xs text-slate-500" title={stat.detail}>
                      {stat.detail}
                    </p>
                  </div>
                );
              })}
            </div>

            <div className="grid gap-6 lg:grid-cols-2">
              <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
                <div className="flex items-center justify-between">
                  <h2 className="text-xl font-semibold">Vehicles by Category</h2>
                  {user && (
                    <Link
                      to="/categories"
                      className="text-sm text-slate-500 hover:text-slate-900"
                    >
                      Edit ranges
                    </Link>
                  )}
                </div>

                <ul className="mt-5 space-y-4">
                  {categoryCounts.map(({ category, count }) => {
                    const share = vehicles.length
                      ? Math.round((count / vehicles.length) * 100)
                      : 0;

                    return (
                      <li
                        key={category.id}
                        title={`${category.name}: ${count} of ${vehicles.length} vehicles (${share}%)`}
                      >
                        <div className="flex items-center justify-between gap-3 text-sm">
                          <span className="flex min-w-0 items-center gap-2">
                            <CategoryIcon
                              icon={category.icon}
                              size={16}
                              className="shrink-0 text-slate-600"
                            />
                            <span className="font-medium text-slate-900">
                              {category.name}
                            </span>
                            <span className="truncate text-slate-500">
                              {formatRange(category)}
                            </span>
                          </span>
                          <span className="shrink-0 tabular-nums text-slate-700">
                            {count}
                            <span className="ml-1 text-slate-500">({share}%)</span>
                          </span>
                        </div>

                        {/* One series, so every bar uses the same color. */}
                        <div className="mt-2 h-2 rounded bg-slate-100" aria-hidden="true">
                          <div
                            className="h-2 rounded bg-slate-700"
                            style={{ width: `${(count / maxCount) * 100}%` }}
                          />
                        </div>
                      </li>
                    );
                  })}
                </ul>

                <p className="mt-5 text-xs text-slate-500">
                  Categories are worked out from each vehicle's weight, so
                  these counts change as soon as the ranges are edited.
                </p>
              </section>

              <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
                <div className="flex items-center justify-between">
                  <h2 className="text-xl font-semibold">Recently Added</h2>
                  <Link
                    to="/vehicles"
                    className="text-sm text-slate-500 hover:text-slate-900"
                  >
                    View all
                  </Link>
                </div>

                {recentVehicles.length === 0 ? (
                  <p className="mt-5 text-slate-500">
                    No vehicles yet.
                    {user && (
                      <>
                        {" "}
                        <Link to="/vehicles/new" className="text-red-600 hover:underline">
                          Add the first one
                        </Link>
                        .
                      </>
                    )}
                  </p>
                ) : (
                  <ul className="mt-3 divide-y divide-slate-100">
                    {recentVehicles.map((vehicle) => (
                      <li
                        key={vehicle.id}
                        className="flex items-center justify-between gap-3 py-3"
                      >
                        <div className="min-w-0">
                          <p className="truncate font-medium text-slate-900">
                            {vehicle.owner}
                          </p>
                          <p className="text-sm text-slate-500">
                            {vehicle.manufacturerName} · {vehicle.year}
                          </p>
                        </div>

                        <span
                          className="inline-flex shrink-0 items-center gap-1.5 rounded-full
                            bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700"
                        >
                          <CategoryIcon icon={vehicle.categoryIcon} size={14} />
                          {vehicle.categoryName ?? "—"}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            </div>
          </>
        )
      )}

      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-xl font-semibold">Quick Actions</h2>

        <div className="mt-5 grid gap-4 sm:grid-cols-2">
          <Link
            to="/vehicles"
            className="flex items-center justify-between rounded-lg border
              border-slate-200 p-5 hover:bg-slate-50"
          >
            View All Vehicles
            <ArrowRight size={18} />
          </Link>

          <Link
            to={user ? "/categories" : "/login"}
            state={user ? undefined : { from: "/categories" }}
            className="flex items-center justify-between rounded-lg border
              border-slate-200 p-5 hover:bg-slate-50"
          >
            {user ? "Manage Categories" : "Log in to manage categories"}
            <ArrowRight size={18} />
          </Link>
        </div>
      </div>
    </div>
  );
}
