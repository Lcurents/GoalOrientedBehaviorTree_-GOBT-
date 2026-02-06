using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-CropGrowthStageSensor")]
    public class CropGrowthStageSensor : GlobalWorldSensorBase
    {
        private CropBehaviour crop;

        public override void Created()
        {
            // Find crop in scene
            crop = UnityEngine.Object.FindFirstObjectByType<CropBehaviour>();
        }

        public override SenseValue Sense()
        {
            // Refresh if null
            if (crop == null)
            {
                crop = UnityEngine.Object.FindFirstObjectByType<CropBehaviour>();
            }

            if (crop == null)
                return 0;

            return crop.GrowthStage;
        }
    }
}
