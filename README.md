# Game Client Quickstart

The sample game uses Unity 6.  Once installed pull the repo from GitHub and then load it up through Unity Hub.  After the assets are built it's time to configure the connections to the game server as well as the platform websocket server.

Find the `Assets/EnjinPlatform/GraphQL/EnjinPlatformGraphQlReference` scriptable object in the project window and click it to open its properties in the inspector.  Change the Url field to the URL of your game server the click `Introspect`.  It should connect to your game server and pull the GraphQL schema data.

Next find the `Assets/HappyHarvest/Resources/GameManager` prefab and click it to open it in the inspector.  In the inspector you will see the `EnjinWebsocketManager` script has been added to it.  This is where you can set which platform server to connect to for websocket events.  Make sure the platform you connect to is the same one which your game server also connects to.  The `App Key` should be the key supplied by the websocket sever to identify the app it's running, for self-hosted platforms this will be the key you set in your config, for the Cloud hosted platform this can be found in the [docs](https://docs.enjin.io/docs/websocket-events).

After the above config is complete you should be able to load the `Farm_Outdoor` scene into the editor window and hit play.  From here we now need to 'login' to the game server and retrieve our managed wallet.

With the game running click the `Menu` button in the top right.  Next press `Login` and enter your email address and a password.  If an account for your email doesn't yet exist then one will be created the first time you try to log in.  After a moment you should be logged in and a few more moments later a managed wallet should be returned to you.

Once you are logged in the game will store your login auth token locally so you don't need to log in in each time the game loads.  If an auth token is found then it will log in automatically right away and get your managed wallet.

To play the game walk around using `W/S/A/D`, find some empty land in your fields and then use the hoe tool to dig by clicking the space with the left mouse button.  There is a random chance of finding either a plain gold coin, a gold could with a gem in it, or a gemstone.  These are the items that you can collect on the blockchain.  Click the item to pick it up and this will initiate a mint on the blockchain.  After 30-60s the item will be in your managed wallet, and you will be able to see it on chain in [Subscan](https://canary-matrix.subscan.io/).  You will also be able to check your `backpack` in the top right to see your blockchain items loaded into the game.