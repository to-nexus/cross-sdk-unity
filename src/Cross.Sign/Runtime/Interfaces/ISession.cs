using Cross.Core.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Interfaces
{
    /// <summary>
    ///     A <see cref="IStore{TKey,TValue}" /> interface for a module
    ///     that stores <see cref="Session" /> data.
    /// </summary>
    public interface ISession : IStore<string, Session>
    {
    }
}