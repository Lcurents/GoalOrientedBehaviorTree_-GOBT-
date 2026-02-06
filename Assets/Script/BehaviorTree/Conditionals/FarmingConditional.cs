using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Conditional: Cek apakah agent bisa melakukan farming
    /// Return SUCCESS jika ada crop (untuk water/harvest) ATAU bisa plant
    /// </summary>
    [TaskCategory("GOAP/Farming")]
    [TaskDescription("Cek apakah agent bisa melakukan farming (ada crop atau bisa plant)")]
    public class FarmingConditional : Conditional
    {
        [UnityEngine.Tooltip("Apakah selalu return success? (untuk testing)")]
        public SharedBool alwaysAllow = true;
        
        public override TaskStatus OnUpdate()
        {
            // Untuk simplicity, selalu allow farming jika survival tidak urgent
            // Nanti GOAP planner akan handle detail apakah bisa plant/water/harvest
            if (alwaysAllow.Value)
            {
                return TaskStatus.Success;
            }
            
            // Optional: Cek apakah ada crop di scene
            CropBehaviour[] crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            if (crops.Length > 0)
            {
                // Ada crop, bisa water atau harvest
                return TaskStatus.Success;
            }
            
            // Tidak ada crop, tapi masih bisa plant
            return TaskStatus.Success;
        }
    }
}
