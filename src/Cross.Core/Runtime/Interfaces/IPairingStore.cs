using Cross.Core.Models.Pairing;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     A <see cref="IStore{TKey,TValue}" /> interface for a module
    ///     that stores <see cref="PairingStruct" /> data.
    /// </summary>
    public interface IPairingStore : IStore<string, PairingStruct>
    {
    }
}