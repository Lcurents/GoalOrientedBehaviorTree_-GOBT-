using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Brain;
using FarmingGoap.Goals;
using FarmingGoap.Managers;
using UnityEngine;
using System.Linq;
using System.Text;

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
                FarmLog.SystemError($"{Owner.name} - SelectFarmingGoalMultiAgent: Missing GoapActionProvider or NPCStats!");
                return TaskStatus.Failure;
            }

            // Guard: Don't request goals if AgentType hasn't been assigned yet
            if (actionProvider.AgentType == null)
            {
                return TaskStatus.Failure;
            }

            if (CropManager.Instance == null)
            {
                FarmLog.SystemError("CropManager not found! Add CropManager to scene.");
                return TaskStatus.Failure;
            }

            // Get all crops and calculate utilities
            var allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            if (allCrops.Length == 0)
            {
                if (enableDebugLog.Value)
                    FarmLog.GoalWarn(Owner.name, "No crops found in scene");
                return TaskStatus.Failure;
            }
            
            // DIAGNOSTIC: Log how many crops found
            if (enableDebugLog.Value && Time.frameCount % 600 == 0) // Every 10 seconds
            {
                FarmLog.System($"Scene has {allCrops.Length} crops active");
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
                        FarmLog.Goal(Owner.name, $"CONTINUE reserved {currentReservedCrop.name} | {goalForReserved} | U={utilityForReserved:F3} | Stage={stage}");
                        lastSelectedGoal = goalForReserved;
                        lastTargetCrop = currentReservedCrop;
                    }

                    return TaskStatus.Success;
                }
                else
                {
                    // Crop no longer needs work, release it
                    if (enableDebugLog.Value)
                        FarmLog.Goal(Owner.name, $"Reserved {currentReservedCrop.name} no longer needs work (Stage={stage}), releasing");
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

            // Build utility evaluation table for logging
            StringBuilder utilityTable = enableDebugLog.Value ? new StringBuilder() : null;

            foreach (var crop in allCrops)
            {
                // Skip if reserved by another agent
                bool available = CropManager.Instance.IsCropAvailable(crop, Owner.gameObject);
                if (!available)
                {
                    if (utilityTable != null)
                    {
                        var owner = CropManager.Instance.GetReservedAgent(crop);
                        utilityTable.AppendLine($"    {crop.name} (Stage={crop.GrowthStage}): RESERVED by {(owner != null ? owner.name : "?")}");
                    }
                    continue;
                }

                int stage = crop.GrowthStage;

                // Calculate base utilities with agent-specific weights
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
                float maxDistance = 50f;
                float normalizedDist = Mathf.Clamp01(distance / maxDistance);
                float distanceBonus = 0.15f * (1f - normalizedDist);

                // Apply distance bonus to valid utilities
                if (plantU > -999f) plantU += distanceBonus;
                if (waterU > -999f) waterU += distanceBonus;
                if (harvestU > -999f) harvestU += distanceBonus;

                // Log per-crop utility breakdown
                if (utilityTable != null)
                {
                    utilityTable.AppendLine($"    {crop.name} (Stage={stage}, Dist={distance:F1}): Plant={FarmLog.U(plantU)} | Water={FarmLog.U(waterU)} | Harvest={FarmLog.U(harvestU)}");
                }

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
                // No valid goals - log diagnostic
                if (enableDebugLog.Value && Time.frameCount % 120 == 0)
                {
                    int totalCrops = allCrops.Length;
                    int reservedByOthers = 0;
                    int noValidStage = 0;
                    foreach (var crop in allCrops)
                    {
                        if (!CropManager.Instance.IsCropAvailable(crop, Owner.gameObject))
                            reservedByOthers++;
                        else
                            noValidStage++;
                    }
                    FarmLog.GoalWarn(Owner.name, $"NO VALID GOAL | Total={totalCrops}, ReservedByOthers={reservedByOthers}, NoMatchingStage={noValidStage}");
                }
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

            // Submit bid if crop is NOT already reserved by this agent
            // After each goal completes, crops are released - agent must re-bid to reclaim
            // Auction only happens when multiple agents bid on the same FREE crop simultaneously
            // Existing reservations are NEVER overridden (first-come-first-served)
            bool alreadyMine = CropManager.Instance.IsReservedBy(targetCrop, Owner.gameObject);
            
            if (!alreadyMine)
            {
                CropManager.Instance.SubmitBid(targetCrop, Owner.gameObject, maxUtility, selectedGoal);
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

            // Log only when goal OR target crop changes (with full utility table)
            if ((lastSelectedGoal != selectedGoal || lastTargetCrop != targetCrop) && enableDebugLog.Value)
            {
                // Print the full utility evaluation table
                if (utilityTable != null && utilityTable.Length > 0)
                {
                    FarmLog.Utility(Owner.name, $"Evaluation (E={stats.Energy:F0}, H={stats.Hunger:F0}):\n{utilityTable}    >> BEST: {selectedGoal} -> {targetCrop.name} (U={maxUtility:F3})");
                }

                FarmLog.Goal(Owner.name, $"SELECT {selectedGoal} -> {targetCrop.name} | U={maxUtility:F3} | Stage={targetCrop.GrowthStage}");
                lastSelectedGoal = selectedGoal;
                lastTargetCrop = targetCrop;
            }

            return TaskStatus.Success;
        }
    }
}
