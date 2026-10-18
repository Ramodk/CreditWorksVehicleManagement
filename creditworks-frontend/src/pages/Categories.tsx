import { useEffect, useState } from "react";
import axios from "axios";
import {
  CheckCircle,
  Info,
  LoaderCircle,
  Plus,
  RotateCcw,
  Save,
  Trash,
} from "lucide-react";

import {
  getCategories,
  getCategoryIcons,
  saveCategories,
} from "../api/categories";
import CategoryIcon from "../components/CategoryIcon";
import { getCategoryIcon } from "../components/categoryIcons";
import type { VehicleCategory } from "../types/category";

// One editable category. Only the upper limit is edited: each
// category starts where the previous one ends, so the form
// cannot produce gaps or overlaps.
interface CategoryRow {
  key: string;
  id: number;
  name: string;
  icon: string;
  max: string;
}

interface RowErrors {
  name?: string;
  icon?: string;
  max?: string;
}

const weightPattern = /^\d+(\.\d{1,2})?$/;

const weightFormat = new Intl.NumberFormat(undefined, {
  maximumFractionDigits: 2,
});

function toRows(categories: VehicleCategory[]): CategoryRow[] {
  return [...categories]
    .sort((a, b) => a.minWeight - b.minWeight)
    .map((category) => ({
      key: `existing-${category.id}`,
      id: category.id,
      name: category.name,
      icon: category.icon,
      max: category.maxWeight?.toString() ?? "",
    }));
}

function isLast(rows: CategoryRow[], index: number) {
  return index === rows.length - 1;
}

// The lower limit of a row is the previous row's upper limit.
function minFor(rows: CategoryRow[], index: number): number | null {
  if (index === 0) return 0;

  const previous = rows[index - 1].max;

  return weightPattern.test(previous) ? Number(previous) : null;
}

function toRequest(rows: CategoryRow[]): VehicleCategory[] {
  return rows.map((row, index) => ({
    id: row.id,
    name: row.name.trim(),
    icon: row.icon,
    minWeight: minFor(rows, index) ?? 0,
    maxWeight: isLast(rows, index) ? null : Number(row.max),
  }));
}

// Comparable snapshot, ignoring the unused limit on the last row.
function snapshot(rows: CategoryRow[]) {
  return JSON.stringify(
    rows.map((row, index) => [
      row.id,
      row.name.trim(),
      row.icon,
      isLast(rows, index) ? "" : row.max,
    ])
  );
}

function validate(rows: CategoryRow[]): RowErrors[] {
  return rows.map((row, index) => {
    const errors: RowErrors = {};
    const name = row.name.trim();

    if (!name) {
      errors.name = "Name is required.";
    } else if (name.length > 100) {
      errors.name = "Name cannot exceed 100 characters.";
    } else if (
      rows.some(
        (other, otherIndex) =>
          otherIndex !== index &&
          other.name.trim().toLowerCase() === name.toLowerCase()
      )
    ) {
      errors.name = "Another category already uses this name.";
    }

    if (!row.icon) {
      errors.icon = "Choose an icon.";
    }

    if (!isLast(rows, index)) {
      const min = minFor(rows, index);

      if (!row.max) {
        errors.max = "Enter the upper limit.";
      } else if (!weightPattern.test(row.max)) {
        errors.max = "Use a positive number with up to two decimal places.";
      } else if (min !== null && Number(row.max) <= min) {
        errors.max = `Must be more than ${weightFormat.format(min)} kg.`;
      }
    }

    return errors;
  });
}

function getErrorMessages(err: unknown): string[] {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data;

    if (Array.isArray(data?.errors) && data.errors.length > 0) {
      return data.errors.filter(
        (value: unknown): value is string => typeof value === "string"
      );
    }

    if (typeof data?.detail === "string") {
      return [data.detail];
    }
  }

  return ["Unable to save the categories. Please try again."];
}

export default function Categories() {
  const [savedRows, setSavedRows] = useState<CategoryRow[]>([]);
  const [rows, setRows] = useState<CategoryRow[]>([]);
  const [icons, setIcons] = useState<string[]>([]);

  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [saving, setSaving] = useState(false);
  const [showErrors, setShowErrors] = useState(false);
  const [serverErrors, setServerErrors] = useState<string[]>([]);
  const [message, setMessage] = useState("");

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const [categories, iconKeys] = await Promise.all([
          getCategories(),
          getCategoryIcons(),
        ]);

        if (active) {
          const loaded = toRows(categories);
          setSavedRows(loaded);
          setRows(loaded);
          setIcons(iconKeys);
        }
      } catch {
        if (active) {
          setLoadError(
            "Unable to load categories. Please refresh the page."
          );
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

  const rowErrors = validate(rows);
  const hasErrors = rowErrors.some((errors) => Object.keys(errors).length > 0);
  const isDirty = snapshot(rows) !== snapshot(savedRows);

  function updateRow(key: string, changes: Partial<CategoryRow>) {
    setMessage("");
    setRows((current) =>
      current.map((row) => (row.key === key ? { ...row, ...changes } : row))
    );
  }

  function addRow() {
    setMessage("");
    setRows((current) => [
      ...current,
      { key: crypto.randomUUID(), id: 0, name: "", icon: "", max: "" },
    ]);
  }

  function deleteRow(key: string) {
    setMessage("");
    setRows((current) => current.filter((row) => row.key !== key));
  }

  function discardChanges() {
    setRows(savedRows);
    setShowErrors(false);
    setServerErrors([]);
    setMessage("");
  }

  async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setServerErrors([]);
    setMessage("");
    setShowErrors(true);

    if (hasErrors) return;

    setSaving(true);

    try {
      const saved = toRows(await saveCategories(toRequest(rows)));

      setSavedRows(saved);
      setRows(saved);
      setShowErrors(false);
      setMessage(
        "Categories saved. The vehicle list now uses the new ranges."
      );
    } catch (err) {
      setServerErrors(getErrorMessages(err));
    } finally {
      setSaving(false);
    }
  }

  const inputClass =
    "w-full rounded-lg border bg-white px-3 py-2 outline-none " +
    "focus:border-red-500 focus:ring-2 focus:ring-red-100";

  if (loading) {
    return (
      <p className="flex items-center gap-2 text-slate-500">
        <LoaderCircle size={18} className="animate-spin" />
        Loading categories...
      </p>
    );
  }

  if (loadError) {
    return (
      <div role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">
        {loadError}
      </div>
    );
  }

  return (
    <section className="mx-auto max-w-4xl space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-slate-900">
          Category Management
        </h1>
        <p className="mt-2 text-slate-500">
          Vehicles are placed in a category by weight. Changes apply to
          all existing vehicles as soon as you save.
        </p>
      </div>

      <div className="flex gap-3 rounded-lg bg-blue-50 p-4 text-sm text-blue-800">
        <Info size={18} className="mt-0.5 shrink-0" />
        <div className="space-y-1">
          <p>
            Each category starts where the previous one ends, so there
            are never gaps or overlaps. The last category has no upper
            limit.
          </p>
          <p>
            A range includes its lower limit but not its upper limit: a
            vehicle at exactly 500 kg belongs to the category that starts
            at 500 kg.
          </p>
          <p>
            Deleting a category extends the next one to cover its range.
          </p>
        </div>
      </div>

      {message && (
        <div
          role="status"
          className="flex items-center gap-2 rounded-lg bg-green-50 p-4 text-green-700"
        >
          <CheckCircle size={18} />
          {message}
        </div>
      )}

      {serverErrors.length > 0 && (
        <div role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">
          <p className="font-medium">The categories were not saved:</p>
          <ul className="mt-1 list-disc pl-5">
            {serverErrors.map((error) => (
              <li key={error}>{error}</li>
            ))}
          </ul>
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        {rows.map((row, index) => {
          const errors = showErrors ? rowErrors[index] : {};
          const min = minFor(rows, index);
          const last = isLast(rows, index);
          const label = row.name.trim() || `Category ${index + 1}`;

          return (
            <fieldset
              key={row.key}
              className="space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
            >
              <legend className="sr-only">{label}</legend>

              <div className="flex items-start gap-3">
                <span
                  className="mt-7 flex h-9 w-9 shrink-0 items-center justify-center
                    rounded-full bg-slate-100 text-slate-700"
                >
                  <CategoryIcon icon={row.icon} size={18} />
                </span>

                <div className="flex-1">
                  <label
                    htmlFor={`name-${row.key}`}
                    className="mb-1 block text-sm font-medium"
                  >
                    Name
                  </label>
                  <input
                    id={`name-${row.key}`}
                    value={row.name}
                    maxLength={100}
                    onChange={(e) => updateRow(row.key, { name: e.target.value })}
                    aria-invalid={!!errors.name}
                    aria-describedby={errors.name ? `name-error-${row.key}` : undefined}
                    placeholder="e.g. Light"
                    className={`${inputClass} ${
                      errors.name ? "border-red-400" : "border-slate-300"
                    }`}
                  />
                  {errors.name && (
                    <p id={`name-error-${row.key}`} className="mt-1 text-sm text-red-600">
                      {errors.name}
                    </p>
                  )}
                </div>

                <button
                  type="button"
                  onClick={() => deleteRow(row.key)}
                  disabled={rows.length === 1}
                  aria-label={`Delete ${label}`}
                  title={
                    rows.length === 1
                      ? "At least one category is required"
                      : `Delete ${label}`
                  }
                  className="mt-7 rounded-lg p-2 text-slate-500 hover:bg-red-50
                    hover:text-red-600 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <Trash size={18} />
                </button>
              </div>

              <fieldset>
                <legend className="mb-1 text-sm font-medium">Icon</legend>
                <div className="flex flex-wrap gap-2">
                  {icons.map((icon) => (
                    <label key={icon} title={getCategoryIcon(icon).label}>
                      <input
                        type="radio"
                        name={`icon-${row.key}`}
                        value={icon}
                        checked={row.icon === icon}
                        onChange={() => updateRow(row.key, { icon })}
                        className="peer sr-only"
                      />
                      <span
                        className="flex h-10 w-10 cursor-pointer items-center justify-center
                          rounded-lg border border-slate-300 text-slate-600
                          hover:bg-slate-50 peer-checked:border-red-500
                          peer-checked:bg-red-50 peer-checked:text-red-600
                          peer-focus-visible:ring-2 peer-focus-visible:ring-red-200"
                      >
                        <CategoryIcon icon={icon} size={20} />
                        <span className="sr-only">{getCategoryIcon(icon).label}</span>
                      </span>
                    </label>
                  ))}
                </div>
                {errors.icon && (
                  <p className="mt-1 text-sm text-red-600">{errors.icon}</p>
                )}
              </fieldset>

              <div>
                <span className="mb-1 block text-sm font-medium">
                  Weight range
                </span>
                <div className="flex flex-wrap items-center gap-2 text-slate-700">
                  <span>
                    From{" "}
                    <strong className="tabular-nums">
                      {min === null ? "—" : weightFormat.format(min)}
                    </strong>{" "}
                    kg
                  </span>

                  {last ? (
                    <span>and above</span>
                  ) : (
                    <>
                      <label htmlFor={`max-${row.key}`}>up to</label>
                      <input
                        id={`max-${row.key}`}
                        type="number"
                        inputMode="decimal"
                        min="0.01"
                        step="0.01"
                        value={row.max}
                        onChange={(e) => updateRow(row.key, { max: e.target.value })}
                        aria-invalid={!!errors.max}
                        aria-describedby={errors.max ? `max-error-${row.key}` : undefined}
                        className={`${inputClass} w-36 tabular-nums ${
                          errors.max ? "border-red-400" : "border-slate-300"
                        }`}
                      />
                      <span>kg</span>
                    </>
                  )}
                </div>
                {errors.max && (
                  <p id={`max-error-${row.key}`} className="mt-1 text-sm text-red-600">
                    {errors.max}
                  </p>
                )}
              </div>
            </fieldset>
          );
        })}

        <button
          type="button"
          onClick={addRow}
          className="flex w-full items-center justify-center gap-2 rounded-xl
            border-2 border-dashed border-slate-300 py-4 font-medium
            text-slate-600 hover:border-slate-400 hover:bg-white"
        >
          <Plus size={18} />
          Add category
        </button>

        <div className="flex flex-wrap items-center justify-end gap-3 border-t border-slate-200 pt-6">
          {isDirty && (
            <span className="mr-auto text-sm text-amber-700">
              You have unsaved changes.
            </span>
          )}

          <button
            type="button"
            onClick={discardChanges}
            disabled={!isDirty || saving}
            className="flex items-center gap-2 rounded-lg border border-slate-300
              px-5 py-3 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <RotateCcw size={16} />
            Discard changes
          </button>

          <button
            type="submit"
            disabled={!isDirty || saving}
            className="flex items-center gap-2 rounded-lg bg-red-600 px-6 py-3
              font-medium text-white hover:bg-red-700
              disabled:cursor-not-allowed disabled:opacity-50"
          >
            <Save size={18} />
            {saving ? "Saving..." : "Save categories"}
          </button>
        </div>
      </form>
    </section>
  );
}
