export interface VehicleCategory {
  id: number;
  name: string;
  minWeight: number;
  // null means no upper limit (the last category).
  maxWeight: number | null;
  icon: string;
}
