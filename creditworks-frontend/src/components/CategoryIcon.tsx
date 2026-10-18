import { getCategoryIcon } from "./categoryIcons";

interface CategoryIconProps {
  icon: string | null | undefined;
  size?: number;
  className?: string;
}

export default function CategoryIcon({
  icon,
  size = 16,
  className,
}: CategoryIconProps) {
  const { image: Image } = getCategoryIcon(icon);

  return <Image size={size} className={className} aria-hidden="true" />;
}
