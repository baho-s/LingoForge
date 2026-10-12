import { useTranslation } from 'react-i18next';

export interface BarPoint {
  date: string;
  value: number;
}

interface WeeklyBarChartProps {
  data: BarPoint[];
  height?: number;
}

export default function WeeklyBarChart({ data, height = 140 }: WeeklyBarChartProps) {
  const { i18n } = useTranslation();
  const max = Math.max(1, ...data.map((d) => d.value));
  const peak = Math.max(...data.map((d) => d.value));

  return (
    <div className="flex items-end justify-between gap-2" style={{ height: height + 28 }}>
      {data.map((d) => {
        const isPeak = d.value > 0 && d.value === peak;
        const label = new Date(d.date).toLocaleDateString(i18n.language, { weekday: 'short' });
        return (
          <div key={d.date} className="group flex flex-1 flex-col items-center justify-end gap-1.5" title={`${d.value}`}>
            <span className="text-[11px] font-semibold text-muted opacity-0 transition-opacity group-hover:opacity-100">
              {d.value}
            </span>
            <div
              className={`w-full max-w-9 rounded-t-lg transition-all ${
                isPeak ? 'bg-gradient-to-t from-amber-600 to-amber-300 shadow-[0_0_12px_rgba(245,158,11,0.45)]' : 'bg-elevated'
              }`}
              style={{ height: Math.max(4, (d.value / max) * height) }}
            />
            <span className="text-[11px] font-medium text-muted">{label}</span>
          </div>
        );
      })}
    </div>
  );
}
