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
        
        // Cache values to prevent recursive layout updates
        private float _lastPadding = -1f;
        private float _lastScrollViewWidth = -1f;
        private bool _isUpdatingLayout = false;

        public NetworkSearchPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
            // Register to scrollView only, not the entire View, to prevent recursive layout
            View.scrollView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

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
            // Skip if already updating to prevent recursive layout
            if (_isUpdatingLayout)
                return;
                
            var currentWidth = View.scrollView.resolvedStyle.width;
            
            // Skip if width hasn't changed significantly (less than 5px) to prevent recursive layout updates
            const float WIDTH_CHANGE_THRESHOLD = 5f;
            if (Mathf.Abs(currentWidth - _lastScrollViewWidth) < WIDTH_CHANGE_THRESHOLD)
                return;
            
            _lastScrollViewWidth = currentWidth;
            
            // Defer padding update to next frame to prevent recursive layout
            View.scrollView.schedule.Execute(() =>
            {
                if (!_isUpdatingLayout)
                    ConfigureItemPaddings();
            }).ExecuteLater(10);
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
            // Prevent recursive calls
            if (_isUpdatingLayout)
                return;
                
            _isUpdatingLayout = true;
            
            try
            {
                var scrollViewWidth = View.scrollView.resolvedStyle.width;
                const float itemWidth = 79;
                var itemsCanFit = Mathf.FloorToInt(scrollViewWidth / itemWidth);
                
                // Avoid division by zero
                if (itemsCanFit <= 0)
                    return;

                // Round to avoid floating point precision issues causing jitter
                var rawPadding = (scrollViewWidth - itemsCanFit * itemWidth) / itemsCanFit / 2;
                var padding = Mathf.Round(rawPadding);
                
                // Skip if padding hasn't changed significantly (less than 2px)
                const float PADDING_CHANGE_THRESHOLD = 2f;
                if (Mathf.Abs(padding - _lastPadding) < PADDING_CHANGE_THRESHOLD)
                    return;
                
                _lastPadding = padding;
                items ??= _items;

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    item.style.paddingLeft = padding;
                    item.style.paddingRight = padding;
                }
            }
            finally
            {
                _isUpdatingLayout = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                View.scrollView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                CrossSdk.NetworkController.ChainChanged -= ChainChangedHandler;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}