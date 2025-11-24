using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cross.Sdk.Unity.Components;
using Cross.Sdk.Unity.Utils;
using Cross.Sign.Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cross.Sdk.Unity
{
    /// <summary>
    ///     ModalController for Unity UI Toolkit
    /// </summary>
    public class ModalControllerUtk : ModalController
    {
        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnClose = new(false);

        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnOpen = new(true);
        public UIDocument UIDocument { get; private set; }

        public Modal Modal { get; private set; }

        public VisualElement CrossSdkModalElement { get; private set; }

        public RouterController RouterController { get; private set; }

        protected ModalHeaderPresenter ModalHeaderPresenter { get; private set; }

        protected override Task InitializeAsyncCore()
        {
            var web3Modal = CrossSdk.Instance;
            UIDocument = web3Modal.GetComponentInChildren<UIDocument>(true);

            CrossSdkModalElement = UIDocument.rootVisualElement.Children().First();

            Modal = CrossSdkModalElement.Q<Modal>();

            RouterController = new RouterController(Modal.body);
            RouterController.ViewChanged += ViewChangedHandler;

            ModalHeaderPresenter = new ModalHeaderPresenter(RouterController, Modal);

            LoadingAnimator.Instance.PauseAnimation();

            UnityEventsDispatcher.Instance.Tick += TickHandler;

            return Task.CompletedTask;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            if (args.newViewType == ViewType.None)
                CloseCore();
        }

        protected override void OpenCore(ViewType view)
        {
            CrossSdkModalElement.visible = true;
            RouterController.OpenView(view);
            LoadingAnimator.Instance.ResumeAnimation();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnOpen);

            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "MODAL_OPEN",
                properties = new Dictionary<string, object>
                {
                    { "connected", CrossSdk.IsAccountConnected }
                }
            });
        }

        protected override void CloseCore()
        {
            CrossSdkModalElement.visible = false;
            LoadingAnimator.Instance.PauseAnimation();
            RouterController.CloseAllViews();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnClose);

            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "MODAL_CLOSE",
                properties = new Dictionary<string, object>
                {
                    { "connected", CrossSdk.IsAccountConnected }
                }
            });
        }

        private void TickHandler()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                RouterController.GoBack();
        }
    }
}