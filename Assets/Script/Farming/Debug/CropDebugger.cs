using UnityEngine;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;

namespace FarmingGoap.Debug
{
    /// <summary>
    /// Debug script untuk verify semua crops terdeteksi dan tracked dengan benar
    /// Attach ke GameObject di scene (bisa di CropManager atau object lain)
    /// </summary>
    public class CropDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableContinuousDebug = true;
        [SerializeField] private float debugInterval = 2f; // Log every 2 seconds
        
        private float debugTimer = 0f;
        
        private void Start()
        {
            // Log semua crops saat start
            LogAllCrops();
        }
        
        private void Update()
        {
            if (!enableContinuousDebug) return;
            
            debugTimer += Time.deltaTime;
            if (debugTimer >= debugInterval)
            {
                debugTimer = 0f;
                LogAllCrops();
            }
        }
        
        [ContextMenu("Log All Crops")]
        public void LogAllCrops()
        {
            var allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            UnityEngine.Debug.Log($"========== CROP DEBUGGER ==========");
            UnityEngine.Debug.Log($"Total Crops Found: {allCrops.Length}");
            
            if (allCrops.Length == 0)
            {
                UnityEngine.Debug.LogError("❌ NO CROPS FOUND IN SCENE!");
                return;
            }
            
            for (int i = 0; i < allCrops.Length; i++)
            {
                var crop = allCrops[i];
                var cropName = crop.gameObject.name;
                var instanceID = crop.GetInstanceID();
                var position = crop.transform.position;
                var stage = crop.GrowthStage;
                
                // Check reservation
                GameObject reservedAgent = null;
                string reservedBy = "NONE";
                if (CropManager.Instance != null)
                {
                    reservedAgent = CropManager.Instance.GetReservedAgent(crop);
                    reservedBy = reservedAgent != null ? reservedAgent.name : "NONE";
                }
                
                UnityEngine.Debug.Log(
                    $"Crop #{i+1}: " +
                    $"Name='{cropName}', " +
                    $"InstanceID={instanceID}, " +
                    $"Pos={position}, " +
                    $"Stage={stage}, " +
                    $"ReservedBy={reservedBy}"
                );
            }
            
            UnityEngine.Debug.Log($"===================================");
        }
        
        [ContextMenu("Verify Crop Instances")]
        public void VerifyCropInstances()
        {
            var allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            UnityEngine.Debug.Log($"========== INSTANCE VERIFICATION ==========");
            
            // Check if all crops have unique instance IDs
            var instanceIDs = new System.Collections.Generic.HashSet<int>();
            bool allUnique = true;
            
            foreach (var crop in allCrops)
            {
                int id = crop.GetInstanceID();
                if (!instanceIDs.Add(id))
                {
                    UnityEngine.Debug.LogError($"❌ DUPLICATE INSTANCE ID FOUND: {id} for {crop.gameObject.name}");
                    allUnique = false;
                }
            }
            
            if (allUnique)
            {
                UnityEngine.Debug.Log($"✅ All {allCrops.Length} crops have UNIQUE instance IDs (working correctly)");
            }
            else
            {
                UnityEngine.Debug.LogError($"❌ Some crops share instance IDs (BUG!)");
            }
            
            UnityEngine.Debug.Log($"===========================================");
        }
        
        [ContextMenu("Test Crop Reservation")]
        public void TestCropReservation()
        {
            if (CropManager.Instance == null)
            {
                UnityEngine.Debug.LogError("❌ CropManager not found!");
                return;
            }
            
            var allCrops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            UnityEngine.Debug.Log($"========== RESERVATION TEST ==========");
            
            foreach (var crop in allCrops)
            {
                var reserved = CropManager.Instance.GetReservedAgent(crop);
                bool isAvailableForTest = CropManager.Instance.IsCropAvailable(crop, gameObject);
                
                UnityEngine.Debug.Log(
                    $"{crop.gameObject.name}: " +
                    $"Reserved={reserved != null}, " +
                    $"ReservedBy={reserved?.name ?? "NONE"}, " +
                    $"AvailableForDebugger={isAvailableForTest}"
                );
            }
            
            UnityEngine.Debug.Log($"======================================");
        }
    }
}
