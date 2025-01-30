using EnjinPlatform.Art.UI;
using EnjinPlatform.Managers;
using EnjinPlatform.Services;
using UnityEngine;
using UnityEngine.UIElements;

public class BackpackUI
{
    public System.Action OnClose;
    public System.Action OnOpen;
    
    private VisualElement m_Root;
    private VisualTreeAsset m_ItemEntryTemplate;
    private ListView m_ItemList;
    
    private Button m_OpenBackpack;

    private int _clickCount;

    public BackpackUI(VisualElement root, VisualTreeAsset itemEntryTemplate)
    {
        m_Root = root.Q<VisualElement>("BackpackUI");
        m_OpenBackpack = root.Q<Button>("OpenBackpack");
        m_ItemEntryTemplate = itemEntryTemplate;
        m_ItemList = m_Root.Q<ListView>("BlockchainItems");
        
        Debug.Log(m_OpenBackpack);
        
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
    }
    
    private void Open()
    {
        GetManagedWalletAccount();
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
    
    async void GetManagedWalletAccount()
    {
        await EnjinPlatformService.Instance.GetManagedWalletAccount(true);
    }
    
    private void FillItemList()
    {
        m_ItemList.Clear();
        EnjinPlatformService.TokenAccount[] items = EnjinPlatformService.Instance.ManagedWalletAccount.tokens;
        m_ItemList.makeItem = () =>
        {
            var newListEntry = m_ItemEntryTemplate.Instantiate();
            var newListEntryLogic = new BlockchainItem();
                
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);
            newListEntryLogic.SetMeltButton(newListEntry.Q<Button>("MeltButton"));
            newListEntryLogic.SetTransferButton(newListEntry.Q<Button>("TransferButton"));
            newListEntryLogic.SetRecipient(m_Root.Q<TextField>("Recipient"));
                
            return newListEntry;
        };
        
        m_ItemList.bindItem = (item, index) =>
        {
            var userData = (item.userData as BlockchainItem);
            userData?.SetTokenAccount(items[index]);
            userData?.SetName(items[index].token.name);
        };
        m_ItemList.fixedItemHeight = 64;
        m_ItemList.itemsSource = items;
    }
}