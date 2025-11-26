#nullable enable

using System;
using System.Linq;
using Newtonsoft.Json;
using Cross.Sign.Utils;

namespace Cross.Sign.Models.Cacao
{
    public class CacaoPayload
    {
        [JsonProperty("domain")]
        public string Domain { get; }

        [JsonProperty("iss")]
        public string Iss { get; } // did:pkh

        [JsonProperty("aud")]
        public string Aud { get; }

        [JsonProperty("version")]
        public string Version { get; }

        [JsonProperty("nonce")]
        public string Nonce { get; }

        [JsonProperty("iat")]
        public string IssuedAt { get; }

        [JsonProperty("nbf")]
        public string? NotBefore { get; }

        [JsonProperty("exp")]
        public string? Expiration { get; }

        [JsonProperty("statement", NullValueHandling = NullValueHandling.Ignore)]
        public string? Statement { get; }

        [JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string? RequestId { get; }

        [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Resources { get; }

        public CacaoPayload(
            string domain,
            string iss,
            string aud,
            string version,
            string nonce,
            string issuedAt,
            string? notBefore = null,
            string? expiration = null,
            string? statement = null,
            string? requestId = null,
            string[]? resources = null)
        {
            Domain = domain;
            Iss = iss;
            Aud = aud;
            Version = version;
            Nonce = nonce;
            IssuedAt = issuedAt;
            NotBefore = notBefore;
            Expiration = expiration;
            Statement = statement;
            RequestId = requestId;
            Resources = resources;
        }

        public static CacaoPayload FromAuthPayloadParams(AuthPayloadParams authPayloadParams, string iss)
        {
            return new CacaoPayload(
                authPayloadParams.Domain,
                iss,
                authPayloadParams.Aud,
                authPayloadParams.Version,
                authPayloadParams.Nonce,
                authPayloadParams.Iat,
                authPayloadParams.Nbf,
                authPayloadParams.Exp,
                authPayloadParams.Statement,
                authPayloadParams.RequestId?.ToString(),
                authPayloadParams.Resources?.ToArray()
            );
        }

        public string FormatMessage()
        {
            if (!Iss.StartsWith("did:pkh:"))
            {
                throw new InvalidOperationException($"Invalid issuer: {Iss}. Expected 'did:pkh:'.");
            }

            var header = $"{Domain} wants you to sign in with your Ethereum account:";
            var walletAddress = CacaoUtils.ExtractDidAddress(Iss);
            var uri = $"URI: {Aud}";
            var version = $"Version: {Version}";
            var chainId = $"Chain ID: {CacaoUtils.ExtractDidChainIdReference(Iss)}";
            var nonce = $"Nonce: {Nonce}";
            var issuedAt = $"Issued At: {IssuedAt}";
            var expirationTime = Expiration != null ? $"Expiration Time: {Expiration}" : null;
            var notBefore = NotBefore != null ? $"Not Before: {NotBefore}" : null;
            var resources = Resources is { Length: > 0 }
                ? $"Resources:\n{string.Join('\n', Resources.Select(resource => $"- {resource}"))}"
                : null;

            var statement = Statement;
            if (ReCap.TryGetRecapFromResources(Resources, out var recapStr))
            {
                var decoded = ReCap.Decode(recapStr);
                statement = decoded.FormatStatement(statement);
            }

            // Build message parts
            var messageParts = new System.Collections.Generic.List<string> { header, walletAddress };
            
            // Add blank line and optional statement per EIP-4361 standard
            messageParts.Add("");  // Blank line before statement/URI
            if (!string.IsNullOrWhiteSpace(statement))
            {
                messageParts.Add(statement);
                messageParts.Add("");  // Blank line after statement (only when statement exists)
            }
            
            // Add remaining fields
            messageParts.Add(uri);
            messageParts.Add(version);
            messageParts.Add(chainId);
            messageParts.Add(nonce);
            messageParts.Add(issuedAt);
            
            if (!string.IsNullOrWhiteSpace(expirationTime))
                messageParts.Add(expirationTime);
            if (!string.IsNullOrWhiteSpace(notBefore))
                messageParts.Add(notBefore);
            if (!string.IsNullOrWhiteSpace(resources))
                messageParts.Add(resources);

            var message = string.Join('\n', messageParts);

            return message;
        }
    }
}