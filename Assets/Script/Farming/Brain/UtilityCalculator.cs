using UnityEngine;

namespace FarmingGoap.Brain
{
    /// <summary>
    /// Kalkulator Utility untuk pemilihan goal farming
    /// Formula: U(goal) = (W_goal × GoalBenefit) - (W_E × E_cost/E_Max) - (W_H × H_cost/H_Max)
    /// </summary>
    public static class UtilityCalculator
    {
        // ========== CONSTANTS ==========
        private const float MAX_ENERGY = 100f;
        private const float MAX_HUNGER = 100f;
        
        // Weights
        private const float WEIGHT_GOAL = 1.0f;
        private const float WEIGHT_ENERGY = 0.2f;
        private const float WEIGHT_HUNGER = 0.1f;
        
        // Goal Benefits
        private const float BENEFIT_PLANTING = 0.4f;
        private const float BENEFIT_WATERING = 0.3f;
        private const float BENEFIT_HARVESTING = 0.8f;
        
        // Estimated costs per goal (energy & hunger yang akan habis)
        private const float PLANTING_ENERGY_COST = 8f;  // 8 energy untuk tanam
        private const float PLANTING_HUNGER_COST = 5f;  // 5 hunger untuk tanam
        
        private const float WATERING_ENERGY_COST = 6f;  // 6 energy untuk siram
        private const float WATERING_HUNGER_COST = 4f;  // 4 hunger untuk siram
        
        private const float HARVESTING_ENERGY_COST = 10f; // 10 energy untuk panen
        private const float HARVESTING_HUNGER_COST = 8f;  // 8 hunger untuk panen
        
        // ========== PUBLIC METHODS ==========
        
        /// <summary>
        /// Hitung utility untuk PlantingGoal (dengan agent-specific weights)
        /// </summary>
        public static float CalculatePlantingUtility(float currentEnergy, float currentHunger, int cropStage, 
            float weightEnergy, float weightHunger, float goalBenefit)
        {
            // Disable jika crop sudah ditanam
            if (cropStage > 0)
                return -999f;
            
            return CalculateUtility(
                goalBenefit,
                PLANTING_ENERGY_COST,
                PLANTING_HUNGER_COST,
                currentEnergy,
                currentHunger,
                weightEnergy,
                weightHunger
            );
        }
        
        /// <summary>
        /// Hitung utility untuk WateringGoal (dengan agent-specific weights)
        /// </summary>
        public static float CalculateWateringUtility(float currentEnergy, float currentHunger, int cropStage,
            float weightEnergy, float weightHunger, float goalBenefit)
        {
            // Disable jika crop belum ditanam atau sudah matang
            if (cropStage < 1 || cropStage >= 3)
                return -999f;
            
            return CalculateUtility(
                goalBenefit,
                WATERING_ENERGY_COST,
                WATERING_HUNGER_COST,
                currentEnergy,
                currentHunger,
                weightEnergy,
                weightHunger
            );
        }
        
        /// <summary>
        /// Hitung utility untuk HarvestingGoal (dengan agent-specific weights)
        /// </summary>
        public static float CalculateHarvestingUtility(float currentEnergy, float currentHunger, int cropStage,
            float weightEnergy, float weightHunger, float goalBenefit)
        {
            // Disable jika crop belum matang
            if (cropStage < 3)
                return -999f;
            
            return CalculateUtility(
                goalBenefit,
                HARVESTING_ENERGY_COST,
                HARVESTING_HUNGER_COST,
                currentEnergy,
                currentHunger,
                weightEnergy,
                weightHunger
            );
        }
        
        // ========== LEGACY METHODS (for backward compatibility) ==========
        
        /// <summary>
        /// Hitung utility untuk PlantingGoal (legacy - uses default weights)
        /// </summary>
        public static float CalculatePlantingUtility(float currentEnergy, float currentHunger, int cropStage)
        {
            return CalculatePlantingUtility(currentEnergy, currentHunger, cropStage, 
                WEIGHT_ENERGY, WEIGHT_HUNGER, BENEFIT_PLANTING);
        }
        
        /// <summary>
        /// Hitung utility untuk WateringGoal (legacy - uses default weights)
        /// </summary>
        public static float CalculateWateringUtility(float currentEnergy, float currentHunger, int cropStage)
        {
            return CalculateWateringUtility(currentEnergy, currentHunger, cropStage,
                WEIGHT_ENERGY, WEIGHT_HUNGER, BENEFIT_WATERING);
        }
        
        /// <summary>
        /// Hitung utility untuk HarvestingGoal (legacy - uses default weights)
        /// </summary>
        public static float CalculateHarvestingUtility(float currentEnergy, float currentHunger, int cropStage)
        {
            return CalculateHarvestingUtility(currentEnergy, currentHunger, cropStage,
                WEIGHT_ENERGY, WEIGHT_HUNGER, BENEFIT_HARVESTING);
        }
        
        // ========== PRIVATE HELPER ==========
        
        /// <summary>
        /// Formula inti: U(goal) = (W_goal × GoalBenefit) - (W_E × E_cost/E_Max) - (W_H × H_cost/H_Max)
        /// </summary>
        private static float CalculateUtility(
            float goalBenefit,
            float energyCost,
            float hungerCost,
            float currentEnergy,
            float currentHunger,
            float weightEnergy,
            float weightHunger)
        {
            float benefitTerm = WEIGHT_GOAL * goalBenefit;
            float energyTerm = weightEnergy * (energyCost / MAX_ENERGY);
            float hungerTerm = weightHunger * (hungerCost / MAX_HUNGER);
            
            float utility = benefitTerm - energyTerm - hungerTerm;
            
            // Safety: Jika energy/hunger tidak cukup untuk execute goal, kurangi utility drastis
            if (currentEnergy < energyCost * 1.5f || currentHunger > (MAX_HUNGER - hungerCost * 1.5f))
            {
                utility *= 0.5f; // Penalty 50% jika mendekati batas
            }
            
            return utility;
        }
        
        /// <summary>
        /// Legacy overload (for backward compatibility)
        /// </summary>
        private static float CalculateUtility(
            float goalBenefit,
            float energyCost,
            float hungerCost,
            float currentEnergy,
            float currentHunger)
        {
            return CalculateUtility(goalBenefit, energyCost, hungerCost, currentEnergy, currentHunger,
                WEIGHT_ENERGY, WEIGHT_HUNGER);
        }
        
        // ========== DEBUG HELPER ==========
        
        public static string GetUtilityDebugString(float plantingU, float wateringU, float harvestingU)
        {
            return $"Utilities: Planting={plantingU:F3}, Watering={wateringU:F3}, Harvesting={harvestingU:F3}";
        }
    }
}
