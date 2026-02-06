using UnityEngine;
using UnityEditor;
using BehaviorDesigner.Runtime;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using System.IO;

#if UNITY_EDITOR
/// <summary>
/// Editor script untuk create unique Behavior Tree assets untuk multi-agent
/// Usage: Tools → Multi-Agent → Create Unique BT Assets
/// </summary>
public class CreateUniqueBehaviorTrees : EditorWindow
{
    [MenuItem("Tools/Multi-Agent/Create Unique BT Assets")]
    public static void CreateUniqueBTAssets()
    {
        // Find all agents with BehaviorTree component
        var agents = Object.FindObjectsByType<BehaviorTree>(FindObjectsSortMode.None);
        
        if (agents.Length == 0)
        {
            EditorUtility.DisplayDialog("No Agents Found", 
                "No GameObjects with BehaviorTree component found in scene.", "OK");
            return;
        }
        
        Debug.Log($"[CreateUniqueBT] Found {agents.Length} agents with BehaviorTree");
        
        int duplicatedCount = 0;
        
        foreach (var agentBT in agents)
        {
            // Get external asset
            var externalBehavior = agentBT.ExternalBehavior;
            if (externalBehavior == null)
            {
                Debug.LogWarning($"[CreateUniqueBT] {agentBT.gameObject.name} has no External Behavior assigned! Skipping.");
                continue;
            }
            
            // Get asset path
            string originalPath = AssetDatabase.GetAssetPath(externalBehavior);
            if (string.IsNullOrEmpty(originalPath))
            {
                Debug.LogWarning($"[CreateUniqueBT] Cannot find asset path for {agentBT.gameObject.name}");
                continue;
            }
            
            // Create unique asset name
            string directory = Path.GetDirectoryName(originalPath);
            string extension = Path.GetExtension(originalPath);
            string agentName = agentBT.gameObject.name.Replace(" ", "_");
            string newFileName = $"{agentName}_BehaviorTree{extension}";
            string newPath = Path.Combine(directory, newFileName);
            
            // Check if already exists
            if (File.Exists(newPath))
            {
                Debug.Log($"[CreateUniqueBT] {agentBT.gameObject.name} already has unique asset: {newFileName}");
                continue;
            }
            
            // Duplicate asset
            bool success = AssetDatabase.CopyAsset(originalPath, newPath);
            if (success)
            {
                AssetDatabase.Refresh();
                
                // Load new asset
                var newBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(newPath);
                if (newBehavior != null)
                {
                    // Assign to agent
                    agentBT.ExternalBehavior = newBehavior;
                    EditorUtility.SetDirty(agentBT);
                    
                    Debug.Log($"[CreateUniqueBT] ✅ Created {newFileName} for {agentBT.gameObject.name}");
                    duplicatedCount++;
                }
                else
                {
                    Debug.LogError($"[CreateUniqueBT] Failed to load new asset: {newPath}");
                }
            }
            else
            {
                Debug.LogError($"[CreateUniqueBT] Failed to copy asset: {originalPath} → {newPath}");
            }
        }
        
        if (duplicatedCount > 0)
        {
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success!", 
                $"Created {duplicatedCount} unique Behavior Tree assets.\n\nAll agents now have independent BT state!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Already Done", 
                "All agents already have unique BT assets or no External Behaviors found.", "OK");
        }
    }
    
    [MenuItem("Tools/Multi-Agent/Verify Agent Instances")]
    public static void VerifyAgentInstances()
    {
        var agents = Object.FindObjectsByType<GoapActionProvider>(FindObjectsSortMode.None);
        
        if (agents.Length == 0)
        {
            EditorUtility.DisplayDialog("No Agents", "No agents with GoapActionProvider found.", "OK");
            return;
        }
        
        Debug.Log("=== AGENT INSTANCE VERIFICATION ===");
        
        System.Collections.Generic.Dictionary<int, int> btAssetCounts = new System.Collections.Generic.Dictionary<int, int>();
        
        foreach (var agent in agents)
        {
            var bt = agent.GetComponent<BehaviorTree>();
            var stats = agent.GetComponent<NPCStats>();
            
            int btAssetID = bt != null && bt.ExternalBehavior != null ? bt.ExternalBehavior.GetInstanceID() : 0;
            
            Debug.Log($"[{agent.gameObject.name}] " +
                      $"GOAP={agent.GetInstanceID()}, " +
                      $"Stats={(stats != null ? stats.GetInstanceID() : 0)}, " +
                      $"BT Component={(bt != null ? bt.GetInstanceID() : 0)}, " +
                      $"BT Asset={btAssetID}");
            
            if (btAssetID != 0)
            {
                if (!btAssetCounts.ContainsKey(btAssetID))
                    btAssetCounts[btAssetID] = 0;
                btAssetCounts[btAssetID]++;
            }
        }
        
        Debug.Log("=== BT ASSET SHARING CHECK ===");
        foreach (var kvp in btAssetCounts)
        {
            if (kvp.Value > 1)
            {
                Debug.LogError($"⚠️ BT Asset ID {kvp.Key} is SHARED by {kvp.Value} agents! This will cause conflicts!");
            }
            else
            {
                Debug.Log($"✅ BT Asset ID {kvp.Key} is unique (1 agent)");
            }
        }
        
        if (btAssetCounts.Count == agents.Length)
        {
            EditorUtility.DisplayDialog("Verification Passed!", 
                "All agents have unique BT assets. System should work correctly!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Shared Assets Detected!", 
                $"Some agents share BT assets! This will cause conflicts.\n\nRun: Tools → Multi-Agent → Create Unique BT Assets", "OK");
        }
    }
}
#endif
