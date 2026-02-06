using UnityEngine;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Testing
{
    /// <summary>
    /// Helper untuk testing dan debugging NPC behavior
    /// Attach ke Farmer GameObject untuk kontrol manual
    /// </summary>
    public class NPCTestController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCStats stats;
        [SerializeField] private CropBehaviour crop;

        [Header("Quick Test Buttons")]
        [SerializeField] private bool setHungry = false;
        [SerializeField] private bool setTired = false;
        [SerializeField] private bool setCropReady = false;
        [SerializeField] private bool resetStats = false;
        [SerializeField] private bool addFood = false;

        private void OnValidate()
        {
            if (stats == null) stats = GetComponent<NPCStats>();
            if (crop == null) crop = GetComponent<CropBehaviour>();
        }

        private void Update()
        {
            // Test buttons (toggle di Inspector saat Play)
            if (setHungry)
            {
                setHungry = false;
                TestSetHungry();
            }

            if (setTired)
            {
                setTired = false;
                TestSetTired();
            }

            if (setCropReady)
            {
                setCropReady = false;
                TestSetCropReady();
            }

            if (resetStats)
            {
                resetStats = false;
                TestResetStats();
            }

            if (addFood)
            {
                addFood = false;
                TestAddFood();
            }

            // Keyboard shortcuts (saat Play Mode)
            if (Input.GetKeyDown(KeyCode.H))
                TestSetHungry();

            if (Input.GetKeyDown(KeyCode.E))
                TestSetTired();

            if (Input.GetKeyDown(KeyCode.C))
                TestSetCropReady();

            if (Input.GetKeyDown(KeyCode.R))
                TestResetStats();

            if (Input.GetKeyDown(KeyCode.F))
                TestAddFood();
        }

        private void TestSetHungry()
        {
            if (stats != null)
            {
                stats.IncreaseHunger(100); // Set ke max hunger
                UnityEngine.Debug.Log("[TEST] Hunger set to 100 - NPC should try to eat!");
            }
        }

        private void TestSetTired()
        {
            if (stats != null)
            {
                stats.DecreaseEnergy(100); // Set energy ke 0
                UnityEngine.Debug.Log("[TEST] Energy set to 0 - NPC should sleep!");
            }
        }

        private void TestSetCropReady()
        {
            if (crop != null)
            {
                crop.SetGrowthStage(3); // Set crop matang
                UnityEngine.Debug.Log("[TEST] Crop set to stage 3 - NPC should harvest!");
            }
        }

        private void TestResetStats()
        {
            if (stats != null)
            {
                stats.DecreaseHunger(100); // Reset hunger to 0
                stats.IncreaseEnergy(100); // Reset energy to 100
                UnityEngine.Debug.Log("[TEST] Stats reset - Hunger=0, Energy=100");
            }

            if (crop != null)
            {
                crop.SetGrowthStage(0); // Reset crop
                UnityEngine.Debug.Log("[TEST] Crop reset to stage 0");
            }
        }

        private void TestAddFood()
        {
            if (stats != null)
            {
                stats.AddFood(5);
                UnityEngine.Debug.Log($"[TEST] Added 5 food - Total: {stats.FoodCount}");
            }
        }

        private void OnGUI()
        {
            // Display keyboard shortcuts
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 150));
            GUILayout.Label("<b>Test Shortcuts:</b>");
            GUILayout.Label("H - Set Hungry (100)");
            GUILayout.Label("E - Set Tired (Energy 0)");
            GUILayout.Label("C - Set Crop Ready (Stage 3)");
            GUILayout.Label("R - Reset All Stats");
            GUILayout.Label("F - Add 5 Food");
            GUILayout.EndArea();
        }
    }
}
