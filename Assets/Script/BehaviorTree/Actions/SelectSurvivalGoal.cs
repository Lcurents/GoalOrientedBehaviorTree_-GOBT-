using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Goals;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Action: Pilih goal survival menggunakan priority-based
    /// Priority: EatGoal > SleepGoal
    /// </summary>
    [TaskCategory("GOAP/Survival")]
    [TaskDescription("Pilih survival goal (Eat atau Sleep) berdasarkan priority")]
    public class SelectSurvivalGoal : Action
    {
        [UnityEngine.Tooltip("Enable debug logging?")]
        public SharedBool enableDebugLog = true;
        
        [UnityEngine.Tooltip("Threshold hunger untuk EatGoal (default: 70)")]
        public SharedFloat eatThreshold = 70f;
        
        [UnityEngine.Tooltip("Threshold energy untuk SleepGoal (default: 30)")]
        public SharedFloat sleepThreshold = 30f;
        
        private GoapActionProvider actionProvider;
        private NPCStats stats;
        private string lastSelectedGoal = ""; // Track last goal to avoid spam
        
        public override void OnAwake()
        {
            // Owner = GameObject yang attach Behavior Tree component
            actionProvider = Owner.GetComponent<GoapActionProvider>();
            stats = Owner.GetComponent<NPCStats>();
        }
        
        public override TaskStatus OnUpdate()
        {
            if (actionProvider == null || stats == null)
            {
                UnityEngine.Debug.LogError("[SelectSurvivalGoal] GoapActionProvider atau NPCStats tidak ditemukan!");
                return TaskStatus.Failure;
            }
            
            // Priority 1: EatGoal (jika hunger tinggi DAN ada food)
            if (stats.Hunger > eatThreshold.Value && stats.FoodCount > 0)
            {
                actionProvider.RequestGoal<EatGoal>();
                if (lastSelectedGoal != "EatGoal")
                {
                    if (enableDebugLog.Value)
                        UnityEngine.Debug.Log($"[BT] Survival Planner: EatGoal selected (Hunger={stats.Hunger})");
                    lastSelectedGoal = "EatGoal";
                }
                return TaskStatus.Success;
            }
            
            // Priority 2: SleepGoal (jika energy rendah)
            if (stats.Energy < sleepThreshold.Value)
            {
                actionProvider.RequestGoal<SleepGoal>();
                if (lastSelectedGoal != "SleepGoal")
                {
                    if (enableDebugLog.Value)
                        UnityEngine.Debug.Log($"[BT] Survival Planner: SleepGoal selected (Energy={stats.Energy})");
                    lastSelectedGoal = "SleepGoal";
                }
                return TaskStatus.Success;
            }
            
            // Tidak ada goal survival yang bisa dijalankan
            // Return FAILURE so BT Selector continues to farming branch
            // (Edge case: Hunger > 70 tapi FoodCount = 0 → can't eat, don't block farming)
            lastSelectedGoal = "";
            return TaskStatus.Failure;
        }
    }
}
