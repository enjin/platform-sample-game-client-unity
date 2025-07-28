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

        private Button m_OpenBackpack;

        public BackpackUI(VisualElement root, VisualTreeAsset itemEntryTemplate)
        {
            m_Root = root.Q<VisualElement>("BackpackUI");
            m_OpenBackpack = root.Q<Button>("OpenBackpack");
            m_ItemEntryTemplate = itemEntryTemplate;
            m_ItemList = m_Root.Q<ListView>("BlockchainItems");

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
            await GetManagedWalletAccount();
            FillItemList();

            m_Root.visible = true;
            m_ItemList.visible = true;
            OnOpen?.Invoke();
        }

        private void Close()
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
            await GetManagedWalletAccount();
            FillItemList();
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
            PlatformModels.TokenAccount[] items = EnjinManager.Instance.walletAccount.tokenAccounts;
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