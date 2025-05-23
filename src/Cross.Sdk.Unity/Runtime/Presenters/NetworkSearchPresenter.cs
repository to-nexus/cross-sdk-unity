using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cross.Sdk.Unity.Components;
using Cross.Sdk.Unity.Utils;
using Cross.Core.Common.Model.Errors;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity
{
    public class NetworkSearchPresenter : Presenter<NetworkSearchView>
    {
        public override string Title
        {
            get => "Choose network";
        }

        private readonly List<VisualElement> _items = new();

        private readonly Dictionary<string, CardSelect> _netowrkItems = new();
        private string _highlightedChainId;
        private bool _disposed;

        public NetworkSearchPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
            View.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            CrossSdk.Initialized += (_, _) =>
            {
                foreach (var chain in CrossSdk.NetworkController.Chains.Values)
                {
                    var item = MakeNetworkItem(chain);

                    _netowrkItems[chain.ChainId] = item;
                    View.scrollView.Add(item);
                }

                CrossSdk.NetworkController.ChainChanged += ChainChangedHandler;

                var activeChain = CrossSdk.NetworkController.ActiveChain;
                if (activeChain != default)
                    HighlightActiveChain(activeChain.ChainId);
            };
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            if (e.NewChain == null)
                return;

            HighlightActiveChain(e.NewChain.ChainId);
        }

        private void HighlightActiveChain(string chainId)
        {
            if (_highlightedChainId != null && _netowrkItems.TryGetValue(_highlightedChainId, out var prevItem))
                prevItem.SetActivated(false);

            if (_netowrkItems.TryGetValue(chainId, out var item))
                item.SetActivated(true);

            _highlightedChainId = chainId;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ConfigureItemPaddings();
        }

        private CardSelect MakeNetworkItem(Chain chain)
        {
            var item = new CardSelect
            {
                LabelText = chain.Name
            };

            item.Clicked += async () =>
            {
                PlayerPrefs.SetString("WC_SELECTED_CHAIN_ID", chain.ChainId);
                if (!CrossSdk.IsAccountConnected)
                {
                    await CrossSdk.NetworkController.ChangeActiveChainAsync(chain);
                    Router.OpenView(ViewType.Connect);
                }
                else
                {
                    await ChangeChainWithTimeout(chain);
                }
            };

            var hexagon = Resources.Load<VectorImage>("Cross/Sdk/Images/hexagon");
            var imageContainer = item.Q<VisualElement>(CardSelect.NameImageContainer);
            imageContainer.style.backgroundImage = new StyleBackground(hexagon);
            imageContainer.style.width = 52;
            item.Q<VisualElement>(CardSelect.NameIconImageBorder).style.display = DisplayStyle.None;

            var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>(chain.ImageUrl);
            item.Icon = remoteSprite;

            _items.Add(item);
            return item;
        }

        private async Task ChangeChainWithTimeout(Chain chain)
        {
            try
            {
                var changeChainTask = CrossSdk.NetworkController.ChangeActiveChainAsync(chain);

                await Task.Delay(TimeSpan.FromMilliseconds(70));

                if (changeChainTask.IsCompleted)
                    Router.GoBack();
                else
                    Router.OpenView(ViewType.NetworkLoading);

                await changeChainTask;
            }
            catch (CrossNetworkException e)
            {
                // If user declines network switch, MetaMask returns a long json error message.
                // The message is not user-friendly, so we show a default error message instead.
                var defaultErrorMessage = SdkErrors.MessageFromType(e.CodeType);
                CrossSdk.NotificationController.Notify(NotificationType.Error, defaultErrorMessage);
                Router.GoBack();
            }
            catch (Exception e)
            {
                CrossSdk.NotificationController.Notify(NotificationType.Error, e.Message);
                Router.GoBack();
                throw;
            }
        }

        private void ConfigureItemPaddings(IList<VisualElement> items = null)
        {
            var scrollViewWidth = View.scrollView.resolvedStyle.width;
            const float itemWidth = 79;
            var itemsCanFit = Mathf.FloorToInt(scrollViewWidth / itemWidth);

            var padding = (scrollViewWidth - itemsCanFit * itemWidth) / itemsCanFit / 2;
            items ??= _items;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.style.paddingLeft = padding;
                item.style.paddingRight = padding;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                CrossSdk.NetworkController.ChainChanged += ChainChangedHandler;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}