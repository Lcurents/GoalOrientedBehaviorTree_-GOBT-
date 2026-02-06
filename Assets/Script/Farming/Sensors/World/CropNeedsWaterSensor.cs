using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-CropNeedsWaterSensor")]
    public class CropNeedsWaterSensor : GlobalWorldSensorBase
    {
        private CropBehaviour crop;

        public override void Created()
        {
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

            // Return 1 if crop needs water (stage >= 1 and not fully grown)
            return (crop.GrowthStage >= 1 && crop.GrowthStage < 3) ? 1 : 0;
        }
    }
}
