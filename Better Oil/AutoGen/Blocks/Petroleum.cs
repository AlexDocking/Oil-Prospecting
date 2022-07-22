using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Items;

namespace Eco.Mods.TechTree
{
    public class DivideValue : IDynamicValue
    {
        public float GetBaseValue => 1f;

        public ref int ControllerID => throw new System.NotImplementedException();

        public float GetCurrentValue(IDynamicValueContext context, object obj)
        {
            throw new System.NotImplementedException();
        }

        public int GetCurrentValueInt(IDynamicValueContext context, object obj, float multiplier)
        {
            throw new System.NotImplementedException();
        }
    }
    public partial class PetroleumRecipe : RecipeFamily
    {
        partial void ModsPreInitialize()
        {
            /* The new function is:
             * Time per barrel = talent discounts * (maxtime * (1 - f * oil))
            */

            //How much to dampen the curve by
            float dampener = 0.95f;
            //The value that LayerModifiedValue gives is actually (1 - oil), so we must reverse it to be able to use the oil value in the equation
            var oil = new MultiDynamicValue(MultiDynamicOps.Sum,
                    new ConstantValue(1f), new MultiDynamicValue(MultiDynamicOps.Multiply,
                        new ConstantValue(-1f), new LayerModifiedValue(Eco.Simulation.WorldLayers.LayerNames.Oilfield, 3)));
            var func = new MultiDynamicValue(MultiDynamicOps.Multiply,
                oil,
                new ConstantValue(dampener));
            //1 - (modified) oil, so that low oil gives values close to 1 meaning close to maximum time, while high values gives a low time multiplier to speed up production
            var oneMinusOil = new MultiDynamicValue(MultiDynamicOps.Sum,
                new ConstantValue(1f), new MultiDynamicValue(MultiDynamicOps.Multiply,
                        new ConstantValue(-1f), func));
            this.CraftMinutes = new MultiDynamicValue(MultiDynamicOps.Multiply,
                CreateCraftTimeValue(typeof(PetroleumRecipe), 20, typeof(OilDrillingSkill), typeof(OilDrillingFocusedSpeedTalent), typeof(OilDrillingParallelSpeedTalent)),
                oneMinusOil
            );
        }
    }
}