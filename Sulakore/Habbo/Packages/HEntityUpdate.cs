using System.Globalization;

using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HEntityUpdate
{
    public int Index { get; set; }
    public bool IsController { get; set; }

    public HPoint Tile { get; set; }
    public HPoint? MovingTo { get; set; }

    public HSign Sign { get; set; }
    public HStance Stance { get; set; }
    public HAction Action { get; set; }
    public HDirection HeadFacing { get; set; }
    public HDirection BodyFacing { get; set; }

    public HEntityUpdate(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Index = format.Read<int>(ref packetSpan);

        Tile = new HPoint(format.Read<int>(ref packetSpan), format.Read<int>(ref packetSpan),
            float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));

        HeadFacing = (HDirection)format.Read<int>(ref packetSpan);
        BodyFacing = (HDirection)format.Read<int>(ref packetSpan);

        string action = format.ReadUTF8(ref packetSpan);
        string[] actionData = action.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (string actionInfo in actionData)
        {
            string[] actionValues = actionInfo.Split(' ');

            if (actionValues.Length < 2) continue;
            if (string.IsNullOrWhiteSpace(actionValues[0])) continue;

            switch (actionValues[0])
            {
                case "flatctrl":
                {
                    IsController = true;
                    break;
                }
                case "mv":
                {
                    string[] values = actionValues[1].Split(',');
                    if (values.Length >= 3)
                    {
                        MovingTo = new HPoint(int.Parse(values[0]), int.Parse(values[1]),
                            float.Parse(values[2], CultureInfo.InvariantCulture));
                    }
                    Action = HAction.Move;
                    break;
                }
                case "sit":
                {
                    Action = HAction.Sit;
                    Stance = HStance.Sit;
                    break;
                }
                case "lay":
                {
                    Action = HAction.Lay;
                    Stance = HStance.Lay;
                    break;
                }
                case "sign":
                {
                    Sign = (HSign)int.Parse(actionValues[1]);
                    Action = HAction.Sign;
                    break;
                }
            }
        }
    }

    public static HEntityUpdate[] Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        var updates = new HEntityUpdate[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < updates.Length; i++)
        {
            updates[i] = new HEntityUpdate(format, ref packetSpan);
        }
        return updates;
    }
}