using System.Globalization;

using Sulakore.Network.Protocol;

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

    public HEntity(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();
        Motto = packet.Read<string>();
        FigureId = packet.Read<string>();
        Index = packet.Read<int>();

        _tile = new HPoint(packet.Read<int>(), packet.Read<int>(),
            double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture));

        packet.Read<int>();
        EntityType = (HEntityType)packet.Read<int>();

        switch (EntityType)
        {
            case HEntityType.User:
            {
                Gender = (HGender)packet.Read<string>().ToLower()[0];
                packet.Read<int>();
                packet.Read<int>();
                FavoriteGroup = packet.Read<string>();
                packet.Read<string>();
                packet.Read<int>();
                packet.Read<bool>();
                break;
            }
            case HEntityType.Pet:
            {
                packet.Read<int>();
                packet.Read<int>();
                packet.Read<string>();
                packet.Read<int>();
                packet.Read<bool>();
                packet.Read<bool>();
                packet.Read<bool>();
                packet.Read<bool>();
                packet.Read<bool>();
                packet.Read<bool>();
                packet.Read<int>();
                packet.Read<string>();
                break;
            }
            case HEntityType.RentableBot:
            {
                packet.Read<string>();
                packet.Read<int>();
                packet.Read<string>();
                for (int j = packet.Read<int>(); j > 0; j--)
                {
                    packet.Read<short>();
                }
                break;
            }
        }
    }

    public static HEntity[] Parse(ref HReadOnlyPacket packet)
    {
        var entities = new HEntity[packet.Read<int>()];
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i] = new HEntity(ref packet);
        }
        return entities;
    }
}