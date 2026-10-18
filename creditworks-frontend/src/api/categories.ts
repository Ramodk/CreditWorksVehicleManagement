import apiClient from "./client";
import type { VehicleCategory } from "../types/category";

export async function getCategories(): Promise<VehicleCategory[]> {
  const response = await apiClient.get<VehicleCategory[]>(
    "/vehicle-categories"
  );

  return response.data;
}

export async function getCategoryIcons(): Promise<string[]> {
  const response = await apiClient.get<string[]>(
    "/vehicle-categories/icons"
  );

  return response.data;
}

// Replaces the whole configuration. Id 0 adds a category and
// categories left out of the list are deleted.
export async function saveCategories(
  categories: VehicleCategory[]
): Promise<VehicleCategory[]> {
  const response = await apiClient.put<VehicleCategory[]>(
    "/vehicle-categories",
    categories
  );

  return response.data;
}
