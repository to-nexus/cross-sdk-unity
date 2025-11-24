using System;

namespace Cross.Core.Models.Pairing
{
    public class PairingCreatedEventArgs : EventArgs
    {
        public PairingStruct Pairing;
    }
}