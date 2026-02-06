using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Brain;
using FarmingGoap.Goals;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Action: Pilih farming goal menggunakan utility-based
    /// Utility: Max(PlantingUtility, WateringUtility, HarvestingUtility)
    /// </summary>
    [TaskCategory("GOAP/Farming")]
    [TaskDescription("Pilih farming goal (Planting/Watering/Harvesting) berdasarkan utility")]
    public class SelectFarmingGoal : Action
    {
        [UnityEngine.Tooltip("Enable debug logging?")]
        public SharedBool enableDebugLog = true;
        
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
                UnityEngine.Debug.LogError("[SelectFarmingGoal] GoapActionProvider atau NPCStats tidak ditemukan!");
                return TaskStatus.Failure;
            }
            
            // Cari crop terdekat untuk determine stage
            CropBehaviour nearestCrop = FindNearestCrop();
            int cropStage = nearestCrop != null ? nearestCrop.GrowthStage : 0;
            
            // Hitung utilities untuk 3 farming goals
            float plantingUtility = UtilityCalculator.CalculatePlantingUtility(
                stats.Energy, stats.Hunger, cropStage);
            
            float wateringUtility = UtilityCalculator.CalculateWateringUtility(
                stats.Energy, stats.Hunger, cropStage);
            
            float harvestingUtility = UtilityCalculator.CalculateHarvestingUtility(
                stats.Energy, stats.Hunger, cropStage);
            
            // Pilih goal dengan utility tertinggi
            float maxUtility = Mathf.Max(plantingUtility, wateringUtility, harvestingUtility);
            
            // Jika ada goal yang valid (utility > -999)
            if (maxUtility > -999f)
            {
                string selectedGoal = "";
                
                if (maxUtility == plantingUtility)
                {
                    actionProvider.RequestGoal<PlantingGoal>();
                    selectedGoal = "PlantingGoal";
                    
                    if (lastSelectedGoal != selectedGoal)
                    {
                        if (enableDebugLog.Value)
                        {
                            UnityEngine.Debug.Log($"[BT] Farming Utilities: P={plantingUtility:F3}, W={wateringUtility:F3}, H={harvestingUtility:F3}");
                            UnityEngine.Debug.Log($"[BT] Farming Planner: PlantingGoal selected (U={plantingUtility:F3})");
                        }
                        lastSelectedGoal = selectedGoal;
                    }
                    return TaskStatus.Success;
                }
                else if (maxUtility == wateringUtility)
                {
                    actionProvider.RequestGoal<WateringGoal>();
                    selectedGoal = "WateringGoal";
                    
                    if (lastSelectedGoal != selectedGoal)
                    {
                        if (enableDebugLog.Value)
                        {
                            UnityEngine.Debug.Log($"[BT] Farming Utilities: P={plantingUtility:F3}, W={wateringUtility:F3}, H={harvestingUtility:F3}");
                            UnityEngine.Debug.Log($"[BT] Farming Planner: WateringGoal selected (U={wateringUtility:F3})");
                        }
                        lastSelectedGoal = selectedGoal;
                    }
                    return TaskStatus.Success;
                }
                else // harvestingUtility
                {
                    actionProvider.RequestGoal<HarvestingGoal>();
                    selectedGoal = "HarvestingGoal";
                    
                    if (lastSelectedGoal != selectedGoal)
                    {
                        if (enableDebugLog.Value)
                        {
                            UnityEngine.Debug.Log($"[BT] Farming Utilities: P={plantingUtility:F3}, W={wateringUtility:F3}, H={harvestingUtility:F3}");
                            UnityEngine.Debug.Log($"[BT] Farming Planner: HarvestingGoal selected (U={harvestingUtility:F3})");
                        }
                        lastSelectedGoal = selectedGoal;
                    }
                    return TaskStatus.Success;
                }
            }
            
            // Semua goal disabled (-999), fallback ke IdleGoal
            actionProvider.RequestGoal<IdleGoal>();
            if (lastSelectedGoal != "IdleGoal")
            {
                if (enableDebugLog.Value)
                    UnityEngine.Debug.Log("[BT] Farming Planner: IdleGoal selected (fallback)");
                lastSelectedGoal = "IdleGoal";
            }
            return TaskStatus.Success;
        }
        
        private CropBehaviour FindNearestCrop()
        {
            CropBehaviour[] allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            if (allCrops.Length == 0)
                return null;
            
            CropBehaviour nearest = null;
            float minDistance = float.MaxValue;
            
            foreach (var crop in allCrops)
            {
                float distance = Vector3.Distance(Owner.transform.position, crop.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = crop;
                }
            }
            
            return nearest;
        }
    }
}
