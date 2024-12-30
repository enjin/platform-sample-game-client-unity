using System.Collections;
using System.Collections.Generic;
using EnjinPlatform.Managers;
using UnityEngine;
using UnityEngine.UIElements;
using EnjinPlatform.Services;

namespace Template2DCommon
{
    public class SettingMenu
    {
        public System.Action OnClose;
        public System.Action OnOpen;

        private VisualElement m_Root;
        private Button m_OpenMenu;

        private VisualElement m_Login;
        private TextField m_EmailField;
        private TextField m_PasswordField;
        private Button m_LoginButton;
        private Button m_CancelLoginButton;
        
        private VisualElement m_MessageBox;
        private Label m_MessageLabel;
        private Button m_OkButton;
        
        private Button m_OpenLoginButton;
        private Button m_CloseButton;
        private Button m_QuitButton;

        private DropdownField m_ResolutionDropdown;
        private Toggle m_FullscreenToggle;

        private Slider m_MainVolumeSlider;
        private Slider m_BGMVolumeSlider;
        private Slider m_SFXVolumeSlider;

        private List<Resolution> m_AvailableResolutions;
    
        public SettingMenu(VisualElement root)
        {
            m_Root = root.Q<VisualElement>("SettingMenu");
            m_OpenMenu = root.Q<Button>("OpenSettingMenuButton");
            
            m_Login = root.Q<VisualElement>("LoginMenu");
            m_EmailField = m_Login.Q<TextField>("Email");
            m_PasswordField = m_Login.Q<TextField>("Password");
            m_LoginButton = m_Login.Q<Button>("LoginButton");
            m_CancelLoginButton = m_Login.Q<Button>("CancelLoginButton");
            
            m_MessageBox = root.Q<VisualElement>("MessageBox");
            m_MessageLabel = m_MessageBox.Q<Label>("MessageLabel");
            m_OkButton = m_MessageBox.Q<Button>("OkButton");
            
            m_OpenLoginButton = m_Root.Q<Button>("OpenLoginButton");
            m_CloseButton = m_Root.Q<Button>("CloseButton");
            m_QuitButton = m_Root.Q<Button>("QuitButton");

            m_ResolutionDropdown = m_Root.Q<DropdownField>("ResolutionDropdown");
            m_FullscreenToggle = m_Root.Q<Toggle>("FullscreenToggle");

            m_MainVolumeSlider = m_Root.Q<Slider>("MainVolume");
            m_BGMVolumeSlider = m_Root.Q<Slider>("MusicVolume");
            m_SFXVolumeSlider = m_Root.Q<Slider>("SFXVolume");

            m_MainVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                SoundManager.Instance.Sound.MainVolume = evt.newValue;
                SoundManager.Instance.UpdateVolume();
            });
            m_BGMVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                SoundManager.Instance.Sound.BGMVolume = evt.newValue;
                SoundManager.Instance.UpdateVolume();
            });
            m_SFXVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                SoundManager.Instance.Sound.SFXVolume = evt.newValue;
                SoundManager.Instance.UpdateVolume();
            });

            m_Root.visible = false;
            m_Login.visible = false;
            m_MessageBox.visible = false;

            Debug.Log(EnjinPlatformService.Instance.IsLoggedIn());
            if (EnjinPlatformService.Instance.IsLoggedIn())
            {
                m_OpenLoginButton.text = "Logout";
            }

            m_OpenMenu.clicked += () =>
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

            m_OpenLoginButton.clicked += () =>
            {
                if (EnjinPlatformService.Instance.IsLoggedIn())
                {
                    EnjinPlatformService.Instance.Logout();
                    m_Login.visible = false;
                    return;
                }
                
                m_Login.visible = true;
                m_Root.visible = false;
                m_EmailField.value = "";
                m_PasswordField.value = "";
            };
            
            m_CancelLoginButton.clicked += () =>
            {
                m_Login.visible = false;
                m_Root.visible = true;
            };
            
            m_LoginButton.clicked += () =>
            {
                if(m_MessageBox.visible)
                    return;
                
                if (string.IsNullOrEmpty(m_EmailField.value) || string.IsNullOrEmpty(m_PasswordField.value))
                {
                    m_MessageLabel.text = "Email or password is empty";
                    m_MessageBox.visible = true;
                    return;
                }
                
                m_LoginButton.SetEnabled(false);
                m_CancelLoginButton.SetEnabled(false);
                
                EnjinPlatformService.Instance.OnLoginSuccess += async success =>
                {
                    if (success)
                    {
                        await EnjinPlatformService.Instance.CreateManagedWalletAccount();
                        await EnjinPlatformService.Instance.GetManagedWalletAccount();
                        
                        m_Login.visible = false;
                        m_Root.visible = true;
                    }
                    else
                    {
                        m_MessageLabel.text = "Login failed";
                        m_MessageBox.visible = true;
                        m_LoginButton.SetEnabled(true);
                        m_CancelLoginButton.SetEnabled(true);
                    }
                };
                
                EnjinPlatformService.Instance.RegisterAndLogin(m_EmailField.value, m_PasswordField.value);
            };

            if (m_MessageBox.visible)
            {
                m_OkButton.clicked += () =>
                {
                    m_MessageLabel.text = "";
                    m_MessageBox.visible = false;
                };
            }

            m_CloseButton.clicked += Close;
            m_QuitButton.clicked += Application.Quit;
        
            //fill resolution dropdown
            m_AvailableResolutions = new List<Resolution>();
        
            List<string> resEntries = new List<string>();
            foreach (var resolution in Screen.resolutions)
            {
                //if we already have a resolution with same width & height, we skip.
                if(m_AvailableResolutions.FindIndex(r => r.width == resolution.width && r.height == resolution.height) != -1)
                    continue;
            
                var resName = resolution.width+"x"+resolution.height;
                resEntries.Add(resName);
                m_AvailableResolutions.Add(resolution);
            
            }

            m_ResolutionDropdown.choices = resEntries;

            m_ResolutionDropdown.RegisterValueChangedCallback(evt =>
            {
                if (m_ResolutionDropdown.index == -1)
                    return;
            
                var res = m_AvailableResolutions[m_ResolutionDropdown.index];
                Screen.SetResolution(res.width, res.height, m_FullscreenToggle.value);
            });

            m_FullscreenToggle.value = Screen.fullScreen;
            m_FullscreenToggle.RegisterValueChangedCallback(evt =>
            {
                Screen.fullScreen = evt.newValue;
            });
        }

        bool CompareResolution(Resolution a, Resolution b)
        {
            return a.width == b.width && a.height == b.height && a.refreshRateRatio.CompareTo(b.refreshRateRatio) == 0;
        }

        void Open()
        {
            m_MainVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.Sound.MainVolume);
            m_BGMVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.Sound.BGMVolume);
            m_SFXVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.Sound.SFXVolume);
        
            string currentRes = Screen.width + "x" + Screen.height;
            m_ResolutionDropdown.label = currentRes;
            m_ResolutionDropdown.SetValueWithoutNotify(currentRes);
        
            m_Login.visible = false;
            m_MessageBox.visible = false;
            m_Root.visible = true;
            OnOpen.Invoke();   
        }

        void Close()
        {
            SoundManager.Instance.PlayUISound();
            SoundManager.Instance.Save();
            m_Root.visible = false;
            m_Login.visible = false;
            m_MessageBox.visible = false;
            m_LoginButton.SetEnabled(true);
            m_CancelLoginButton.SetEnabled(true);
            OnClose.Invoke();
        }
    }
}