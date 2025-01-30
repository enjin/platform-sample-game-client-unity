using System;
using EnjinPlatform.Data;
using EnjinPlatform.Managers;
using EnjinPlatform.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnjinPlatform.Art.UI
{
    public class BlockchainItem
    {
        private IntegerField m_ItemDetails;
        private Button m_Melt;
        private Button m_Transfer;
        private EnjinPlatformService.TokenAccount m_TokenAccount;
        private EnjinBlockchainToken m_Token;
        private TextField m_Recipient;
        
        public void SetTokenAccount(EnjinPlatformService.TokenAccount tokenAccount)
        {
            m_TokenAccount = tokenAccount;
            int.TryParse(tokenAccount.balance, out int balance);
            m_ItemDetails.value = balance;
            m_ItemDetails.maxLength = balance;
            m_Token = EnjinGameManager.Instance.GetToken(m_TokenAccount.token.collectionId, m_TokenAccount.token.tokenId);
        }
        
        public void SetVisualElement(VisualElement visualElement)
        {
            m_ItemDetails = visualElement.Q<IntegerField>("ItemDetails");
        }
        
        public void SetRecipient(TextField recipient)
        {
            m_Recipient = recipient;
        }
        
        public void SetMeltButton(Button meltButton)
        {
            m_Melt = meltButton;
            m_Melt.clicked += () =>
            {
                if (int.TryParse(m_TokenAccount.balance, out int balance))
                {
                    if (m_ItemDetails.value > balance)
                    {
                        Debug.Log("Trying to melt " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                        Debug.Log("Cannot melt more than you have.");
                    }
                    else
                    {
                        m_Token.item.ItemService.Melt(m_ItemDetails.value);
                        Debug.Log("Melt " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                    }
                }
            };
        }
        
        public void SetTransferButton(Button transferButton)
        {
            m_Transfer = transferButton;
            m_Transfer.clicked += () =>
            {
                if (int.TryParse(m_TokenAccount.balance, out int balance))
                {
                    if (m_ItemDetails.value > balance)
                    {
                        Debug.Log("Trying to send " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                        Debug.Log("Cannot send more than you have.");
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(m_Recipient.text))
                        {
                            Debug.Log("Recipient is null.");
                        }
                        else
                        {
                            m_Token.Transfer(m_Recipient.text, m_ItemDetails.value);
                            Debug.Log("Send " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                        }
                    }
                }
            };
        }
        
        public void SetName(string name)
        {
            m_ItemDetails.label = name;
        }
    }
}