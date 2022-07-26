using System.Collections.Generic;

namespace BetterOil
{
    public interface IOilfieldMapSetter
    {
        public void UpdateValues(IEnumerable<ValueChange> newValues);
    }
}