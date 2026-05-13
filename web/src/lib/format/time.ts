const relativeTimeFormatter = new Intl.RelativeTimeFormat("en", { numeric: "auto" });

/**
 * Returns "Never" when `isoString` is missing, otherwise a human-readable
 * relative time like "2 minutes ago" / "in 3 hours".
 */
export function formatLastSeen(isoString: string | null | undefined): string {
  if (!isoString) return "Never";

  const date = new Date(isoString);
  if (Number.isNaN(date.getTime())) return "Unknown";

  const diffSeconds = Math.round((date.getTime() - Date.now()) / 1000);

  const ranges: Array<[number, Intl.RelativeTimeFormatUnit]> = [
    [60, "second"],
    [60, "minute"],
    [24, "hour"],
    [7, "day"],
    [4.345, "week"],
    [12, "month"],
    [Number.POSITIVE_INFINITY, "year"],
  ];

  let value = diffSeconds;
  for (const [limit, unit] of ranges) {
    if (Math.abs(value) < limit) {
      return relativeTimeFormatter.format(value, unit);
    }
    value = Math.round(value / limit);
  }
  return date.toLocaleString();
}
