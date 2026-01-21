using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity.Components
{
    public class GetWalletView : VisualElement
    {
        public const string Name = "get-wallet-view";
        public static readonly string NameIosOption = "ios-option";
        public static readonly string NameAndroidOption = "android-option";

        private readonly VisualElement _iosOption;
        private readonly VisualElement _androidOption;

        public event Action IosOptionClicked;
        public event Action AndroidOptionClicked;

        public VisualElement IosOption => _iosOption;
        public VisualElement AndroidOption => _androidOption;

        public new class UxmlFactory : UxmlFactory<GetWalletView>
        {
        }

        public GetWalletView() : this(null)
        {
        }

        public GetWalletView(string visualTreePath)
        {
            var asset = Resources.Load<VisualTreeAsset>(visualTreePath ?? "Cross/Sdk/Views/GetWalletView/GetWalletView");
            asset.CloneTree(this);

            name = Name;
            AddToClassList("get-wallet-view");

            _iosOption = this.Q<VisualElement>(NameIosOption);
            _androidOption = this.Q<VisualElement>(NameAndroidOption);

            if (_iosOption != null)
                _iosOption.RegisterCallback<ClickEvent>(_ => IosOptionClicked?.Invoke());
            
            if (_androidOption != null)
                _androidOption.RegisterCallback<ClickEvent>(_ => AndroidOptionClicked?.Invoke());
        }
    }
}
