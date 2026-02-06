using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-StorageTargetSensor")]
    public class StorageTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // Find storage in scene
            var storage = GameObject.FindGameObjectWithTag("Storage");
            
            if (storage == null)
            {
                // Return agent's position as fallback
                return new PositionTarget(agent.Transform.position);
            }

            return new TransformTarget(storage.transform);
        }
    }
}
