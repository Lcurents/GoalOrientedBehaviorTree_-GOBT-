using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Sensors.World
{
    /// <summary>
    /// Per-agent sensor: reads THIS agent's watering can inventory.
    /// </summary>
    [GoapId("Farming-HasWateringCanSensor")]
    public class HasWateringCanSensor : LocalWorldSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var stats = references.GetCachedComponent<NPCStats>();
            if (stats == null) return 0;
            return stats.HasWateringCan;
        }
    }
}
