using UnityEngine;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using FarmingGoap.Behaviours;
using BehaviorDesigner.Runtime;

/// <summary>
/// Debug script untuk verify setiap agent punya instance terpisah
/// Attach ke SETIAP agent GameObject
/// </summary>
public class DebugAgentStatus : MonoBehaviour
{
    [Header("References (Auto-filled)")]
    [SerializeField] private GoapActionProvider goapProvider;
    [SerializeField] private AgentBehaviour agentBehaviour;
    [SerializeField] private NPCStats stats;
    [SerializeField] private BehaviorTree behaviorTree;
    
    [Header("Debug Info")]
    [SerializeField] private string currentGoal = "None";
    [SerializeField] private string currentAction = "None";
    [SerializeField] private int instanceID_GOAP;
    [SerializeField] private int instanceID_Agent;
    [SerializeField] private int instanceID_Stats;
    [SerializeField] private int instanceID_BT;
    [SerializeField] private string runnerName = "None";
    
    private void Awake()
    {
        goapProvider = GetComponent<GoapActionProvider>();
        agentBehaviour = GetComponent<AgentBehaviour>();
        stats = GetComponent<NPCStats>();
        behaviorTree = GetComponent<BehaviorTree>();
        
        if (goapProvider != null)
        {
            instanceID_GOAP = goapProvider.GetInstanceID();
            
            // Check if Runner is properly assigned
            var runner = goapProvider.GetComponent<AgentBehaviour>();
            if (runner != null)
            {
                runnerName = runner.gameObject.name;
            }
            else
            {
                runnerName = "NO RUNNER!";
                Debug.LogError($"[{gameObject.name}] GoapActionProvider has NO AgentBehaviour runner!");
            }
        }
        
        if (agentBehaviour != null)
            instanceID_Agent = agentBehaviour.GetInstanceID();
        if (stats != null)
            instanceID_Stats = stats.GetInstanceID();
        if (behaviorTree != null)
            instanceID_BT = behaviorTree.GetInstanceID();
        
        Debug.Log($"[{gameObject.name}] GOAP={instanceID_GOAP}, Agent={instanceID_Agent}, Stats={instanceID_Stats}, BT={instanceID_BT}, Runner={runnerName}");
    }
    
    private void Update()
    {
        if (goapProvider != null && goapProvider.CurrentPlan != null)
        {
            currentGoal = goapProvider.CurrentPlan.Goal?.GetType().Name ?? "None";
            currentAction = goapProvider.CurrentPlan.Action?.GetType().Name ?? "None";
        }
        else
        {
            currentGoal = "No Plan";
            currentAction = "N/A";
        }
    }
}
