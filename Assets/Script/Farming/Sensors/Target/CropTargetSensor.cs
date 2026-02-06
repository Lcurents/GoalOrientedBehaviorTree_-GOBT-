using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-CropTargetSensor")]
    public class CropTargetSensor : LocalTargetSensorBase
    {
        private CropBehaviour[] crops;

        public override void Created()
        {
            // Find all crops in scene (bukan dari agent!)
            crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
        }

        public override void Update()
        {
            // Refresh crop list periodically
            if (Time.frameCount % 120 == 0) // Every 2 seconds
            {
                crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            }
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (crops == null || crops.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[CropTargetSensor] No crops found in scene!");
                return null;
            }

            // Return first crop found (for single crop setup)
            // Later: find nearest crop
            return new TransformTarget(crops[0].transform);
        }
    }
}
