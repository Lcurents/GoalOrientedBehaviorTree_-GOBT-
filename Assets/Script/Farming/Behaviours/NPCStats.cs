using UnityEngine;

namespace FarmingGoap.Behaviours
{
    public class NPCStats : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float hunger = 0f; // 0-100
        [SerializeField] private float energy = 100f; // 0-100
        
        [Header("Shared Resources")]
        [SerializeField] private int sharedFoodDisplay = 0; // Inspector display only (read from static)
        private static int sharedFoodCount = 0; // Shared across ALL agents

        [Header("Inventory - REDESIGNED")]
        [SerializeField] private int hasSeed = 0; // Jumlah bibit
        [SerializeField] private int hasWateringCan = 0; // Jumlah ember
        [SerializeField] private int hasShovel = 0; // Jumlah sekop

        [Header("Passive Rates")]
        [SerializeField] private float hungerIncreaseRate = 5f; // per 10 detik
        [SerializeField] private float energyDecreaseRate = 3f; // per 10 detik saat bekerja

        [Header("Utility Weights - Agent Personality")]
        [UnityEngine.Tooltip("Weight untuk Energy cost (contoh: 0.2)")]
        [SerializeField] private float weightEnergy = 0.2f;
        
        [UnityEngine.Tooltip("Weight untuk Hunger cost (contoh: 0.1)")]
        [SerializeField] private float weightHunger = 0.1f;
        
        [UnityEngine.Tooltip("Benefit untuk PlantingGoal (contoh: 0.4)")]
        [SerializeField] private float goalBenefitPlanting = 0.4f;
        
        [UnityEngine.Tooltip("Benefit untuk WateringGoal (contoh: 0.3)")]
        [SerializeField] private float goalBenefitWatering = 0.3f;
        
        [UnityEngine.Tooltip("Benefit untuk HarvestingGoal (contoh: 0.8)")]
        [SerializeField] private float goalBenefitHarvesting = 0.8f;

        private float passiveTimer = 0f;

        public float Hunger => hunger;
        public float Energy => energy;
        public int FoodCount => sharedFoodCount;
        
        // Inventory properties
        public int HasSeed { get => hasSeed; set => hasSeed = value; }
        public int HasWateringCan { get => hasWateringCan; set => hasWateringCan = value; }
        public int HasShovel { get => hasShovel; set => hasShovel = value; }
        
        // Utility weight properties (agent personality)
        public float WeightEnergy => weightEnergy;
        public float WeightHunger => weightHunger;
        public float GoalBenefitPlanting => goalBenefitPlanting;
        public float GoalBenefitWatering => goalBenefitWatering;
        public float GoalBenefitHarvesting => goalBenefitHarvesting;

        private void Update()
        {
            // Passive increase/decrease setiap 10 detik
            passiveTimer += Time.deltaTime;
            
            if (passiveTimer >= 10f)
            {
                passiveTimer = 0f;
                
                // Hunger naik
                hunger = Mathf.Min(100f, hunger + hungerIncreaseRate);
                
                // Energy turun (jika sedang bekerja/aktif)
                energy = Mathf.Max(0f, energy - energyDecreaseRate);
            }
        }

        public void IncreaseHunger(float amount)
        {
            hunger = Mathf.Min(100f, hunger + amount);
        }

        public void DecreaseHunger(float amount)
        {
            hunger = Mathf.Max(0f, hunger - amount);
        }

        public void IncreaseEnergy(float amount)
        {
            energy = Mathf.Min(100f, energy + amount);
        }

        public void DecreaseEnergy(float amount)
        {
            energy = Mathf.Max(0f, energy - amount);
        }

        public void AddFood(int amount)
        {
            sharedFoodCount += amount;
            UnityEngine.Debug.Log($"[{gameObject.name}] Added {amount} food → shared total: {sharedFoodCount}");
        }

        public void RemoveFood(int amount)
        {
            sharedFoodCount = Mathf.Max(0, sharedFoodCount - amount);
        }
        
        /// <summary>
        /// Reset shared food saat scene reload (karena static tidak reset otomatis)
        /// </summary>
        private void OnDestroy()
        {
            // Only reset when last NPCStats is destroyed
            if (FindObjectsByType<NPCStats>(FindObjectsSortMode.None).Length <= 1)
                sharedFoodCount = 0;
        }
        
        private void LateUpdate()
        {
            // Sync Inspector display
            sharedFoodDisplay = sharedFoodCount;
        }
    }
}
