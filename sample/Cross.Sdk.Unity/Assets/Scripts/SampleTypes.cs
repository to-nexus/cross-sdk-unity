using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Sample
{
    [Struct("Mail")]
    public class Mail
    {
        [Parameter("tuple", "from", 1, "Person")]
        public Person From { get; set; }

        [Parameter("tuple[]", "to", 2, "Person[]")]
        public List<Person> To { get; set; }

        [Parameter("string", "contents", 3)] public string Contents { get; set; }
    }

    [Struct("Person")]
    public class Person
    {
        [Parameter("string", "name", 1)] public string Name { get; set; }

        [Parameter("address[]", "wallets", 2)] public List<string> Wallets { get; set; }
    }

    [Struct("Group")]
    public class Group
    {
        [Parameter("string", "name", 1)] public string Name { get; set; }

        [Parameter("tuple[]", "members", 2, "Person[]")]
        public List<Person> Members { get; set; }
    }

    /// <summary>
    /// Simple EIP-712 message type (like cross-sdk-js Ping example)
    /// </summary>
    [Struct("Ping")]
    public class Ping
    {
        [Parameter("string", "contents", 1)]
        public string Contents { get; set; }
    }

    /// <summary>
    /// ERC20Mint EIP-712 message type (real-world use case for token minting with permit)
    /// </summary>
    [Struct("ERC20Mint")]
    public class ERC20Mint
    {
        [Parameter("address", "token", 1)]
        public string Token { get; set; }

        [Parameter("uint256", "amount", 2)]
        public string Amount { get; set; }

        [Parameter("address", "feeRecipient", 3)]
        public string FeeRecipient { get; set; }

        [Parameter("uint256", "feeBPS", 4)]
        public string FeeBPS { get; set; }

        [Parameter("uint256", "nonce", 5)]
        public string Nonce { get; set; }

        [Parameter("uint256", "deadline", 6)]
        public string Deadline { get; set; }
    }
}