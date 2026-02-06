using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-SeedStorageTargetSensor")]
    public class SeedStorageTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // Untuk sekarang, gunakan posisi Storage yang sama
            var storage = GameObject.FindGameObjectWithTag("Storage");
            if (storage != null)
            {
                return new TransformTarget(storage.transform);
            }

            // Fallback ke agent position
            return new PositionTarget(agent.Transform.position);
        }
    }
}
