
export interface Manufacturer {
  id: number;
  name: string;
}

export interface CreateVehicleRequest {
  owner: string;
  manufacturerId: number;
  year: number;
  weight: number;
}

export interface Vehicle {
  id: number;
  owner: string;
  manufacturerId: number;
  manufacturerName: string;
  year: number;
  weight: number;
  categoryName: string | null;
  categoryIcon: string | null;
}

export type VehicleSortField = "owner" | "manufacturer" | "year" | "weight";
