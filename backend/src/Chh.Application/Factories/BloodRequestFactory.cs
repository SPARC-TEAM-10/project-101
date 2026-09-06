using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Factories;

/// <summary>
/// Constructs <see cref="BloodRequest"/> instances from a validated request, computing the
/// 6-hour expiry window and setting the initial <see cref="BloodRequestStatus.Matching"/> state
/// (AC1) — kept out of the entity, matching <see cref="IndividualProfileFactory"/>'s pattern.
/// </summary>
public static class BloodRequestFactory
{
    /// <summary>Creates a new blood request from a validated request.</summary>
    /// <param name="requesterMobileNumber">The authenticated requester's mobile number (from the JWT "sub" claim, never client-supplied).</param>
    /// <param name="request">The validated blood request details.</param>
    /// <param name="createdAtUtc">UTC timestamp the request is created.</param>
    public static BloodRequest Create(string requesterMobileNumber, CreateBloodRequestRequest request, DateTimeOffset createdAtUtc)
    {
        return new BloodRequest
        {
            RequesterMobileNumber = requesterMobileNumber,
            PatientName = request.PatientName.Trim(),
            BloodGroup = request.BloodGroup,
            UnitsRequired = request.UnitsRequired,
            LocationCityArea = request.LocationCityArea.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SearchRadiusKm = request.SearchRadiusKm,
            Urgency = request.Urgency,
            Status = BloodRequestStatus.Matching,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = createdAtUtc + BloodRequestConstants.RequestValidity
        };
    }
}
