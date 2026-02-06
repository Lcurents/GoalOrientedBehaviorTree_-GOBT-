using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-PlantSeedAction")]
    public class PlantSeedAction : GoapActionBase<PlantSeedAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            var stats = agent.GetComponent<NPCStats>();
            
            // Cek apakah punya sekop atau tidak
            if (stats != null && stats.HasShovel > 0)
            {
                data.Timer = 2f; // Dengan sekop: cepat (2 detik)
            }
            else
            {
                data.Timer = 5f; // Tanpa sekop: lambat (5 detik)
            }

            // Find crop at target position
            var targetPos = data.Target.Position;
            var nearbyColliders = Physics2D.OverlapCircleAll(targetPos, 1f);
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    // CRITICAL: Verify this crop is reserved by THIS agent
                    var reservedAgent = FarmingGoap.Managers.CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[PlantSeed] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[PlantSeed] {agent.gameObject.name} found {crop.name} but it's reserved by {reservedAgent?.name ?? "NONE"}! Ignoring.");
                    }
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogError($"[PlantSeed] {agent.gameObject.name} couldn't find their reserved crop at target position!");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Tanam bibit
                data.Crop.Plant();
                
                // Consume bibit dan sekop
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    if (stats.HasShovel > 0)
                    {
                        stats.HasShovel--; // Sekop habis dipakai
                    }
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
            public bool ActionCompleted; // Track if action finished successfully
        }
    }
}
