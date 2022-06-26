namespace OilProspecting
{
    /// <summary>
    /// The object that is serialized/deserialized to store the layer data in
    /// </summary>
    public class OilLayerValues
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public float[] Values { get; set; }
    }
}