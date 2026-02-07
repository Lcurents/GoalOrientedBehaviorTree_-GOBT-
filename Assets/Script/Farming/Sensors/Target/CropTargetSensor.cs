using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Sensors.Target
{
    [GoapId("Farming-CropTargetSensor")]
    public class CropTargetSensor : LocalTargetSensorBase
    {
        private CropBehaviour[] crops;
        [SerializeField] private bool enableDebugLog = true; // Enable by default for multi-agent debugging
        
        // Track which agents already got the "no reserved crop" warning to avoid spam
        private System.Collections.Generic.HashSet<GameObject> nullWarned = new System.Collections.Generic.HashSet<GameObject>();

        public override void Created()
        {
            // Find all crops in scene (bukan dari agent!)
            crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
        }

        public override void Update()
        {
            // Refresh crop list periodically
            if (Time.frameCount % 120 == 0) // Every 2 seconds
            {
                crops = Object.FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            }
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (crops == null || crops.Length == 0)
            {
                FarmLog.SystemWarn("CropTargetSensor: No crops found in scene!");
                return null;
            }

            GameObject agentObject = agent.Transform.gameObject;
            CropBehaviour currentlyOwnedCrop = null;

            // CRITICAL FIX: ONLY return crop that is reserved for this agent
            // Don't search for "nearest available" - that ignores auction results!
            if (CropManager.Instance != null)
            {
                foreach (var crop in crops)
                {
                    var reservedAgent = CropManager.Instance.GetReservedAgent(crop);
                    if (reservedAgent == agentObject)
                    {
                        currentlyOwnedCrop = crop;
                        if (enableDebugLog)
                            FarmLog.Sensor(agentObject.name, $"Target={crop.name} (reserved)");
                        break;
                    }
                }
            }

            // Return reserved crop (or null if no reservation)
            if (currentlyOwnedCrop != null)
            {
                nullWarned.Remove(agentObject); // Reset so we log again if they lose the crop
                return new TransformTarget(currentlyOwnedCrop.transform);
            }

            // No reserved crop = agent can't act yet (needs to bid in auction first)
            // Only log ONCE per agent to avoid spam - resets when they get a crop
            if (enableDebugLog && !nullWarned.Contains(agentObject))
            {
                FarmLog.Sensor(agentObject.name, "Target=NULL (no reserved crop, waiting for auction)");
                nullWarned.Add(agentObject);
            }
            return null;
        }
    }
}
