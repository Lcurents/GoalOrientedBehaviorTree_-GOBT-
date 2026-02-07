using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace FarmingGoap.BehaviorTree
{
    /// <summary>
    /// Action: Fallback idle behavior - tidak request goal GOAP apapun
    /// Agent akan pure idle (GOAP tidak planning)
    /// </summary>
    [TaskCategory("GOAP/Idle")]
    [TaskDescription("Fallback idle - tidak ada goal yang dipilih, agent pure idle")]
    public class SelectIdleGoal : Action
    {
        [UnityEngine.Tooltip("Enable debug logging?")]
        public SharedBool enableDebugLog = false; // Default false untuk idle
        
        private string lastLog = "";
        
        public override TaskStatus OnUpdate()
        {
            // Tidak request goal apapun - let agent idle
            // GOAP planner tidak aktif, agent akan pure idle state
            
            if (enableDebugLog.Value && lastLog != "Idle")
            {
                FarmLog.Goal("Agent", "Idle (no goal selected)");
                lastLog = "Idle";
            }
            
            return TaskStatus.Success; // Always success karena ini fallback
        }
    }
}
