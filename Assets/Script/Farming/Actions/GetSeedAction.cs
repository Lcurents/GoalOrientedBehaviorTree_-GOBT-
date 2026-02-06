using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-GetSeedAction")]
    public class GetSeedAction : GoapActionBase<GetSeedAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 1f; // 1 detik untuk ambil bibit
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // SUCCESS: Tambah bibit ke inventory
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    // Limit to 2 seeds max (prevent excessive accumulation)
                    if (stats.HasSeed < 2)
                    {
                        stats.HasSeed++;
                        UnityEngine.Debug.Log($"[GetSeedAction] Ambil bibit! HasSeed: {stats.HasSeed}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[GetSeedAction] Sudah punya cukup bibit, skip. HasSeed: {stats.HasSeed}");
                    }
                }
                
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
        }
    }
}
