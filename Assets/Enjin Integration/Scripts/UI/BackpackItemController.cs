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
        private Label m_ItemName;
        private Label m_OwnedAmount;
        private Button m_Melt;
        private Button m_Transfer;
        private PlatformModels.TokenAccount m_TokenAccount;
        private EnjinToken m_Token;
        private TextField m_Recipient;
        
        public void SetTokenAccount(PlatformModels.TokenAccount tokenAccount)
        {
            m_TokenAccount = tokenAccount;
            int.TryParse(tokenAccount.balance, out int balance);

            // Read-only display of the amount currently owned (the raw balance
            // string, so very large values display without int overflow).
            if (m_OwnedAmount != null)
                m_OwnedAmount.text = tokenAccount.balance;

            // Editable amount defaults to 1 when they own at least one, rather
            // than pre-filling the entire balance.
            m_ItemDetails.value = balance >= 1 ? 1 : balance;
            m_ItemDetails.maxLength = balance;
            m_Token = EnjinManager.Instance.GetToken(m_TokenAccount.token.collection.collectionId, m_TokenAccount.token.tokenId);
        }
        
        public void SetVisualElement(VisualElement visualElement)
        {
            m_ItemDetails = visualElement.Q<IntegerField>("ItemDetails");
            m_ItemName = visualElement.Q<Label>("ItemName");
            m_OwnedAmount = visualElement.Q<Label>("OwnedAmount");
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
                        // Fire-and-forget: UI refresh happens via OnWalletUpdated.
                        _ = m_Token.item.Melt(m_ItemDetails.value);
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
            if (m_ItemName != null)
                m_ItemName.text = m_Token?.item != null ? m_Token.item.DisplayName : "";
        }
    }
}