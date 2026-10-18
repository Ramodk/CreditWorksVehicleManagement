
import apiClient from "./client";
import type { Manufacturer } from "../types/vehicle";

export async function getManufacturers(): Promise<Manufacturer[]> {
  const response = await apiClient.get<Manufacturer[]>(
    "/manufacturers"
  );

  return response.data;
}
