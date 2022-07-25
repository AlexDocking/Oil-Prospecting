using System.Collections.Generic;

namespace BetterOil
{
    public abstract class OilfieldMapSynchroniser
    {
        public abstract void ValuesChanged(IEnumerable<ValueChange> newValues);
        public abstract double[,] GetValues();
    }
}