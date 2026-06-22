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

The project pulls in the [Enjin Platform Unity SDK](https://github.com/enjin/platform-unity-sdk) as a UPM package over git. `Packages/manifest.json` references it as:

```
"io.enjin.platform-sdk": "https://github.com/enjin/platform-unity-sdk.git#v3.0.2"
```

Unity will fetch and cache the package on first project open.

> **Note: the sample game itself does not use the Enjin Platform SDK.** All blockchain actions (mint, melt, transfer, wallet reads) go through the [C# game server](https://github.com/enjin/platform-sample-game-server)'s REST API — the client's `EnjinApiService` simply makes HTTP calls. The SDK lives on the **server**, where the API key is kept safe and away from the client.
>
> The SDK UPM package above is pulled in only for the optional `EnjinSdkSmoke` harness ([see below](#optional-smoke-test-harnesses)), which exercises the SDK directly for verification. If your goal is to learn how to integrate Enjin into a game, the pattern to study here is **game client → your server → Enjin Platform SDK**, not calling the SDK from the client. (Curious what the UPM package is for and why this game doesn't use it? See [**What is the UPM package for, and when should you use it?**](#what-is-the-upm-package-for-and-when-should-you-use-it) at the end.)

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

## Optional: Smoke Test Harnesses

Two standalone test scenes are included for verifying the integration without playing the full game:

- **`Assets/GameServerSmoke/`** — exercises the client's `EnjinApiService` end-to-end against a running game server (register/login, mint, melt, transfer, balance queries). Open via **Enjin → Open Game Server Smoke Scene** and press Play.
- **`Assets/EnjinSdkSmoke/`** — calls the Enjin Platform directly through the [Unity SDK](https://github.com/enjin/platform-unity-sdk) UPM package, against the canary GraphQL endpoint and bypassing the game server. Useful for confirming the UPM package works end-to-end inside Unity, or telling whether a problem is in the package/Platform or in your server.

See the `README.md` inside each folder for details.

-----

## Editor Menu Items Reference

| Menu Path | When to Use |
|---|---|
| **Enjin → Stamp Collection ID onto EnjinItem Assets** | **Required** before first play, and any time you change which server you connect to. |
| **Enjin → Open Game Server Smoke Scene** | Optional. Build/open the standalone scene that tests the client against the game server. |

-----

## What is the UPM package for, and when should you use it?

The Enjin UPM package (`io.enjin.platform-sdk`, added in [Step 3](#step-3-open-the-project-in-unity)) lets your Unity code talk to the Enjin Platform **directly**. Add it through the Package Manager and you can run Platform queries and mutations — mint, melt, transfer, wallet and balance reads — straight from C# in your game, with no backend of your own.

### Then why doesn't this sample game use it?

**Security.** Every Platform call is authenticated with your Platform API token. If your Unity game calls the Platform directly, that token has to be embedded in the build you hand to players — and a token shipped inside a distributed app can be extracted and used to act as your account (mint, transfer, drain funds). 

To avoid that, this sample keeps the token on a small [game server](https://github.com/enjin/platform-sample-game-server) it controls. The Unity client only ever calls that server, and the server makes the Platform calls on its behalf. The token never leaves your infrastructure. That client→server split is the recommended pattern for a game you actually publish.

### When to use the UPM package vs. a server

Use the **UPM package directly** when your token won't end up in untrusted hands:

- **Prototyping and learning** — wire up Platform calls from the Editor to see how things work before building any backend. (The optional `EnjinSdkSmoke` scene in this project does exactly this.)
- **Internal or trusted tools** — an in-house Unity Editor utility or admin tool that never leaves your team.
- **Hackathons and demos** — where shipping to untrusted users isn't a concern.

Use a **server in between** (what this sample demonstrates) for anything you distribute to players:

- The API token stays on your server and never ships in the build.
- The server can enforce its own rules — authentication, validation, rate limiting, anti-cheat — before it ever touches the Platform.
- The client only knows about *your* API, never your Platform credentials.

**Rule of thumb:** if players will run your build, keep the token behind a server. The UPM package is the quick path for prototypes and trusted tools; the server pattern is the safe path for shipping games.

-----

## Full Documentation

For more in-depth information about the Enjin Platform and its features, please refer to the official documentation:

* [Setup Guide](https://docs.enjin.io/guides/platform/enjin-farmer-sample-game/setup-guide)
* [Implementation Breakdown](https://docs.enjin.io/guides/platform/enjin-farmer-sample-game/implementation-breakdown)
