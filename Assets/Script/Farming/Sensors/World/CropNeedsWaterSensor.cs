using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;

namespace FarmingGoap.Sensors.World
{
    /// <summary>
    /// Per-agent sensor: checks if THIS agent's reserved crop needs water.
    /// Returns 1 if crop is planted/growing (stage 1-2), 0 otherwise.
    /// </summary>
    [GoapId("Farming-CropNeedsWaterSensor")]
    public class CropNeedsWaterSensor : LocalWorldSensorBase
    {
        private CropBehaviour[] allCrops;

        public override void Created()
        {
            allCrops = UnityEngine.Object.FindObjectsByType<CropBehaviour>(UnityEngine.FindObjectsSortMode.None);
        }

        public override void Update()
        {
            if (UnityEngine.Time.frameCount % 120 == 0)
                allCrops = UnityEngine.Object.FindObjectsByType<CropBehaviour>(UnityEngine.FindObjectsSortMode.None);
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            if (CropManager.Instance == null || allCrops == null || allCrops.Length == 0)
                return 0;

            // Find the crop reserved by THIS agent
            var agentObject = agent.Transform.gameObject;
            foreach (var crop in allCrops)
            {
                if (crop == null) continue;
                var reservedAgent = CropManager.Instance.GetReservedAgent(crop);
                if (reservedAgent == agentObject)
                {
                    // Return 1 if crop needs water (planted or growing)
                    return (crop.GrowthStage >= 1 && crop.GrowthStage < 3) ? 1 : 0;
                }
            }

            // No reserved crop → doesn't need water
            return 0;
        }
    }
}
