using UnityEngine;

namespace FarmingGoap.Behaviours
{
    public class NPCStats : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float hunger = 0f; // 0-100
        [SerializeField] private float energy = 100f; // 0-100
        [SerializeField] private int foodCount = 0;

        [Header("Inventory - REDESIGNED")]
        [SerializeField] private int hasSeed = 0; // Jumlah bibit
        [SerializeField] private int hasWateringCan = 0; // Jumlah ember
        [SerializeField] private int hasShovel = 0; // Jumlah sekop

        [Header("Passive Rates")]
        [SerializeField] private float hungerIncreaseRate = 5f; // per 10 detik
        [SerializeField] private float energyDecreaseRate = 3f; // per 10 detik saat bekerja

        private float passiveTimer = 0f;

        public float Hunger => hunger;
        public float Energy => energy;
        public int FoodCount => foodCount;
        
        // Inventory properties
        public int HasSeed { get => hasSeed; set => hasSeed = value; }
        public int HasWateringCan { get => hasWateringCan; set => hasWateringCan = value; }
        public int HasShovel { get => hasShovel; set => hasShovel = value; }

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
            foodCount += amount;
        }

        public void RemoveFood(int amount)
        {
            foodCount = Mathf.Max(0, foodCount - amount);
        }
    }
}
