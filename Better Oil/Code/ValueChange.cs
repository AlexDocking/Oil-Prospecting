namespace BetterOil
{
    public class ValueChange
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public double NewValue { get; internal set; }
        public ValueChange(int x, int y, double newValue)
        {
            X = x;
            Y = y;
            NewValue = newValue;
        }
    }
}