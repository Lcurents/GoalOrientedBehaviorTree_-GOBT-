using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-GetShovelAction")]
    public class GetShovelAction : GoapActionBase<GetShovelAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 1f; // 1 detik untuk ambil sekop
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // SUCCESS: Tambah sekop ke inventory
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    // Limit to 2 shovels max (prevent excessive accumulation)
                    if (stats.HasShovel < 2)
                    {
                        stats.HasShovel++;
                        UnityEngine.Debug.Log($"[GetShovelAction] Ambil sekop! HasShovel: {stats.HasShovel}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[GetShovelAction] Sudah punya cukup sekop, skip. HasShovel: {stats.HasShovel}");
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
