export interface ParsedBulkWord {
  original: string;
  translation: string;
  lineNumber: number;
}

const BULK_LINE_PATTERN = /^(.+?)\s*(?:\t+|\s+\|\s+|\s*;\s*|\s*,\s*|\s+[-–—]\s+|\s*:\s*)(.+)$/;

export function parseBulkText(value: string): { items: ParsedBulkWord[]; invalidCount: number } {
  const items: ParsedBulkWord[] = [];
  let invalidCount = 0;

  value.split(/\r?\n/).forEach((line, index) => {
    const trimmed = line.trim();
    if (!trimmed) return;

    const match = trimmed.match(BULK_LINE_PATTERN);
    const original = match?.[1].trim();
    const translation = match?.[2].trim();
    if (!original || !translation) {
      invalidCount++;
      return;
    }
    items.push({ original, translation, lineNumber: index + 1 });
  });

  return { items, invalidCount };
}
