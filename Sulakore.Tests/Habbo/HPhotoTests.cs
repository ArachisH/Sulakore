using Sulakore.Habbo.Camera;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HPhotoTests
{
    [Fact]
    public void HPhoto_EmptySerializeDeserialize()
    {
        string emptyPhotoJson = new HPhoto().ToString();
        var deserializedPhoto = HPhoto.Create(emptyPhotoJson);

        Assert.NotNull(deserializedPhoto);
        Assert.Empty(deserializedPhoto!.Planes);
        Assert.Empty(deserializedPhoto!.Sprites);
        Assert.Equal(deserializedPhoto!.Modifiers, new Modifiers());
        Assert.Empty(deserializedPhoto!.Filters);

        Assert.Equal((uint)0, deserializedPhoto.RoomId);
        Assert.Null(deserializedPhoto.Zoom);
    }
}
