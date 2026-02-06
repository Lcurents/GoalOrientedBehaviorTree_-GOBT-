using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    /// <summary>
    /// Plant seed WITHOUT shovel - SLOW (5 seconds)
    /// Requires: Only HasSeed (fallback option)
    /// </summary>
    [GoapId("Farming-PlantSeedSlowAction")]
    public class PlantSeedSlowAction : GoapActionBase<PlantSeedSlowAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 5f; // Slow without shovel
            data.Agent = agent.gameObject; // Store agent reference

            // Find crop at target position
            var targetPos = data.Target.Position;
            var nearbyColliders = Physics2D.OverlapCircleAll(targetPos, 1f);
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    // CRITICAL: Verify this crop is reserved by THIS agent
                    var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[PlantSlow] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[PlantSlow] {agent.gameObject.name} found {crop.name} but it's reserved by {reservedAgent?.name ?? "NONE"}! Ignoring.");
                    }
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogError($"[PlantSlow] {agent.gameObject.name} couldn't find their reserved crop at target position!");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Plant seed (slow)
                data.Crop.Plant();
                
                // Consume seed only
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    UnityEngine.Debug.Log($"[PlantSeedSlow] Tanam tanpa sekop (5s)! Seed={stats.HasSeed}");
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Keep reservation for next goal on same crop
            if (data.Crop != null && data.Agent != null)
            {
                UnityEngine.Debug.Log($"[PlantSlow] {data.Agent.name} finished planting {data.Crop.name} (keeping reservation)");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[PlantSlow] {data.Agent.name} INTERRUPTED, released {data.Crop.name}");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
            public GameObject Agent; // For crop release
            public bool ActionCompleted; // Track if action finished successfully
        }
    }
}
