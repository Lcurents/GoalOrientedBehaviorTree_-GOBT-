using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-HasWateringCanSensor")]
    public class HasWateringCanSensor : GlobalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override SenseValue Sense()
        {
            var stats = Object.FindFirstObjectByType<NPCStats>();
            
            if (stats == null)
                return 0;

            return stats.HasWateringCan;
        }
    }
}
