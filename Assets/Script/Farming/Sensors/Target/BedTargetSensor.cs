using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-BedTargetSensor")]
    public class BedTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // Find bed in scene
            var bed = GameObject.FindGameObjectWithTag("Bed");
            
            if (bed == null)
            {
                // Return agent's position as fallback
                return new PositionTarget(agent.Transform.position);
            }

            return new TransformTarget(bed.transform);
        }
    }
}
