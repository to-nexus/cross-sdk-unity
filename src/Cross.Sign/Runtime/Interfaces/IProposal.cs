using Cross.Core.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Interfaces
{
    /// <summary>
    ///     A <see cref="IStore{TKey,TValue}" /> interface for a module
    ///     that stores <see cref="ProposalStruct" /> data.
    /// </summary>
    public interface IProposal : IStore<long, ProposalStruct>
    {
    }
}