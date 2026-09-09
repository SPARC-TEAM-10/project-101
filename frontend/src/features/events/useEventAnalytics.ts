import { useState } from "react";
import { useQuery } from "@tanstack/react-query";

import {
  exportEventAttendanceCsv,
  getEventAttendanceParticipants,
  getEventAttendanceSummary,
  type AttendanceViewStatus,
} from "../../api/eventApi";
import { ApiError } from "../../api/httpClient";

export interface ExportCsvError {
  status: number | null;
  message: string;
}

// CHH-45/US-CHH-005-08 — event attendance analytics: summary stats, a status/search-filterable
// participant list, and a CSV export.
export function useEventAnalytics(accessToken: string | undefined, eventId: string) {
  const [statusFilter, setStatusFilter] = useState<AttendanceViewStatus | undefined>(undefined);
  const [search, setSearch] = useState("");
  const [isExporting, setIsExporting] = useState(false);
  const [exportError, setExportError] = useState<ExportCsvError | null>(null);

  const summaryQuery = useQuery({
    queryKey: ["events", "attendance", "summary", eventId, accessToken],
    queryFn: () => getEventAttendanceSummary(accessToken, eventId),
    enabled: Boolean(eventId),
  });

  const participantsQuery = useQuery({
    queryKey: ["events", "attendance", "participants", eventId, statusFilter, search, accessToken],
    queryFn: () => getEventAttendanceParticipants(accessToken, eventId, { status: statusFilter, search: search.trim() || undefined }),
    enabled: Boolean(eventId),
  });

  async function exportCsv() {
    setExportError(null);
    setIsExporting(true);
    try {
      const blob = await exportEventAttendanceCsv(accessToken, eventId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `event-${eventId}-attendance.csv`;
      link.click();
      URL.revokeObjectURL(url);
      return { ok: true as const };
    } catch (err) {
      const error =
        err instanceof ApiError
          ? { status: err.status, message: err.problem.detail ?? "Couldn't export attendance. Try again." }
          : { status: null, message: "Couldn't export attendance. Try again." };
      setExportError(error);
      return { ok: false as const, error };
    } finally {
      setIsExporting(false);
    }
  }

  return {
    summary: summaryQuery.data,
    isSummaryLoading: summaryQuery.isLoading,
    isSummaryError: summaryQuery.isError,
    participants: participantsQuery.data ?? [],
    isParticipantsLoading: participantsQuery.isLoading,
    statusFilter,
    setStatusFilter,
    search,
    setSearch,
    exportCsv,
    isExporting,
    exportError,
  };
}
