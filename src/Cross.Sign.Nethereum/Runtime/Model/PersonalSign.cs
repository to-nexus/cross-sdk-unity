using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Newtonsoft.Json;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("personal_sign")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99998)]
    [JsonConverter(typeof(PersonalSignConverter))] // ✅ 여기에 붙임
    public class PersonalSign
    {
        public string Message { get; set; }

        public PersonalSign(string message)
        {
            Message = message;
        }

        [Preserve]
        public PersonalSign()
        {
        }
    }
}
