using System.Text.Json;
using System.Text.Json.Serialization;
using Chh.Domain.Enums;

namespace Chh.Api.Json;

/// <summary>
/// Serializes <see cref="BloodGroup"/> using clinical notation ("A+", "AB-", etc.) instead of the
/// C# member name — matches PRD §8's Data Dictionary and contracts/chh-api.v1.yaml. The C# member
/// names (<c>APositive</c>, etc.) exist only because "+"/"-" aren't valid identifier characters;
/// this converter is the one place that bridges the two.
/// </summary>
public class BloodGroupJsonConverter : JsonConverter<BloodGroup>
{
    private static readonly IReadOnlyDictionary<BloodGroup, string> ToWire = new Dictionary<BloodGroup, string>
    {
        [BloodGroup.APositive] = "A+",
        [BloodGroup.ANegative] = "A-",
        [BloodGroup.BPositive] = "B+",
        [BloodGroup.BNegative] = "B-",
        [BloodGroup.OPositive] = "O+",
        [BloodGroup.ONegative] = "O-",
        [BloodGroup.ABPositive] = "AB+",
        [BloodGroup.ABNegative] = "AB-"
    };

    private static readonly IReadOnlyDictionary<string, BloodGroup> FromWire =
        ToWire.ToDictionary(kv => kv.Value, kv => kv.Key);

    /// <inheritdoc />
    public override BloodGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is not null && FromWire.TryGetValue(value, out var bloodGroup))
        {
            return bloodGroup;
        }

        throw new JsonException($"'{value}' is not a valid blood group.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BloodGroup value, JsonSerializerOptions options) =>
        writer.WriteStringValue(ToWire[value]);
}
