using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PusherClient;
using EnjinPlatform.Events;
using UnityEngine;

namespace EnjinPlatform.Managers
{
    public class EnjinWebsocketManager : MonoBehaviour
    {
        public static EnjinWebsocketManager instance = null;
        private Pusher _pusher;
        private Channel _channel;
        private Dictionary<string, PusherEventListener> _listeners = new Dictionary<string, PusherEventListener>();
        [SerializeField] public string appKey;
        [SerializeField] public string host;
        private string _channelName;

        public string ChannelName
        {
            get => _channelName;
            set => _channelName = value;
        }

        private void RegisterListeners()
        {
            var listenerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(PusherEventListener)) && !t.IsAbstract);

            foreach (var type in listenerTypes)
            {
                var listener = (PusherEventListener)Activator.CreateInstance(type);
                var eventName = "platform:" + string.Concat(type.Name.Replace("Listener", "").Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
                instance._listeners.Add(eventName, listener);
            }
        }

        private void BindListeners()
        {
            foreach (var listener in _listeners)
            {
                Debug.Log("Bind: " + listener.Key);
                _channel.Bind(listener.Key, listener.Value.OnEvent);
            }
        }

        async Task Start()
        { 
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            Debug.Log("Starting Websocket Connection");
            await InitialisePusher();
        }

        private async Task InitialisePusher()
        {
            //Environment.SetEnvironmentVariable("PREFER_DNS_IN_ADVANCE", "true");

            if (_pusher == null && (appKey != ""))
            {
                RegisterListeners();
                
                _pusher = new Pusher(appKey, new PusherOptions()
                {
                    Host = host,
                    Cluster = null,
                    Encrypted = true
                });

                _pusher.Error += OnPusherOnError;
                _pusher.ConnectionStateChanged += PusherOnConnectionStateChanged;
                _pusher.Connected += PusherOnConnected;
                _pusher.Subscribed += OnChannelOnSubscribed;
                Debug.Log("Connecting to Websocket");
                await _pusher.ConnectAsync();
            }
            else
            {
                Debug.LogError(
                    "App Key and Host must be correctly set in the inspector, and the channel name must have been set after a user has logged in.");
            }
        }
        
        public async Task SubscribeToWalletAccountChannel(string channelName)
        {
            _channelName = channelName;
            _channel = await _pusher.SubscribeAsync(channelName);
            BindListeners();
        }

        private void PusherOnConnected(object sender)
        {
            Debug.Log("Connected to Websocket");
        }

        private void PusherOnConnectionStateChanged(object sender, ConnectionState state)
        {
            Debug.Log("Websocket connection state changed: " + state);
        }

        private void OnPusherOnError(object s, PusherException e)
        {
            Debug.Log("Websocket Errored");
            Debug.LogError(e.InnerException.ToString());
        }

        private void OnChannelOnSubscribed(object s, Channel channel)
        {
            Debug.Log("Websocket Channel Subscribed");
        }

        async Task OnApplicationQuit()
        {
            if (_pusher != null)
            {
                await _pusher.DisconnectAsync();
            }
        }
    }
}