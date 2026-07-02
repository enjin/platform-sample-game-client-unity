# Introduction

This repository contains the Unity client for the Enjin Platform sample game. It's a simple farming game designed to demonstrate how to integrate Enjin's NFT technology into a Unity project. 🧑‍🌾

This client communicates with a [C# game server](https://github.com/enjin/platform-sample-game-server) (ASP.NET Core, built on the [Enjin Platform C# SDK](https://github.com/enjin/platform-csharp-sdk) v3) to handle all blockchain-related actions like minting, melting, reading players' wallets, and transferring NFTs.

-----

## Quick Start Guide

This guide will get you up and running quickly.

### **Prerequisites**

  - ✅ Unity Hub with Unity Editor version `6000.3.16f1`
  - ✅ Git

-----

### **Step 1: Set Up the Game Server**

⚠️ **Important:** Before you can run this Unity client, you **must set up and run the C# game server first**.

Follow the setup instructions in the [game server repository](https://github.com/enjin/platform-sample-game-server). The server will start on `http://localhost:3000` by default, create its managed daemon wallet, and create a Collection on first run. The Collection ID is persisted in the server's `state.json` and exposed via `GET /api/setup/collection-id`.

You no longer need to copy the Collection ID by hand — the Unity client pulls it from the server in Step 4.

-----

### **Step 2: Clone the Client Repository**

Open a terminal or command prompt and run:

```bash
git clone https://github.com/enjin/platform-sample-game-client-unity.git
```

-----

### **Step 3: Open the Project in Unity**

1.  Open **Unity Hub**.
2.  Click `Add` -\> `Add project from disk`.
3.  Select the cloned `platform-sample-game-client-unity` folder.
4.  Open the project in the Unity Editor.

> **Note: all blockchain actions (mint, melt, transfer, wallet reads) go through the [C# game server](https://github.com/enjin/platform-sample-game-server)'s REST API** — the client's `EnjinApiService` simply makes HTTP calls. The API key and Platform SDK live on the **server**, safely away from the client. The pattern to study here is **game client → your server → Enjin Platform**, not calling the Platform directly from the client (which would ship your API token inside the build).

-----

### **Step 4: Stamp the Collection ID onto Your Item Assets (Required)**

The three `EnjinItem` assets shipped with the client (`GemGreen`, `GoldCoin`, `GoldCoinBlue` under `Assets/Enjin Integration/Scripts/Data/Items`) ship with a placeholder Collection ID and must be stamped with the value created by **your** running server before the game will recognise minted tokens.

With the server running:

1.  In the Unity Editor menu bar, choose **Enjin → Stamp Collection ID onto EnjinItem Assets**.
2.  Confirm the server host in the dialog (defaults to `http://localhost:3000`, persisted in `EditorPrefs` between runs).
3.  The editor will call `GET /api/setup/collection-id`, write the returned value into every `EnjinItem` asset's `Collection Id` field, save the assets, and report how many were updated.

Re-run this menu item any time you point the client at a different server, or after resetting a server's `state.json` (which generates a new Collection).

#### Manual fallback

If the menu item fails (for example, the server is unreachable from the Editor or you'd rather copy/paste), you can stamp the assets by hand:

1.  Get the Collection ID. With the server running, either open `http://localhost:3000/api/setup/collection-id` in a browser, `curl` it, or read the `collectionId` value out of the server's `state.json`.
2.  In Unity's `Project` window, navigate to `Assets/Enjin Integration/Scripts/Data/Items`.
3.  Select **each** of `GemGreen`, `GoldCoin`, and `GoldCoinBlue` in turn. In the Inspector, paste the Collection ID into the **Collection Id** field.
4.  Save the project (`Ctrl/Cmd+S`).

> **Note:** If you run the server on a non-default host or port, also select the `EnjinManager` prefab in `Assets/Enjin Integration/Prefabs/` and update the `Host` field on the `Enjin API Service` component. The default is `http://localhost:3000`.

-----

### **Step 5: Run the Game & How to Play 🎮**

Make sure the game server is still running in the background.

1.  In the Unity Editor's `Project` window, navigate to `Assets/HappyHarvest/Scenes`.
2.  Double-click the `MainMenu` scene and press **Play**, then click **Start** to enter the farm. (You can also open `Farm_Outdoor` directly, but starting from `MainMenu` ensures the `EnjinManager` prefab is initialised.)

Once the game is running:

1.  **Login:** Click the **Menu** button (top-right), then **Login**. Enter any email and password to register a new player. This automatically creates a managed wallet for you on the platform.
2.  **Move:** Use the **W, A, S, D** keys to move your character.
3.  **Harvest:** Walk up to a crop and click on it to harvest it.
4.  **Collect NFTs:** Keep harvesting until a resource item (gem or coin) appears. Click on the item to collect it — this mints the item as an NFT to your player's wallet.
5.  **View Your NFTs:** Click the **Backpack** button (top-right) to see your collected NFTs. From there you can **Melt** them or **Send** them to another wallet.

-----

## Optional: Smoke Test Harness

A standalone test scene is included for verifying the integration without playing the full game:

- **`Assets/GameServerSmoke/`** — exercises the client's `EnjinApiService` end-to-end against a running game server (register/login, mint, melt, transfer, balance queries). Open via **Enjin → Open Game Server Smoke Scene** and press Play.

See the `README.md` inside the folder for details.

-----

## Editor Menu Items Reference

| Menu Path | When to Use |
|---|---|
| **Enjin → Stamp Collection ID onto EnjinItem Assets** | **Required** before first play, and any time you change which server you connect to. |
| **Enjin → Open Game Server Smoke Scene** | Optional. Build/open the standalone scene that tests the client against the game server. |

-----

## Why a game server instead of calling the Platform directly?

**Security.** Every Platform call is authenticated with your Platform API token. If your Unity game called the Platform directly, that token would have to be embedded in the build you hand to players — and a token shipped inside a distributed app can be extracted and used to act as your account (mint, transfer, drain funds).

To avoid that, this sample keeps the token on a small [game server](https://github.com/enjin/platform-sample-game-server) it controls. The Unity client only ever calls that server, and the server makes the Platform calls on its behalf. The token never leaves your infrastructure. The server can also enforce its own rules — authentication, validation, rate limiting, anti-cheat — before it ever touches the Platform.

**Rule of thumb:** if players will run your build, keep the token behind a server. That client→server split is the recommended pattern for a game you actually publish.

-----

## Full Documentation

For more in-depth information about the Enjin Platform and its features, please refer to the official documentation:

* [Setup Guide](https://docs.enjin.io/guides/platform/enjin-farmer-sample-game/setup-guide)
* [Implementation Breakdown](https://docs.enjin.io/guides/platform/enjin-farmer-sample-game/implementation-breakdown)
