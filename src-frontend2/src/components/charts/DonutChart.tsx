interface DonutChartProps {
  correct: number;
  wrong: number;
  size?: number;
}

const RADIUS = 42;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

export default function DonutChart({ correct, wrong, size = 140 }: DonutChartProps) {
  const total = correct + wrong;
  const percent = total > 0 ? Math.round((correct / total) * 100) : 0;
  const correctLength = total > 0 ? (correct / total) * CIRCUMFERENCE : 0;

  return (
    <div className="relative" style={{ width: size, height: size }}>
      <svg viewBox="0 0 100 100" className="-rotate-90" width={size} height={size} aria-hidden="true">
        <circle cx="50" cy="50" r={RADIUS} fill="none" strokeWidth="10" className="stroke-red-500/40" />
        <circle
          cx="50"
          cy="50"
          r={RADIUS}
          fill="none"
          strokeWidth="10"
          strokeLinecap="round"
          strokeDasharray={`${correctLength} ${CIRCUMFERENCE}`}
          className="stroke-amber-500 transition-all duration-500"
          style={{ display: correctLength === 0 ? 'none' : undefined }}
        />
      </svg>
      <span className="absolute inset-0 flex items-center justify-center text-2xl font-bold text-fg">%{percent}</span>
    </div>
  );
}
