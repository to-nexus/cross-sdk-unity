using System.Threading.Tasks;
using Newtonsoft.Json;
using Cross.Sign.Utils;

namespace Cross.Sign.Models.Cacao
{
    /// <summary>
    ///     CAIP-74 Cacao object
    /// </summary>
    public class CacaoObject
    {
        [JsonProperty("h")]
        public readonly CacaoHeader Header;

        [JsonProperty("p")]
        public readonly CacaoPayload Payload;

        [JsonProperty("s")]
        public readonly CacaoSignature Signature;

        public CacaoObject(CacaoHeader header, CacaoPayload payload, CacaoSignature signature)
        {
            Header = header;
            Payload = payload;
            Signature = signature;
        }

        public async Task<bool> VerifySignature(string projectId, string rpcUrl = null)
        {
            UnityEngine.Debug.Log($"[CacaoObject] VerifySignature called");
            UnityEngine.Debug.Log($"[CacaoObject] Payload.Domain: {Payload.Domain}");
            UnityEngine.Debug.Log($"[CacaoObject] Payload.Aud: {Payload.Aud}");
            UnityEngine.Debug.Log($"[CacaoObject] Payload.Iss: {Payload.Iss}");
            
            var reconstructed = FormatMessage();
            UnityEngine.Debug.Log($"[CacaoObject] Reconstructed message:\n{reconstructed}");
            
            var walletAddress = CacaoUtils.ExtractDidAddress(Payload.Iss);
            var chainId = CacaoUtils.ExtractDidChainId(Payload.Iss);
            
            UnityEngine.Debug.Log($"[CacaoObject] Extracted wallet: {walletAddress}");
            UnityEngine.Debug.Log($"[CacaoObject] Extracted chainId: {chainId}");
            
            var result = await SignatureUtils.VerifySignature(walletAddress, reconstructed, Signature, chainId, projectId, rpcUrl);
            UnityEngine.Debug.Log($"[CacaoObject] Verification result: {result}");
            
            return result;
        }

        public string FormatMessage()
        {
            return Payload.FormatMessage();
        }
    }
}