namespace BetterOil
{
    /// <summary>
    /// The object that is serialized/deserialized to a json file
    /// </summary>
    public class OilLayerValues
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public float[] Values { get; set; }
        public float this[int x, int y]
        {
            get
            {
                return Values[x * Height + y];
            }
        }
    }
}