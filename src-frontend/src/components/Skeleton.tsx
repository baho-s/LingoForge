export function SkeletonCard() {
  return (
    <div className="bg-white rounded-xl p-6 shadow-sm animate-pulse">
      <div className="h-4 bg-gray-200 rounded w-1/3 mb-4" />
      <div className="h-8 bg-gray-200 rounded w-2/3 mb-2" />
      <div className="h-4 bg-gray-200 rounded w-1/2" />
    </div>
  );
}

export function SkeletonBar() {
  return (
    <div className="flex items-end gap-2 h-32">
      {Array.from({ length: 7 }).map((_, i) => (
        <div key={i} className="flex-1 bg-gray-200 rounded-t animate-pulse" style={{ height: `${30 + Math.random() * 70}%` }} />
      ))}
    </div>
  );
}
