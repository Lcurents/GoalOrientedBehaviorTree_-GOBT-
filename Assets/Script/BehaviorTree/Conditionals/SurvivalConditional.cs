using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Conditional: Cek apakah agent perlu survival (eat/sleep)
    /// Return SUCCESS jika Hunger > 70 OR Energy < 30
    /// </summary>
    [TaskCategory("GOAP/Survival")]
    [TaskDescription("Cek apakah agent butuh eat atau sleep (survival urgent)")]
    public class SurvivalConditional : Conditional
    {
        [UnityEngine.Tooltip("Threshold hunger untuk trigger survival (default: 70)")]
        public SharedFloat hungerThreshold = 70f;
        
        [UnityEngine.Tooltip("Threshold energy untuk trigger survival (default: 30)")]
        public SharedFloat energyThreshold = 30f;
        
        private NPCStats stats;
        
        public override void OnAwake()
        {
            // Owner = GameObject yang attach Behavior Tree component
            stats = Owner.GetComponent<NPCStats>();
        }
        
        public override TaskStatus OnUpdate()
        {
            if (stats == null)
            {
                UnityEngine.Debug.LogError("[SurvivalConditional] NPCStats tidak ditemukan!");
                return TaskStatus.Failure;
            }
            
            // Cek kondisi survival
            bool needsFood = stats.Hunger > hungerThreshold.Value && stats.FoodCount > 0; // Only trigger if agent CAN eat
            bool needsSleep = stats.Energy < energyThreshold.Value;
            
            if (needsFood || needsSleep)
            {
                // Debug.Log($"[BT] Survival Check: URGENT (Hunger={stats.Hunger}, Energy={stats.Energy})");
                return TaskStatus.Success; // Survival diperlukan
            }
            
            return TaskStatus.Failure; // Kondisi aman, tidak perlu survival
        }
    }
}
