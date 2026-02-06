using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-HasSeedSensor")]
    public class HasSeedSensor : GlobalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override SenseValue Sense()
        {
            // Cari NPCStats di scene
            var stats = Object.FindFirstObjectByType<NPCStats>();
            
            if (stats == null)
                return 0;

            return stats.HasSeed;
        }
    }
}
