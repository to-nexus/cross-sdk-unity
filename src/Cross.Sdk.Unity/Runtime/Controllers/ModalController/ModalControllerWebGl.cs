using System.Threading.Tasks;
using UnityEngine;
using Cross.Sdk.Unity.WebGl.Modal;
using NativeViewType = Cross.Sdk.Unity.ViewType;
using WebGlViewType = Cross.Sdk.Unity.WebGl.Modal.ViewType;

namespace Cross.Sdk.Unity.WebGl
{
    /// <summary>
    /// Modal Controller for the web implementation of the CrossSdk that uses Wagmi.
    /// </summary>
    public class ModalControllerWebGl : ModalController
    {
        protected override Task InitializeAsyncCore()
        {
            ModalInterop.StateChanged += StateChangedHandler;
            return Task.CompletedTask;
        }

        private void StateChangedHandler(ModalState modalState)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = !modalState.open;
#endif
            OnOpenStateChanged(new ModalOpenStateChangedEventArgs(modalState.open));
        }

        protected override void OpenCore(NativeViewType view)
        {
            var viewType = ConvertViewType(view);
            ModalInterop.Open(new OpenModalParameters(viewType));
        }

        protected override void CloseCore()
        {
            ModalInterop.Close();
        }

        private static WebGlViewType ConvertViewType(NativeViewType viewType)
        {
            return viewType switch
            {
                NativeViewType.Connect => WebGlViewType.Connect,
                NativeViewType.None => WebGlViewType.Connect,
                NativeViewType.Account => WebGlViewType.Account,
                NativeViewType.WalletSearch => WebGlViewType.AllWallets,
                NativeViewType.NetworkSearch => WebGlViewType.Networks,
                NativeViewType.QrCode => WebGlViewType.ConnectingWalletConnect,
                NativeViewType.Wallet => WebGlViewType.ConnectWallets,
                NativeViewType.NetworkLoading => WebGlViewType.Networks,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}