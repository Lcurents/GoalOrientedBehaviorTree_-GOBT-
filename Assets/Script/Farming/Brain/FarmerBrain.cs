using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Goals;
using FarmingGoap.AgentTypes;
using UnityEngine;

namespace FarmingGoap.Brain
{
    /// <summary>
    /// FarmerBrain: Menangani setup dan inisialisasi awal agent untuk MAS Mode (Behavior Tree based).
    /// Sistem legacy "useCodeBasedSelection" sudah sepenuhnya dihapus untuk migrasi penuh ke Behavior Tree.
    /// Pengaturan Utility, Prioritas & Seleksi Goal 100% ditangani oleh Behavior Tree.
    /// </summary>
    public class FarmerBrain : MonoBehaviour
    {
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        private GoapBehaviour goap;

        private void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = GetComponent<AgentBehaviour>();
            this.provider = GetComponent<GoapActionProvider>();
            
            // Link AgentBehaviour dengan GoapActionProvider (WAJIB!)
            this.agent.ActionProvider = this.provider;

            // Pastikan agent punya BehaviorTree
            var bt = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            if (bt == null)
            {
                FarmLog.SystemError($"{gameObject.name}: Agent kehilangan BehaviorTree Component! MAS mode tidak akan berjalan lancar.");
            }

            // Assign AgentType in Awake - GoapBehaviour runs at -100 so it's already initialized
            // GoapSetupHelper runs at -101 ensuring factory is registered before GoapBehaviour
            if (this.provider.AgentTypeBehaviour == null && this.goap != null)
            {
                try
                {
                    this.provider.AgentType = this.goap.GetAgentType("FarmerAgent");
                    FarmLog.System($"{gameObject.name}: AgentType 'FarmerAgent' assigned successfully");
                }
                catch (System.Exception e)
                {
                    FarmLog.SystemError($"{gameObject.name}: Failed to assign AgentType - {e.Message}");
                }
            }
        }
    }
}
