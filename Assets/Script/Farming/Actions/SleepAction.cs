using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-SleepAction")]
    public class SleepAction : GoapActionBase<SleepAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
            UnityEngine.Debug.Log("[SleepAction] NPC mulai tidur...");
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            // Restore energy over time (3 detik untuk full)
            var stats = data.Stats;
            if (stats != null)
            {
                float energyRestore = 30f * context.DeltaTime; // 30 per detik
                stats.IncreaseEnergy(energyRestore);
                
                // Selesai jika energy > 80
                if (stats.Energy >= 80f)
                {
                    UnityEngine.Debug.Log($"[SleepAction] Bangun! Energy: {stats.Energy}");
                    return ActionRunState.Completed;
                }
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            [GetComponent]
            public NPCStats Stats { get; set; }
            
            public float Timer { get; set; }
        }
    }
}
