using System.Collections.Generic;

namespace BetterOil
{
    public abstract class OilfieldMapSetter
    {
        public abstract void UpdateValues(IEnumerable<ValueChange> newValues);
    }
}