
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { ArrowLeft, Save } from "lucide-react";

import { getManufacturers } from "../api/manufacturers";
import { createVehicle } from "../api/vehicles";
import type { Manufacturer } from "../types/vehicle";

// Must match VehicleValidator on the API.
const EARLIEST_YEAR = 1886;
const LATEST_YEAR = new Date().getFullYear() + 1;

export default function AddVehicle() {
  const navigate = useNavigate();

  const [owner, setOwner] = useState("");
  const [manufacturerId, setManufacturerId] = useState("");
  const [year, setYear] = useState("");
  const [weight, setWeight] = useState("");

  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([]);
  const [loadingManufacturers, setLoadingManufacturers] = useState(true);
  const [manufacturerError, setManufacturerError] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function loadManufacturers() {
      try {
        const data = await getManufacturers();

        if (active) {
          setManufacturers(data);
        }
      } catch {
        if (active) {
          setManufacturerError(
            "Unable to load manufacturers. Please refresh the page."
          );
        }
      } finally {
        if (active) {
          setLoadingManufacturers(false);
        }
      }
    }

    void loadManufacturers();

    return () => {
      active = false;
    };
  }, []);

  async function handleSubmit(
    event: React.SubmitEvent<HTMLFormElement>
  ) {
    event.preventDefault();
    setError("");

    if (!owner.trim()) {
      setError("Owner's name is required.");
      return;
    }

    if (!manufacturerId) {
      setError("Please select a manufacturer.");
      return;
    }

    if (
      !/^\d{4}$/.test(year) ||
      Number(year) < EARLIEST_YEAR ||
      Number(year) > LATEST_YEAR
    ) {
      setError(
        `Year of manufacture must be between ${EARLIEST_YEAR} and ${LATEST_YEAR}.`
      );
      return;
    }

    if (
      !/^\d+(\.\d{1,2})?$/.test(weight) ||
      Number(weight) <= 0
    ) {
      setError(
        "Weight must be positive with a maximum of two decimal places."
      );
      return;
    }

    setSaving(true);

    try {
      await createVehicle({
        owner: owner.trim(),
        manufacturerId: Number(manufacturerId),
        year: Number(year),
        weight: Number(weight),
      });

      navigate("/vehicles", {
        state: { message: "Vehicle added successfully." },
      });
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const data = err.response?.data;

        if (typeof data?.detail === "string") {
          setError(data.detail);
        } else if (data?.errors) {
          const messages = Object.values(data.errors)
            .flat()
            .filter((value): value is string =>
              typeof value === "string"
            );

          setError(
            messages.join(" ") || "Please check your vehicle details."
          );
        } else {
          setError("Unable to save the vehicle. Please try again.");
        }
      } else {
        setError("An unexpected error occurred.");
      }
    } finally {
      setSaving(false);
    }
  }

  const inputClass =
    "w-full rounded-lg border border-slate-300 bg-white px-4 py-3 " +
    "outline-none focus:border-red-500 focus:ring-2 focus:ring-red-100";

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div>
        <button
          type="button"
          onClick={() => navigate("/vehicles")}
          className="mb-4 flex items-center gap-2 text-sm text-slate-500"
        >
          <ArrowLeft size={16} />
          Back to Vehicles
        </button>

        <h1 className="text-3xl font-bold text-slate-900">
          Add New Vehicle
        </h1>

        <p className="mt-2 text-slate-500">
          Enter the vehicle details below.
        </p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-6 rounded-xl border border-slate-200
          bg-white p-6 shadow-sm md:p-8"
      >
        {error && (
          <div role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">
            {error}
          </div>
        )}

        {manufacturerError && (
          <div role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">
            {manufacturerError}
          </div>
        )}

        <div>
          <label htmlFor="owner" className="mb-2 block font-medium">
            Owner's Name *
          </label>

          <input
            id="owner"
            type="text"
            value={owner}
            onChange={(e) => setOwner(e.target.value)}
            maxLength={150}
            required
            placeholder="e.g. John Smith"
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="manufacturer" className="mb-2 block font-medium">
            Manufacturer *
          </label>

          <select
            id="manufacturer"
            value={manufacturerId}
            onChange={(e) => setManufacturerId(e.target.value)}
            required
            disabled={loadingManufacturers || !!manufacturerError}
            className={inputClass}
          >
            <option value="">
              {loadingManufacturers
                ? "Loading manufacturers..."
                : "Select manufacturer"}
            </option>

            {manufacturers.map((manufacturer) => (
              <option key={manufacturer.id} value={manufacturer.id}>
                {manufacturer.name}
              </option>
            ))}
          </select>
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <div>
            <label htmlFor="year" className="mb-2 block font-medium">
              Year of Manufacture *
            </label>

            <input
              id="year"
              type="number"
              min={EARLIEST_YEAR}
              max={LATEST_YEAR}
              step="1"
              required
              value={year}
              onChange={(e) => setYear(e.target.value)}
              placeholder="e.g. 2020"
              className={inputClass}
            />
          </div>

          <div>
            <label htmlFor="weight" className="mb-2 block font-medium">
              Weight (kg) *
            </label>

            <input
              id="weight"
              type="number"
              min="0.01"
              step="0.01"
              required
              value={weight}
              onChange={(e) => setWeight(e.target.value)}
              placeholder="e.g. 1850.75"
              className={inputClass}
            />
          </div>
        </div>

        <div className="rounded-lg bg-blue-50 p-4 text-sm text-blue-800">
          The vehicle category will be assigned automatically
          based on its weight and the current category rules.
        </div>

        <div className="flex flex-wrap justify-end gap-3 border-t
          border-slate-200 pt-6"
        >
          <button
            type="button"
            onClick={() => navigate("/vehicles")}
            className="rounded-lg border border-slate-300 px-5 py-3"
          >
            Cancel
          </button>

          <button
            type="submit"
            disabled={saving || loadingManufacturers || !!manufacturerError}
            className="flex items-center gap-2 rounded-lg bg-red-600
              px-6 py-3 font-medium text-white hover:bg-red-700
              disabled:cursor-not-allowed disabled:opacity-50"
          >
            <Save size={18} />
            {saving ? "Saving..." : "Save Vehicle"}
          </button>
        </div>
      </form>
    </div>
  );
}
