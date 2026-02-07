using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Brain;
using FarmingGoap.Goals;
using FarmingGoap.Managers;
using UnityEngine;
using System.Linq;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Multi-agent version with crop auction/bidding system
    /// Calculates utility for ALL available crops, submits bids, waits for auction
    /// </summary>
    [TaskCategory("GOAP/Farming")]
    [TaskDescription("Multi-agent farming goal selection dengan auction system")]
    public class SelectFarmingGoalMultiAgent : Action
    {
        [UnityEngine.Tooltip("Enable debug logging?")]
        public SharedBool enableDebugLog = true;
        
        private GoapActionProvider actionProvider;
        private NPCStats stats;
        private string lastSelectedGoal = "";
        private CropBehaviour lastTargetCrop = null;
        
        public override void OnAwake()
        {
            actionProvider = Owner.GetComponent<GoapActionProvider>();
            stats = Owner.GetComponent<NPCStats>();
        }
        
        public override TaskStatus OnUpdate()
        {
            if (actionProvider == null || stats == null)
            {
                UnityEngine.Debug.LogError($"[{Owner.name}] SelectFarmingGoalMultiAgent: Missing components!");
                return TaskStatus.Failure;
            }

            // Guard: Don't request goals if AgentType hasn't been assigned yet
            if (actionProvider.AgentType == null)
            {
                return TaskStatus.Failure;
            }

            if (CropManager.Instance == null)
            {
                UnityEngine.Debug.LogError($"[{Owner.name}] CropManager not found! Add CropManager to scene.");
                return TaskStatus.Failure;
            }

            // Get all crops and calculate utilities
            var allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            if (allCrops.Length == 0)
            {
                if (enableDebugLog.Value)
                    UnityEngine.Debug.LogWarning($"[{Owner.name}] No crops found in scene");
                return TaskStatus.Failure;
            }
            
            // DIAGNOSTIC: Log how many crops found
            if (enableDebugLog.Value && Time.frameCount % 300 == 0) // Every 5 seconds
            {
                var uniqueIDs = new System.Collections.Generic.HashSet<int>();
                foreach (var c in allCrops)
                {
                    uniqueIDs.Add(c.GetInstanceID());
                }
                UnityEngine.Debug.Log($"[{Owner.name}] Found {allCrops.Length} crops (Unique IDs: {uniqueIDs.Count})");
            }

            // Declare agentPos once at method scope to avoid CS0136 error
            Vector3 agentPos = Owner.transform.position;

            // CRITICAL FIX: Check if we have a reserved crop that still needs work
            // If yes, stick with it instead of switching to different crop!
            CropBehaviour currentReservedCrop = null;
            foreach (var crop in allCrops)
            {
                var reservedAgent = CropManager.Instance.GetReservedAgent(crop);
                if (reservedAgent == Owner.gameObject)
                {
                    currentReservedCrop = crop;
                    break;
                }
            }

            // If we have a reserved crop, check if it still needs work
            if (currentReservedCrop != null)
            {
                int stage = currentReservedCrop.GrowthStage;
                bool cropNeedsWork = (stage == 0) ||  // Empty → needs planting
                                    (stage == 1) ||   // Planted → needs watering
                                    (stage == 2) ||   // Growing → needs watering  
                                    (stage == 3);     // Ready → needs harvesting

                // If crop still needs work, CONTINUE with this crop!
                if (cropNeedsWork)
                {
                    // Determine which goal to use based on stage
                    string goalForReserved = "";
                    float utilityForReserved = 0f;

                    float distance = Vector3.Distance(agentPos, currentReservedCrop.transform.position);
                    float maxDistance = 50f;
                    float normalizedDist = Mathf.Clamp01(distance / maxDistance);
                    float distanceBonus = 0.15f * (1f - normalizedDist);

                    if (stage == 0) // Empty → Plant
                    {
                        goalForReserved = "PlantingGoal";
                        utilityForReserved = UtilityCalculator.CalculatePlantingUtility(
                            stats.Energy, stats.Hunger, stage,
                            stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitPlanting) + distanceBonus;
                    }
                    else if (stage == 1 || stage == 2) // Planted or Growing → Water
                    {
                        goalForReserved = "WateringGoal";
                        utilityForReserved = UtilityCalculator.CalculateWateringUtility(
                            stats.Energy, stats.Hunger, stage,
                            stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitWatering) + distanceBonus;
                    }
                    else if (stage == 3) // Ready → Harvest
                    {
                        goalForReserved = "HarvestingGoal";
                        utilityForReserved = UtilityCalculator.CalculateHarvestingUtility(
                            stats.Energy, stats.Hunger, stage,
                            stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitHarvesting) + distanceBonus;
                    }

                    // Request the appropriate goal for reserved crop
                    if (goalForReserved == "PlantingGoal")
                    {
                        actionProvider.RequestGoal<PlantingGoal>();
                    }
                    else if (goalForReserved == "WateringGoal")
                    {
                        actionProvider.RequestGoal<WateringGoal>();
                    }
                    else if (goalForReserved == "HarvestingGoal")
                    {
                        actionProvider.RequestGoal<HarvestingGoal>();
                    }

                    // Log if goal changes
                    if ((lastSelectedGoal != goalForReserved || lastTargetCrop != currentReservedCrop) && enableDebugLog.Value)
                    {
                        UnityEngine.Debug.Log($"[{Owner.name}] COMMITTED to reserved crop: {goalForReserved} → {currentReservedCrop.name} (U={utilityForReserved:F3}, Stage={stage})");
                        lastSelectedGoal = goalForReserved;
                        lastTargetCrop = currentReservedCrop;
                    }

                    return TaskStatus.Success;
                }
                else
                {
                    // Crop no longer needs work (shouldn't happen), release it
                    if (enableDebugLog.Value)
                        UnityEngine.Debug.Log($"[{Owner.name}] Reserved crop {currentReservedCrop.name} no longer needs work (Stage={stage}), releasing...");
                    CropManager.Instance.ReleaseCrop(currentReservedCrop, Owner.gameObject);
                    lastTargetCrop = null;
                }
            }

            // Calculate best utility for each goal type across ALL crops
            float bestPlantingUtility = float.MinValue;
            float bestWateringUtility = float.MinValue;
            float bestHarvestingUtility = float.MinValue;
            
            CropBehaviour bestPlantingCrop = null;
            CropBehaviour bestWateringCrop = null;
            CropBehaviour bestHarvestingCrop = null;

            foreach (var crop in allCrops)
            {
                // Skip if reserved by another agent
                if (!CropManager.Instance.IsCropAvailable(crop, Owner.gameObject))
                    continue;

                int stage = crop.GrowthStage;

                // Calculate base utilities dengan agent-specific weights
                float plantU = UtilityCalculator.CalculatePlantingUtility(
                    stats.Energy, stats.Hunger, stage,
                    stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitPlanting);
                    
                float waterU = UtilityCalculator.CalculateWateringUtility(
                    stats.Energy, stats.Hunger, stage,
                    stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitWatering);
                    
                float harvestU = UtilityCalculator.CalculateHarvestingUtility(
                    stats.Energy, stats.Hunger, stage,
                    stats.WeightEnergy, stats.WeightHunger, stats.GoalBenefitHarvesting);

                // Add distance bonus (nearby crops preferred for load balancing)
                float distance = Vector3.Distance(agentPos, crop.transform.position);
                float maxDistance = 50f; // Assumed max distance
                float normalizedDist = Mathf.Clamp01(distance / maxDistance);
                float distanceBonus = 0.15f * (1f - normalizedDist); // Max +0.15 bonus for nearest crop

                // Apply distance bonus to valid utilities
                if (plantU > -999f) plantU += distanceBonus;
                if (waterU > -999f) waterU += distanceBonus;
                if (harvestU > -999f) harvestU += distanceBonus;

                // Track best crop for each goal type
                if (plantU > bestPlantingUtility && plantU > -999f)
                {
                    bestPlantingUtility = plantU;
                    bestPlantingCrop = crop;
                }

                if (waterU > bestWateringUtility && waterU > -999f)
                {
                    bestWateringUtility = waterU;
                    bestWateringCrop = crop;
                }

                if (harvestU > bestHarvestingUtility && harvestU > -999f)
                {
                    bestHarvestingUtility = harvestU;
                    bestHarvestingCrop = crop;
                }
            }

            // Find overall best goal
            float maxUtility = Mathf.Max(bestPlantingUtility, bestWateringUtility, bestHarvestingUtility);

            if (maxUtility <= -999f)
            {
                // No valid goals
                return TaskStatus.Failure;
            }

            // Submit bid untuk best goal
            string selectedGoal = "";
            CropBehaviour targetCrop = null;

            if (maxUtility == bestPlantingUtility)
            {
                selectedGoal = "PlantingGoal";
                targetCrop = bestPlantingCrop;
            }
            else if (maxUtility == bestWateringUtility)
            {
                selectedGoal = "WateringGoal";
                targetCrop = bestWateringCrop;
            }
            else // harvesting
            {
                selectedGoal = "HarvestingGoal";
                targetCrop = bestHarvestingCrop;
            }

            // Only submit bid if target changed (avoid spamming bids for already-owned crop)
            bool targetChanged = (lastTargetCrop != targetCrop);
            
            // DON'T release old crop here - this was causing agents to abandon crops mid-cycle!
            // Crops are only released when:
            // 1. Agent explicitly switches to Survival/other non-farming goal (handled in Action.Stop())
            // 2. Crop is fully harvested (handled in HarvestCropAction.End())
            // 3. Crop determined to no longer need work (handled above)
            
            if (targetChanged)
            {
                if (selectedGoal == "PlantingGoal")
                    CropManager.Instance.SubmitBid(targetCrop, Owner.gameObject, maxUtility, "Planting");
                else if (selectedGoal == "WateringGoal")
                    CropManager.Instance.SubmitBid(targetCrop, Owner.gameObject, maxUtility, "Watering");
                else
                    CropManager.Instance.SubmitBid(targetCrop, Owner.gameObject, maxUtility, "Harvesting");
            }

            // Request goal (auction akan determine siapa yang benar-benar dapat crop)
            if (selectedGoal == "PlantingGoal")
            {
                actionProvider.RequestGoal<PlantingGoal>();
            }
            else if (selectedGoal == "WateringGoal")
            {
                actionProvider.RequestGoal<WateringGoal>();
            }
            else
            {
                actionProvider.RequestGoal<HarvestingGoal>();
            }

            // Log only when goal OR target crop changes
            if ((lastSelectedGoal != selectedGoal || lastTargetCrop != targetCrop) && enableDebugLog.Value)
            {
                if (targetChanged && lastTargetCrop != null)
                    UnityEngine.Debug.Log($"[{Owner.name}] NEW CROP: {selectedGoal} → {targetCrop.name} (U={maxUtility:F3}, was {lastTargetCrop.name})");
                else
                    UnityEngine.Debug.Log($"[{Owner.name}] Farming: {selectedGoal} → {targetCrop.name} (U={maxUtility:F3})");
                lastSelectedGoal = selectedGoal;
                lastTargetCrop = targetCrop;
            }

            return TaskStatus.Success;
        }
    }
}
