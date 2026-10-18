import apiClient from "./client";
import type {
  CreateVehicleRequest,
  Vehicle,
  VehicleSortField,
} from "../types/vehicle";

export async function getVehicles(
  sortBy?: VehicleSortField,
  descending = false
): Promise<Vehicle[]> {
  const response = await apiClient.get<Vehicle[]>("/vehicles", {
    params: sortBy ? { sortBy, descending } : undefined,
  });

  return response.data;
}

export async function createVehicle(
  vehicle: CreateVehicleRequest
): Promise<void> {
  await apiClient.post("/vehicles", vehicle);
}
