using System;
using Cross.Sign.Models.Cacao;

namespace Cross.Sign.Models
{
    public class SessionAuthenticatedEventArgs : EventArgs
    {
        public CacaoObject[] Auths;
        public Session Session;
    }
}