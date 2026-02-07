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
            
            CropBehaviour bestFallback = null;
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        FarmLog.Action(agent.gameObject.name, $"PlantSlow START | Target={crop.name} (reserved, verified)");
                        break;
                    }
                    
                    // Track unreserved empty crop as fallback
                    if (reservedAgent == null && crop.GrowthStage == 0)
                    {
                        bestFallback = crop;
                    }
                }
            }
            
            // Fallback: claim free empty crop if reservation was lost
            if (data.Crop == null && bestFallback != null && CropManager.Instance != null)
            {
                CropManager.Instance.SubmitBid(bestFallback, agent.gameObject, 1f, "Planting");
                if (CropManager.Instance.IsReservedBy(bestFallback, agent.gameObject))
                {
                    data.Crop = bestFallback;
                    FarmLog.ActionWarn(agent.gameObject.name, $"PlantSlow FALLBACK | Claimed free crop {bestFallback.name}");
                }
            }
            
            if (data.Crop == null)
            {
                FarmLog.ActionWarn(agent.gameObject.name, "PlantSlow ABORTED | No plantable crop at target");
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
                    FarmLog.Resource(agent.gameObject.name, $"PlantSlow consumed | Seed={stats.HasSeed}");
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Only release on successful completion (ActionCompleted=true)
            // Interrupted case is handled by Stop()
            if (data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantSlow COMPLETE | {data.Crop.name} planted (5s) and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"PlantSlow INTERRUPTED | {data.Crop.name} released");
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
