using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-ShovelStorageTargetSensor")]
    public class ShovelStorageTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var storage = GameObject.FindGameObjectWithTag("Storage");
            if (storage != null)
            {
                return new TransformTarget(storage.transform);
            }

            return new PositionTarget(agent.Transform.position);
        }
    }
}
