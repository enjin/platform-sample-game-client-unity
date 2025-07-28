using System;
using UnityEngine;
using UnityEngine.UIElements;
using HappyHarvest.EnjinIntegration.Core;
using HappyHarvest.EnjinIntegration.Data;
using HappyHarvest.EnjinIntegration.Gameplay;

namespace HappyHarvest.EnjinIntegration.UI
{
    public class BackpackItemController
    {
        private IntegerField m_ItemDetails;
        private Button m_Melt;
        private Button m_Transfer;
        private PlatformModels.TokenAccount m_TokenAccount;
        private EnjinToken m_Token;
        private TextField m_Recipient;
        
        public void SetTokenAccount(PlatformModels.TokenAccount tokenAccount)
        {
            m_TokenAccount = tokenAccount;
            int.TryParse(tokenAccount.balance, out int balance);
            m_ItemDetails.value = balance;
            m_ItemDetails.maxLength = balance;
            m_Token = EnjinManager.Instance.GetToken(m_TokenAccount.token.collection.collectionId, m_TokenAccount.token.tokenId);
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
                        Debug.Log("Melt " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                        m_Token.item.Melt(m_ItemDetails.value);
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
                            Debug.Log("Send " + m_ItemDetails.value + " of " + m_TokenAccount.balance + " " + m_ItemDetails.label);
                            m_Token.Transfer(m_Recipient.text, m_ItemDetails.value);
                        }
                    }
                }
            };
        }
        
        public void SetName()
        {
            m_ItemDetails.label = m_Token.item.DisplayName;
        }
    }
}