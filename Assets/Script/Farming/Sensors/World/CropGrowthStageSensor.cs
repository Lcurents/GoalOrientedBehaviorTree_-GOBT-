using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;

namespace FarmingGoap.Sensors.World
{
    /// <summary>
    /// Per-agent sensor: reads the growth stage of THIS agent's reserved crop.
    /// Each agent sees its own crop's state, not a shared global state.
    /// </summary>
    [GoapId("Farming-CropGrowthStageSensor")]
    public class CropGrowthStageSensor : LocalWorldSensorBase
    {
        private CropBehaviour[] allCrops;

        public override void Created()
        {
            allCrops = UnityEngine.Object.FindObjectsByType<CropBehaviour>(UnityEngine.FindObjectsSortMode.None);
        }

        public override void Update()
        {
            // Refresh crop list periodically
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
                    return crop.GrowthStage;
                }
            }

            // No reserved crop → return 0 (empty, needs planting)
            return 0;
        }
    }
}
