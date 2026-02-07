using CrashKonijn.Goap.Runtime;
using FarmingGoap.AgentTypes;
using UnityEngine;

namespace FarmingGoap
{
    /// <summary>
    /// Ensures FarmerAgentTypeFactory is registered in GoapBehaviour's factory list
    /// BEFORE GoapBehaviour.Awake() initializes the GOAP system.
    /// Must be on the same GameObject as GoapBehaviour.
    /// </summary>
    [DefaultExecutionOrder(-101)] // Run BEFORE GoapBehaviour (-100)
    public class GoapSetupHelper : MonoBehaviour
    {
        private void Awake()
        {
            var goap = GetComponent<GoapBehaviour>();
            if (goap == null)
            {
                UnityEngine.Debug.LogError("[GoapSetupHelper] This script must be on the same GameObject as GoapBehaviour!");
                return;
            }

            // Ensure FarmerAgentTypeFactory component exists
            var farmerFactory = GetComponent<FarmerAgentTypeFactory>();
            if (farmerFactory == null)
            {
                farmerFactory = gameObject.AddComponent<FarmerAgentTypeFactory>();
                UnityEngine.Debug.Log("[GoapSetupHelper] Added FarmerAgentTypeFactory component");
            }

            // CRITICAL: Add factory to GoapBehaviour's serialized list if not already there
            // GoapBehaviour only processes factories in this list during Initialize()
            if (!goap.agentTypeConfigFactories.Contains(farmerFactory))
            {
                goap.agentTypeConfigFactories.Add(farmerFactory);
                UnityEngine.Debug.Log("[GoapSetupHelper] ✓ Registered FarmerAgentTypeFactory in GOAP factory list");
            }
            else
            {
                UnityEngine.Debug.Log("[GoapSetupHelper] ✓ FarmerAgentTypeFactory already in GOAP factory list");
            }
        }
    }
}
