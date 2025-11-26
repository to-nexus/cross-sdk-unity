using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Cross.Sdk.Unity.Components;
using Cross.Sdk.Unity.Model;
using Cross.Sdk.Unity.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity
{
    public class WebAppPresenter : Presenter<WebAppView>
    {
        private WalletConnectConnectionProposal _connectionProposal;
        private Wallet _wallet;
        private bool _disposed;

        private const string ContinueInTextTemplate = "Continue in {0}";

        public WebAppPresenter(RouterController router, VisualElement parent, bool hideView = true) : base(router, parent, hideView)
        {
            View.style.display = DisplayStyle.Flex;

            View.OpenLinkClicked += OnOpenLinkClicked;
            View.CopyLinkClicked += OnCopyLinkClicked;

            CrossSdk.AccountConnected += AccountConnectedHandler;
        }

        protected override WebAppView CreateViewInstance()
        {
            return Parent.Q<WebAppView>();
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            if (!CrossSdk.ConnectorController
                    .TryGetConnector<WalletConnectConnector>
                        (ConnectorType.WalletConnect, out var connector))
                throw new Exception("No WC connector"); // TODO: use custom exception

            _connectionProposal = (WalletConnectConnectionProposal)connector.Connect();

            if (WalletUtils.TryGetLastViewedWallet(out var wallet))
            {
                _wallet = wallet;
                View.SetWalletInfo(wallet.Image, string.Format(ContinueInTextTemplate, wallet.Name));
            }
        }

        private void OnOpenLinkClicked()
        {
            var encodedUri = HttpUtility.UrlEncode(_connectionProposal.Uri);
            Application.OpenURL(Path.Combine(_wallet.WebappLink, $"wc?uri={encodedUri}"));
        }

        private void OnCopyLinkClicked()
        {
            GUIUtility.systemCopyBuffer = _connectionProposal.Uri;
            CrossSdk.NotificationController.Notify(NotificationType.Success, "Link copied to clipboard");
        }

        private void AccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            if (!IsVisible)
                return;

            // Save connection method for later use (webapp also doesn't need desktop deep link)
            WalletUtils.SetConnectionMethod("webapp");

            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "CONNECT_SUCCESS",
                properties = new Dictionary<string, object>
                {
                    { "method", "web" },
                    { "name", _wallet.Name },
                    { "explorer_id", _wallet.Id }
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _connectionProposal?.Dispose();
                
                View.OpenLinkClicked -= OnOpenLinkClicked;
                View.CopyLinkClicked -= OnCopyLinkClicked;
                
                CrossSdk.AccountConnected -= AccountConnectedHandler;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}