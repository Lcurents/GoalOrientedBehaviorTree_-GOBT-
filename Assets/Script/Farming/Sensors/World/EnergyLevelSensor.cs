using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-EnergyLevelSensor")]
    public class EnergyLevelSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var stats = references.GetCachedComponent<NPCStats>();
            
            if (stats == null)
                return 0;

            return (int)stats.Energy;
        }
    }
}
