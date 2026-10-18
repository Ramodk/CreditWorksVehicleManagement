import {
  Bus,
  Car,
  Caravan,
  CircleQuestionMark,
  Container,
  Forklift,
  Motorbike,
  Tractor,
  Truck,
  Van,
  type LucideIcon,
} from "lucide-react";

// Maps the API's icon keys (CategoryIcons.Allowed) to images.
const icons: Record<string, { image: LucideIcon; label: string }> = {
  car: { image: Car, label: "Car" },
  van: { image: Van, label: "Van" },
  truck: { image: Truck, label: "Truck" },
  "truck-heavy": { image: Container, label: "Heavy truck" },
  bus: { image: Bus, label: "Bus" },
  motorbike: { image: Motorbike, label: "Motorbike" },
  tractor: { image: Tractor, label: "Tractor" },
  forklift: { image: Forklift, label: "Forklift" },
  caravan: { image: Caravan, label: "Caravan" },
};

export function getCategoryIcon(key: string | null | undefined) {
  return (
    icons[key ?? ""] ?? { image: CircleQuestionMark, label: key ?? "Unknown" }
  );
}
