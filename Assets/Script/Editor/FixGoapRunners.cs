using UnityEngine;
using UnityEditor;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;

#if UNITY_EDITOR
/// <summary>
/// Fix GOAP runner assignments for multi-agent setup
/// </summary>
public class FixGoapRunners : EditorWindow
{
    [MenuItem("Tools/Multi-Agent/Fix GOAP Runners")]
    public static void FixRunners()
    {
        var providers = Object.FindObjectsByType<GoapActionProvider>(FindObjectsSortMode.None);
        
        if (providers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Agents", "No GoapActionProvider found in scene.", "OK");
            return;
        }
        
        int fixedCount = 0;
        
        foreach (var provider in providers)
        {
            var agentBehaviour = provider.GetComponent<AgentBehaviour>();
            
            if (agentBehaviour == null)
            {
                Debug.LogError($"[FixGoapRunners] {provider.gameObject.name} has GoapActionProvider but NO AgentBehaviour!");
                continue;
            }
            
            // Check if AgentBehaviour is enabled
            if (!agentBehaviour.enabled)
            {
                agentBehaviour.enabled = true;
                Debug.LogWarning($"[FixGoapRunners] {provider.gameObject.name} AgentBehaviour was DISABLED, enabled it!");
                fixedCount++;
            }
            
            // Verify GoapBehaviour reference
            var goapBehaviour = Object.FindFirstObjectByType<GoapBehaviour>();
            if (goapBehaviour == null)
            {
                Debug.LogError($"[FixGoapRunners] No GoapBehaviour found in scene! You need a GoapBehaviour GameObject.");
                continue;
            }
            
            // AgentBehaviour should reference GoapBehaviour (this is usually auto-assigned)
            Debug.Log($"[FixGoapRunners] ✅ {provider.gameObject.name}: " +
                      $"GoapProvider (ID={provider.GetInstanceID()}), " +
                      $"AgentBehaviour (ID={agentBehaviour.GetInstanceID()})");
            
            EditorUtility.SetDirty(provider);
            EditorUtility.SetDirty(agentBehaviour);
        }
        
        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog("Fixed!", 
                $"Re-enabled {fixedCount} disabled AgentBehaviour components.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Check Complete", 
                "All agents have AgentBehaviour components. Check Console for details.", "OK");
        }
    }
    
    [MenuItem("Tools/Multi-Agent/Debug GOAP Setup")]
    public static void DebugGoapSetup()
    {
        var providers = Object.FindObjectsByType<GoapActionProvider>(FindObjectsSortMode.None);
        
        Debug.Log("=== GOAP SETUP DEBUG ===");
        
        foreach (var provider in providers)
        {
            var agent = provider.GetComponent<AgentBehaviour>();
            var receiver = provider.Receiver;
            
            Debug.Log($"[{provider.gameObject.name}]:");
            Debug.Log($"  - GoapActionProvider: {provider.GetInstanceID()}");
            Debug.Log($"  - AgentBehaviour: {(agent != null ? agent.GetInstanceID().ToString() : "NULL!")}");
            Debug.Log($"  - AgentBehaviour.enabled: {(agent != null ? agent.enabled : false)}");
            Debug.Log($"  - Receiver: {(receiver != null ? "Present" : "NULL")}");
            
            if (provider.CurrentPlan != null)
            {
                Debug.Log($"  - Current Goal: {provider.CurrentPlan.Goal?.GetType().Name ?? "None"}");
                Debug.Log($"  - Current Action: {provider.CurrentPlan.Action?.GetType().Name ?? "None"}");
            }
            else
            {
                Debug.Log($"  - Current Plan: NULL");
            }
        }
        
        Debug.Log("=== END DEBUG ===");
    }
}
#endif
