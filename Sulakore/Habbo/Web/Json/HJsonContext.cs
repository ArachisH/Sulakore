using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Web.Json;

[JsonSerializable(typeof(HBadge))]
[JsonSerializable(typeof(HFriend))]
[JsonSerializable(typeof(HGroup))]
[JsonSerializable(typeof(HProfile))]
[JsonSerializable(typeof(HRoom))]
[JsonSerializable(typeof(HUser))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class HJsonContext : JsonSerializerContext
{ }