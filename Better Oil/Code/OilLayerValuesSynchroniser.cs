using System.Collections.Generic;

namespace BetterOil
{
    public class OilLayerValuesSynchroniser : OilfieldMapSynchroniser
    {
        public OilLayerValues Data { get; }
        public OilLayerValuesSynchroniser(OilLayerValues data)
        {
            Data = data;
        }

        public override double[,] GetValues()
        {
            double[,] values = new double[Data.Width, Data.Height];
            for (int x = 0; x < Data.Width; x++)
            {
                for (int y = 0; y < Data.Height; y++)
                {
                    values[x, y] = Data[x, y];
                }
            }
            return values;
        }

        public override void ValuesChanged(IEnumerable<ValueChange> newValues)
        {
        }
    }
}