using Cross.Core.Interfaces;
using Cross.Sign.Models.Engine.Methods;

namespace Cross.Sign.Models
{
    public struct PendingRequestStruct : IKeyHolder<long>
    {
        public long Id;

        public string Topic;

        public long Key
        {
            get => Id;
        }

        // Specify object here, so we can store any type
        // We don't care about type-safety for these pending
        // requests
        public SessionRequest<object> Parameters;
    }
}