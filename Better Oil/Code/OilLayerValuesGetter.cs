using System.Collections.Generic;

namespace BetterOil
{
    public class OilLayerValuesGetter : IOilfieldMapGetter
    {
        public OilLayerValues Data { get; }
        public OilLayerValuesGetter(OilLayerValues data)
        {
            Data = data;
        }

        public double[,] GetValues()
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
    }
}