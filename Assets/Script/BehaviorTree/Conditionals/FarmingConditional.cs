using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Conditional: Cek apakah agent bisa melakukan farming
    /// Return SUCCESS jika ada crop yang perlu dikerjakan (plant/water/harvest)
    /// </summary>
    [TaskCategory("GOAP/Farming")]
    [TaskDescription("Cek apakah ada pekerjaan farming tersedia (plant/water/harvest)")]
    public class FarmingConditional : Conditional
    {
        [UnityEngine.Tooltip("Apakah selalu return success? (untuk testing/debugging)")]
        public SharedBool alwaysAllow = false;

        public override TaskStatus OnUpdate()
        {
            // Mode testing override
            if (alwaysAllow.Value)
                return TaskStatus.Success;

            // Cek apakah ada crop di scene yang butuh dikerjakan
            CropBehaviour[] crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);

            if (crops.Length == 0)
                return TaskStatus.Failure;

            // Cek apakah ada crop yang membutuhkan pekerjaan (MAS-aware: cek juga yang tidak reserved)
            foreach (var crop in crops)
            {
                if (crop == null) continue;

                int stage = crop.GrowthStage;

                // Stage 0: Butuh ditanam (planting)
                // Stage 1 atau 2 dengan needsWater: Butuh disiram (watering)
                // Stage 3: Siap dipanen (harvesting)
                bool needsWork = stage == 0 || crop.NeedsWater || stage == 3;

                if (needsWork)
                {
                    // Dalam MAS mode, CropManager akan handle conflict via auction
                    // BT hanya perlu tahu ada pekerjaan yang tersedia
                    return TaskStatus.Success;
                }
            }

            // Tidak ada crop yang butuh pekerjaan saat ini (semua sedang tumbuh stage 1/2)
            return TaskStatus.Failure;
        }
    }
}
