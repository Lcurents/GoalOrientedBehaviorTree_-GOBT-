using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-RandomWanderTargetSensor")]
    public class RandomWanderTargetSensor : LocalTargetSensorBase
    {
        private float lastUpdateTime;
        private Vector3 currentTarget;

        public override void Created()
        {
            lastUpdateTime = 0f;
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // Update target setiap 5 detik
            if (Time.time - lastUpdateTime > 5f)
            {
                // Random position di sekitar agent (radius 5 unit)
                Vector2 randomOffset = Random.insideUnitCircle * 5f;
                currentTarget = agent.Transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                lastUpdateTime = Time.time;
            }

            return new PositionTarget(currentTarget);
        }
    }
}
