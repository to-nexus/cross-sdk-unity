using System.Collections.Generic;
using Cross.Sdk.Unity.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity
{
    public class GetWalletPresenter : Presenter<GetWalletView>
    {
        // CROSSx Wallet 스토어 링크 하드코딩
        private const string IosAppStoreUrl = "https://apps.apple.com/us/app/crossx-games/id6741250674";
        private const string AndroidPlayStoreUrl = "https://play.google.com/store/apps/details?id=com.nexus.crosswallet";
        
        public override string Title => "Get CROSSx Wallet";

        private bool _disposed;

        public GetWalletPresenter(RouterController router, VisualElement parent, bool hideView = true) : base(router, parent, hideView)
        {
            if (View != null)
            {
                View.IosOptionClicked += OnIosOptionClicked;
                View.AndroidOptionClicked += OnAndroidOptionClicked;
            }
        }

        protected override GetWalletView CreateViewInstance()
        {
            var view = Parent.Q<GetWalletView>() ?? new GetWalletView();
            return view;
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            // 항상 두 옵션 모두 표시
            if (View.IosOption != null)
                View.IosOption.style.display = DisplayStyle.Flex;

            if (View.AndroidOption != null)
                View.AndroidOption.style.display = DisplayStyle.Flex;
        }

        private void OnIosOptionClicked()
        {
            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "CLICK_GET_WALLET",
                properties = new Dictionary<string, object>
                {
                    { "platform", "ios" }
                }
            });
            Application.OpenURL(IosAppStoreUrl);
        }

        private void OnAndroidOptionClicked()
        {
            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "CLICK_GET_WALLET",
                properties = new Dictionary<string, object>
                {
                    { "platform", "android" }
                }
            });
            Application.OpenURL(AndroidPlayStoreUrl);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                View.IosOptionClicked -= OnIosOptionClicked;
                View.AndroidOptionClicked -= OnAndroidOptionClicked;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
