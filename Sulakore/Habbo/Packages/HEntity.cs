using System.Globalization;

using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HEntity
{
    public int Id { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public string Motto { get; set; }
    public HGender Gender { get; set; }
    public HEntityType EntityType { get; set; }
    public string FigureId { get; set; }
    public string FavoriteGroup { get; set; }

    private HPoint _tile;
    public HPoint Tile => _lastUpdate?.Tile ?? _tile;

    public HAction Action => _lastUpdate?.Action ?? HAction.None;
    public bool IsController => _lastUpdate?.IsController ?? false;

    private HEntityUpdate _lastUpdate;
    public HEntityUpdate LastUpdate
    {
        get => _lastUpdate;
        set
        {
            if (value?.Index != Index)
            {
                throw new Exception("Entity update data index does not match with current entity index.");
            }
            _lastUpdate = value;
        }
    }

    public HEntity(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);
        Motto = format.ReadUTF8(ref packetSpan);
        FigureId = format.ReadUTF8(ref packetSpan);
        Index = format.Read<int>(ref packetSpan);

        _tile = new HPoint(format.Read<int>(ref packetSpan), format.Read<int>(ref packetSpan),
            float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));

        format.Read<int>(ref packetSpan);
        EntityType = (HEntityType)format.Read<int>(ref packetSpan);

        switch (EntityType)
        {
            case HEntityType.User:
            {
                Gender = (HGender)format.ReadUTF8(ref packetSpan).ToLower()[0];
                format.Read<int>(ref packetSpan);
                format.Read<int>(ref packetSpan);
                FavoriteGroup = format.ReadUTF8(ref packetSpan);
                format.ReadUTF8(ref packetSpan);
                format.Read<int>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                break;
            }
            case HEntityType.Pet:
            {
                format.Read<int>(ref packetSpan);
                format.Read<int>(ref packetSpan);
                format.ReadUTF8(ref packetSpan);
                format.Read<int>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<bool>(ref packetSpan);
                format.Read<int>(ref packetSpan);
                format.ReadUTF8(ref packetSpan);
                break;
            }
            case HEntityType.RentableBot:
            {
                format.ReadUTF8(ref packetSpan);
                format.Read<int>(ref packetSpan);
                format.ReadUTF8(ref packetSpan);
                for (int j = format.Read<int>(ref packetSpan); j > 0; j--)
                {
                    format.Read<short>(ref packetSpan);
                }
                break;
            }
        }
    }

    public static HEntity[] Parse(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        var entities = new HEntity[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i] = new HEntity(format, ref packetSpan);
        }
        return entities;
    }
}