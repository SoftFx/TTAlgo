using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(DisplayName = "[T] Drawable Objects Indicator", Version = "1.0", Category = "Test Indicator Routine")]
    public class DrawablesIndicator : Indicator
    {
        public enum UpdateMode { CreateOnce, Modify, CreateDelete }


        [Parameter(DisplayName = "Mode", DefaultValue = UpdateMode.CreateOnce)]
        public UpdateMode Mode { get; set; }

        [Parameter(DisplayName = "Count", DefaultValue = 3)]
        public int Count { get; set; }

        [Parameter(DisplayName = "Object type", DefaultValue = DrawableObjectType.VerticalLine)]
        public DrawableObjectType ObjectType { get; set; }


        protected override void Calculate(bool isNewBar)
        {
            if (!isNewBar)
            {
                var tooltip = $"{Mode} {UtcNow:HH-mm-ss.fff}";

                var firstRun = DrawableObjects.Count == 0;
                if (Mode == UpdateMode.CreateOnce && firstRun)
                {
                    for (var i = 0; i < Count; i++)
                    {
                        var objName = $"{ObjectType} {i}";
                        var obj = DrawableObjects.Create(objName, ObjectType);
                        obj.Tooltip = tooltip;
                    }
                }
                else if (Mode == UpdateMode.Modify)
                {
                    for (var i = 0; i < Count; i++)
                    {
                        var objName = $"{ObjectType} {i}";
                        if (firstRun)
                            DrawableObjects.Create(objName, ObjectType);
                        var obj = DrawableObjects[objName];
                        obj.Tooltip = tooltip;
                    }
                }
                else if (Mode == UpdateMode.CreateDelete)
                {
                    for (var i = 0; i < Count; i++)
                    {
                        var objName = $"{ObjectType} {i}";
                        if (!firstRun)
                            DrawableObjects.Remove(objName);
                        var obj = DrawableObjects.Create(objName, ObjectType);
                        obj.Tooltip = tooltip;
                    }
                }
            }
        }
    }
}
