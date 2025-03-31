using Cross.Core.Interfaces;
using Cross.Core.Models.Pairing;

namespace Cross.Core.Controllers
{
    /// <summary>
    ///     A <see cref="Store{TKey,TValue}" /> module for storing
    ///     <see cref="PairingStruct" /> data. This will be used
    ///     for storing pairing data
    /// </summary>
    public class PairingStore : Store<string, PairingStruct>, IPairingStore
    {
        /// <summary>
        ///     Create a new instance of this module
        /// </summary>
        /// <param name="coreClient">The <see cref="ICoreClient" /> instance that will be used for <see cref="ICoreClient.Storage" /></param>
        public PairingStore(ICoreClient coreClient) : base(coreClient, "pairing", Cross.Core.CoreClient.StoragePrefix)
        {
        }
    }
}