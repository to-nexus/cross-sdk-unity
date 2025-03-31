using Newtonsoft.Json;
using Cross.Core.Models.Verify;

namespace Cross.Sign.Models.Engine.Events
{
    public class SessionProposalEvent
    {
        [JsonProperty("id")]
        public long Id;

        [JsonProperty("params")]
        public ProposalStruct Proposal;

        [JsonProperty("verifyContext")]
        public VerifiedContext VerifiedContext;
    }
}