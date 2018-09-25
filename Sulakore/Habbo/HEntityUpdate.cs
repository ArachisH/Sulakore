using System;
using System.Globalization;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HEntityUpdate
    {
        public int Index { get; set; }
        public bool IsController { get; set; }

        public HPoint Tile { get; set; }
        public HPoint MovingTo { get; set; }

        public HSign Sign { get; set; }
        public HStance Stance { get; set; }
        public HAction Action { get; set; }
        public HDirection HeadFacing { get; set; }
        public HDirection BodyFacing { get; set; }

        public HEntityUpdate(HPacket packet)
        {
            Index = packet.ReadInt32();

            Tile = new HPoint(packet.ReadInt32(), packet.ReadInt32(),
                double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture));

            HeadFacing = (HDirection)packet.ReadInt32();
            BodyFacing = (HDirection)packet.ReadInt32();

            string action = packet.ReadUTF8();
            string[] actionData = action.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                                double.Parse(values[2], CultureInfo.InvariantCulture));
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

        public static HEntityUpdate[] Parse(HPacket packet)
        {
            var updates = new HEntityUpdate[packet.ReadInt32()];
            for (int i = 0; i < updates.Length; i++)
            {
                updates[i] = new HEntityUpdate(packet);
            }
            return updates;
        }
    }
}