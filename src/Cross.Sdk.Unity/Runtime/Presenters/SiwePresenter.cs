using System;
using Cross.Sdk.Unity.Utils;
using Cross.Sdk.Unity.Views.SiweView;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity
{
    public class SiwePresenter : Presenter<SiweView>
    {
        public override string Title
        {
            get => "Sign In";
        }

        public override bool EnableCloseButton
        {
            get => false;
        }

        private SignatureRequest _lastSignatureRequest;
        private bool _success;
        private bool _disposed;
        private RemoteSprite<Image> _walletLogo;
        
        public SiwePresenter(RouterController router, VisualElement parent, bool hideView = true) : base(router, parent, hideView)
        {
            var appName = CrossSdk.Config.metadata.Name;
            if (!string.IsNullOrWhiteSpace(appName))
            {
                View.Title = $"{appName} wants to connect to your wallet";
            }

            // Load app logo on Presenter creation because it's static
            var appLogoUrl = CrossSdk.Config.metadata.IconUrl;
            if (!string.IsNullOrWhiteSpace(appLogoUrl))
            {
                RemoteSpriteFactory
                    .GetRemoteSprite<Image>(appLogoUrl)
                    .SubscribeImage(View.LogoAppImage);
            }

            View.CancelButtonClicked += RejectButtonClickedHandler;
            View.ApproveButtonClicked += ApproveButtonClickedHandler;
            
            CrossSdk.SiweController.Config.SignInSuccess += SignInSuccessHandler;
            CrossSdk.SiweController.Config.SignOutSuccess += SignOutSuccessHandler;

            CrossSdk.ConnectorController.SignatureRequested += SignatureRequestedHandler;
        }

        private void SignInSuccessHandler(SiweSession siweSession)
        {
            _success = true;
            CrossSdk.CloseModal();
        }

        private void SignOutSuccessHandler()
        {
            Router.GoBack();
        }

        private void SignatureRequestedHandler(object sender, SignatureRequest e)
        {
            _lastSignatureRequest = e;
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            _walletLogo?.UnsubscribeImage(View.LogoWalletImage);

            if (WalletUtils.TryGetRecentWallet(out var wallet))
            {
                _walletLogo = wallet.Image;
                _walletLogo.SubscribeImage(View.LogoWalletImage);
            }

            View.ButtonsEnabled = true;
        }

        protected override async void OnHideCore()
        {
            base.OnHideCore();

            if (!_success)
            {
                await CrossSdk.ConnectorController.DisconnectAsync();
            }
        }

        public async void RejectButtonClickedHandler()
        {
            try
            {
                View.ButtonsEnabled = false;
                CrossSdk.NotificationController.Notify(NotificationType.Info, "Disconnecting...");

                if (_lastSignatureRequest == null) // This shouldn't happen, but it's better to have a fallback
                {
                    await CrossSdk.ConnectorController.DisconnectAsync();
                }
                else
                {
                    await _lastSignatureRequest.RejectAsync();
                }
            }
            catch (Exception)
            {
                View.ButtonsEnabled = true;
                throw;
            }
        }

        public async void ApproveButtonClickedHandler()
        {
            try
            {
                View.ButtonsEnabled = false;
                await _lastSignatureRequest.ApproveAsync();
            }
            catch (Exception)
            {
                View.ButtonsEnabled = true;
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                CrossSdk.SiweController.Config.SignInSuccess -= SignInSuccessHandler;
                CrossSdk.SiweController.Config.SignOutSuccess -= SignOutSuccessHandler;
                CrossSdk.ConnectorController.SignatureRequested -= SignatureRequestedHandler;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}