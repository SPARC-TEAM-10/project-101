import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { EMERGENCY_SEARCH_CATEGORIES, type PublicFacilityDto } from "../../api/facilityApi";
import { useFacilitySearch } from "../../features/emergency/useFacilitySearch";
import { FacilityDetailModal } from "./FacilityDetailModal";

const MAX_QUERY_LENGTH = 100;

function FacilityCard({ facility, onSelect }: { facility: PublicFacilityDto; onSelect: (id: string) => void }) {
  return (
    <button
      type="button"
      onClick={() => onSelect(facility.id)}
      className="flex w-full flex-col gap-1 rounded-md border border-line bg-cream p-4 text-left shadow-sm transition-shadow hover:shadow-md"
    >
      <div className="flex items-center justify-between gap-2">
        <b className="truncate text-[14.5px] font-bold text-ink">{facility.facilityName}</b>
        <span className="shrink-0 rounded-full bg-clay-tint px-[10px] py-[3px] text-[11.5px] font-semibold text-clay-deep">
          {facility.category}
        </span>
      </div>
      <p className="truncate text-[12.5px] text-ink-2">{facility.address}</p>
      {facility.distanceKm != null && (
        <p className="text-[12px] text-ink-3">~{facility.distanceKm.toFixed(1)}km away</p>
      )}
    </button>
  );
}

/**
 * Emergency Services Hub search screen (CHH-69/US-CHH-001-01, Epic CHH-68): search bar, category
 * filter, proximity-sorted results. Guest-accessible (any authenticated role, per the backend's
 * [Authorize] with no role restriction) — see contracts/chh-api.v1.yaml's GET /facilities/search.
 */
export function EmergencyHubPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const { q, setQ, category, setCategory, geolocationStatus, results, status, clearFilters, refetch } =
    useFacilitySearch(session?.token);
  const [selectedFacilityId, setSelectedFacilityId] = useState<string | null>(null);

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <button
          type="button"
          onClick={() => navigate(-1)}
          aria-label="Back"
          className="flex h-9 w-9 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="text-[15px] font-bold">Emergency Services Hub</b>
      </div>

      <div className="mx-auto flex max-w-xl flex-col gap-4 px-4 py-6 lg:px-0">
        <div>
          <label htmlFor="emergency-search-q" className="mb-1 block text-[12.5px] font-semibold text-ink-2">
            Search by name or area
          </label>
          <input
            id="emergency-search-q"
            type="text"
            value={q}
            onChange={(e) => setQ(e.target.value)}
            maxLength={MAX_QUERY_LENGTH}
            placeholder="e.g. City Hospital"
            className="h-11 w-full rounded-md border border-line-strong bg-cream px-3.5 text-[14px] text-ink outline-none focus:border-clay"
          />
        </div>

        <div className="flex gap-2 overflow-x-auto pb-1">
          <button
            type="button"
            onClick={() => setCategory(undefined)}
            className={`shrink-0 rounded-full px-3.5 py-1.5 text-[13px] font-semibold ${
              category === undefined ? "bg-clay text-white" : "bg-sand-2 text-ink-2"
            }`}
          >
            All
          </button>
          {EMERGENCY_SEARCH_CATEGORIES.map((value) => (
            <button
              key={value}
              type="button"
              onClick={() => setCategory(value)}
              className={`shrink-0 rounded-full px-3.5 py-1.5 text-[13px] font-semibold ${
                category === value ? "bg-clay text-white" : "bg-sand-2 text-ink-2"
              }`}
            >
              {value}
            </button>
          ))}
        </div>

        {geolocationStatus === "denied" && (
          <div className="rounded-sm border border-amber bg-amber-tint px-4 py-3 text-[13px] text-amber">
            <label htmlFor="emergency-manual-location" className="mb-1 block font-semibold">
              Enter City/Area manually
            </label>
            <input
              id="emergency-manual-location"
              type="text"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              maxLength={MAX_QUERY_LENGTH}
              placeholder="e.g. Kochi"
              className="h-10 w-full rounded-md border border-line-strong bg-cream px-3 text-[13.5px] text-ink outline-none focus:border-clay"
            />
          </div>
        )}

        {status === "loading" && <p className="text-sm text-ink-2">Loading…</p>}

        {status === "error" && (
          <div className="flex flex-col gap-2 rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
            <p>Couldn&apos;t load results. Try again.</p>
            <button
              type="button"
              onClick={() => refetch()}
              className="w-fit rounded-md border-[1.5px] border-error px-3 py-1.5 text-[13px] font-semibold text-error"
            >
              Retry
            </button>
          </div>
        )}

        {status === "empty" && (
          <div className="flex flex-col items-center rounded-md border border-dashed border-line-strong bg-cream px-4 py-10 text-center">
            <h3 className="mb-1 text-base font-bold text-ink">No results found</h3>
            <p className="max-w-[36ch] text-[13px] leading-[19px] text-ink-2">
              Try a broader area or a different category.
            </p>
            <button
              type="button"
              onClick={clearFilters}
              className="mt-3 rounded-full border border-line-strong bg-cream px-3.5 py-1.5 text-[13px] font-semibold text-ink"
            >
              Clear Filters
            </button>
          </div>
        )}

        {status === "list" && (
          <div className="flex flex-col gap-2">
            {results.map((facility) => (
              <FacilityCard key={facility.id} facility={facility} onSelect={setSelectedFacilityId} />
            ))}
          </div>
        )}
      </div>

      {selectedFacilityId && (
        <FacilityDetailModal facilityId={selectedFacilityId} onClose={() => setSelectedFacilityId(null)} />
      )}
    </div>
  );
}
