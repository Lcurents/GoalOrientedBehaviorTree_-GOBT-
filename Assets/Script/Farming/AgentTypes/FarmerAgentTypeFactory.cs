using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Capabilities;

namespace FarmingGoap.AgentTypes
{
    public class FarmerAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("FarmerAgent");
            
            // Add farming capability
            builder.AddCapability<FarmingCapabilityFactory>();

            return builder.Build();
        }
    }
}
