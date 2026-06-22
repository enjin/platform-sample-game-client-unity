using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using HappyHarvest.EnjinIntegration.Core;
using HappyHarvest.EnjinIntegration.Data;

namespace HappyHarvest.EnjinIntegration.UI
{
    public class BackpackUI
    {
        public System.Action OnClose;
        public System.Action OnOpen;

        private VisualElement m_Root;
        private VisualTreeAsset m_ItemEntryTemplate;
        private ListView m_ItemList;
        private Label m_LoadingLabel;

        private Button m_OpenBackpack;

        public BackpackUI(VisualElement root, VisualTreeAsset itemEntryTemplate)
        {
            m_Root = root.Q<VisualElement>("BackpackUI");
            m_OpenBackpack = root.Q<Button>("OpenBackpack");
            m_ItemEntryTemplate = itemEntryTemplate;
            m_ItemList = m_Root.Q<ListView>("BlockchainItems");
            m_LoadingLabel = m_Root.Q<Label>("LoadingLabel");

            // Rows are interactive controls (amount field + Melt/Send), not a
            // pick list, so disable selection. This also removes the selection
            // highlight that turned the editable field's text white/unreadable.
            m_ItemList.selectionType = SelectionType.None;

            m_OpenBackpack.clicked += () =>
            {
                if (m_Root.visible)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            };

            EnjinManager.Instance.OnWalletUpdated += Refresh;
        }

        public void Dispose()
        {
            EnjinManager.Instance.OnWalletUpdated -= Refresh;
        }

        private async void Open()
        {
            // Show the window right away so opening feels instant; the wallet
            // fetch then runs behind a loading indicator instead of blocking
            // the window from appearing.
            m_Root.visible = true;
            OnOpen?.Invoke();

            await LoadAndFill();
        }

        // Whether the backpack is currently shown. Used by UIHandler to drive
        // the shared Escape / click-outside dismissal.
        public bool IsOpen => m_Root.visible;

        // The visible panel, used for click-outside hit-testing. The backpack
        // has no full-screen backdrop, so this is the panel element itself.
        public VisualElement Panel => m_Root;

        public void Close()
        {
            m_ItemList.visible = false;
            m_Root.visible = false;
            OnClose?.Invoke();
        }

        public async void Refresh()
        {
            // Don't do anything if the backpack isn't visible.
            if (m_Root == null || !m_Root.visible)
                return;

            Debug.Log("New item received! Refreshing backpack...");
            await LoadAndFill();
        }

        // Fetch the wallet tokens behind a loading indicator, then populate the
        // list. The window is expected to already be visible when this runs.
        private async Task LoadAndFill()
        {
            SetLoading(true);
            await GetManagedWalletAccount();
            SetLoading(false);
            FillItemList();
        }

        // Show the "Loading…" placeholder in place of the (empty) item list
        // while the wallet fetch is in flight.
        private void SetLoading(bool loading)
        {
            if (m_LoadingLabel != null)
                m_LoadingLabel.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;

            // Use display (not just visibility) so the hidden one doesn't take
            // up layout space in the fixed-height panel. Also reset visibility,
            // since Close() hides the list via visibility.
            m_ItemList.style.display = loading ? DisplayStyle.None : DisplayStyle.Flex;
            m_ItemList.visible = !loading;
        }

        async Task GetManagedWalletAccount()
        {
            //await EnjinPlatformService.Instance.GetManagedWalletAccount(true);
            await EnjinManager.Instance.GetManagedWalletTokens();
        }

        private void FillItemList()
        {
            m_ItemList.Clear();
            //EnjinPlatformService.TokenAccount[] items = EnjinPlatformService.Instance.ManagedWalletAccount.tokens;
            // walletAccount is null until a successful fetch (e.g. before login),
            // so fall back to an empty list rather than throwing.
            PlatformModels.TokenAccount[] items =
                EnjinManager.Instance.walletAccount?.tokenAccounts ?? new PlatformModels.TokenAccount[0];
            m_ItemList.makeItem = () =>
            {
                var newListEntry = m_ItemEntryTemplate.Instantiate();
                var newListEntryLogic = new BackpackItemController();

                newListEntry.userData = newListEntryLogic;
                newListEntryLogic.SetVisualElement(newListEntry);
                newListEntryLogic.SetMeltButton(newListEntry.Q<Button>("MeltButton"));
                newListEntryLogic.SetTransferButton(newListEntry.Q<Button>("TransferButton"));
                newListEntryLogic.SetRecipient(m_Root.Q<TextField>("Recipient"));

                return newListEntry;
            };

            m_ItemList.bindItem = (item, index) =>
            {
                var userData = (item.userData as BackpackItemController);
                userData?.SetTokenAccount(items[index]);
                userData?.SetName();
            };
            m_ItemList.fixedItemHeight = 64;
            m_ItemList.itemsSource = items;
        }
    }
}