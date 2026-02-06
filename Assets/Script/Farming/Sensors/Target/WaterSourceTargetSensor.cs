using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-WaterSourceTargetSensor")]
    public class WaterSourceTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // Cari GameObject dengan tag "WaterSource"
            var waterSource = GameObject.FindGameObjectWithTag("WaterSource");
            if (waterSource != null)
            {
                return new TransformTarget(waterSource.transform);
            }

            // Fallback ke Storage
            var storage = GameObject.FindGameObjectWithTag("Storage");
            if (storage != null)
            {
                return new TransformTarget(storage.transform);
            }

            return new PositionTarget(agent.Transform.position);
        }
    }
}
