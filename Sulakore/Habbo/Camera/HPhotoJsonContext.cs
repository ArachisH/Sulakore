using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Camera;

[JsonSerializable(typeof(HPhoto))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class HPhotoJsonContext : JsonSerializerContext
{ }