import { useEffect, useState } from "react";
import {
  Link,
  useLocation,
  useNavigate,
  useSearchParams,
} from "react-router-dom";
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  CheckCircle,
  LoaderCircle,
  Plus,
  X,
} from "lucide-react";

import { getVehicles } from "../api/vehicles";
import { useAuth } from "../auth/useAuth";
import CategoryIcon from "../components/CategoryIcon";
import type { Vehicle, VehicleSortField } from "../types/vehicle";

const sortFields: VehicleSortField[] = [
  "owner",
  "manufacturer",
  "year",
  "weight",
];

const weightFormat = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

function parseSortField(value: string | null): VehicleSortField | undefined {
  return sortFields.find((field) => field === value);
}

export default function VehicleList() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { user } = useAuth();

  // Sort lives in the URL so it survives refresh and can be shared.
  const sortBy = parseSortField(searchParams.get("sort"));
  const descending = searchParams.get("dir") === "desc";

  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Read the success message passed from AddVehicle once, on load.
  const [message, setMessage] = useState<string>(
    (location.state as { message?: string } | null)?.message ?? ""
  );

  useEffect(() => {
    if (!message) return;

    // Clear router state so a refresh or Back doesn't show it again.
    navigate(location.pathname + location.search, {
      replace: true,
      state: null,
    });

    const timer = setTimeout(() => setMessage(""), 4000);

    return () => clearTimeout(timer);
    // Run only on mount: the message is a one-time event.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    let active = true;

    async function loadVehicles() {
      setLoading(true);
      setError("");

      try {
        const data = await getVehicles(sortBy, descending);

        if (active) {
          setVehicles(data);
        }
      } catch {
        if (active) {
          setError("Unable to load vehicles. Please try again.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void loadVehicles();

    // Ignore responses from an older sort if the user clicks quickly.
    return () => {
      active = false;
    };
  }, [sortBy, descending]);

  function handleSort(field: VehicleSortField) {
    const nextDescending = sortBy === field ? !descending : false;

    setSearchParams(
      { sort: field, dir: nextDescending ? "desc" : "asc" },
      { replace: true }
    );
  }

  function renderSortableHeader(
    field: VehicleSortField,
    label: string,
    align: "left" | "right" = "left"
  ) {
    const active = sortBy === field;
    const SortIcon = !active ? ArrowUpDown : descending ? ArrowDown : ArrowUp;

    return (
      <th
        scope="col"
        aria-sort={
          active ? (descending ? "descending" : "ascending") : "none"
        }
        className={`px-4 py-3 font-semibold ${
          align === "right" ? "text-right" : "text-left"
        }`}
      >
        <button
          type="button"
          onClick={() => handleSort(field)}
          className={`inline-flex items-center gap-1 hover:text-slate-900 ${
            active ? "text-slate-900" : "text-slate-600"
          }`}
        >
          {label}
          <SortIcon
            size={14}
            className={active ? "text-red-600" : "text-slate-400"}
          />
        </button>
      </th>
    );
  }

  return (
    <section className="space-y-6">
      {message && (
        <div
          role="status"
          className="flex items-center justify-between rounded-lg
            bg-green-50 p-4 text-green-700"
        >
          <span className="flex items-center gap-2">
            <CheckCircle size={18} />
            {message}
          </span>

          <button
            type="button"
            onClick={() => setMessage("")}
            aria-label="Dismiss message"
            className="rounded p-1 hover:bg-green-100"
          >
            <X size={16} />
          </button>
        </div>
      )}

      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Vehicles</h1>
          <p className="mt-2 text-slate-500">
            Click a column heading to sort.
          </p>
        </div>

        {user && (
          <Link
            to="/vehicles/new"
            className="flex items-center gap-2 rounded-lg bg-red-600 px-5
              py-3 font-medium text-white hover:bg-red-700"
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

      <div
        className="overflow-x-auto rounded-xl border border-slate-200
          bg-white shadow-sm"
      >
        <table className="w-full min-w-[640px] text-sm">
          <thead className="border-b border-slate-200 bg-slate-50">
            <tr>
              {renderSortableHeader("owner", "Owner")}
              {renderSortableHeader("manufacturer", "Manufacturer")}
              {renderSortableHeader("year", "Year", "right")}
              {renderSortableHeader("weight", "Weight (kg)", "right")}
              <th
                scope="col"
                className="px-4 py-3 text-left font-semibold text-slate-600"
              >
                Category
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-100">
            {loading && vehicles.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-10 text-center text-slate-500">
                  <span className="inline-flex items-center gap-2">
                    <LoaderCircle size={18} className="animate-spin" />
                    Loading vehicles...
                  </span>
                </td>
              </tr>
            ) : vehicles.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-10 text-center text-slate-500">
                  {error
                    ? "No vehicles to show."
                    : user
                      ? "No vehicles yet. Add your first one."
                      : "No vehicles yet."}
                </td>
              </tr>
            ) : (
              vehicles.map((vehicle) => {
                return (
                  <tr
                    key={vehicle.id}
                    className={`hover:bg-slate-50 ${
                      loading ? "opacity-60" : ""
                    }`}
                  >
                    <td className="px-4 py-3 font-medium text-slate-900">
                      {vehicle.owner}
                    </td>
                    <td className="px-4 py-3 text-slate-700">
                      {vehicle.manufacturerName}
                    </td>
                    <td className="px-4 py-3 text-right tabular-nums text-slate-700">
                      {vehicle.year}
                    </td>
                    <td className="px-4 py-3 text-right tabular-nums text-slate-700">
                      {weightFormat.format(vehicle.weight)}
                    </td>
                    <td className="px-4 py-3">
                      {vehicle.categoryName ? (
                        <span
                          className="inline-flex items-center gap-1.5
                            rounded-full bg-slate-100 px-2.5 py-1 text-xs
                            font-medium text-slate-700"
                        >
                          <CategoryIcon icon={vehicle.categoryIcon} size={14} />
                          {vehicle.categoryName}
                        </span>
                      ) : (
                        <span className="text-slate-400">—</span>
                      )}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}
