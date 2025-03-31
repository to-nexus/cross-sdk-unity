using Cross.Core.Interfaces;
using Cross.Core.Models.Verify;

namespace Cross.Sign.Models
{
    public class AuthPendingRequest : IKeyHolder<long>
    {
        public long Id { get; set; }

        public string PairingTopic { get; set; }

        public Participant Requester { get; set; }

        public AuthPayloadParams PayloadParams { get; set; }

        public long? Expiry { get; set; }

        public VerifiedContext VerifyContext { get; set; }

        public long Key
        {
            get => Id;
        }
    }
}