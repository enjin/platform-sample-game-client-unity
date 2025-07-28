# Introduction

This repository contains the Unity client for the Enjin Platform sample game. It's a simple farming game designed to demonstrate how to integrate Enjin's NFT technology into a Unity project. 🧑‍🌾

This client communicates with a [Node.js game server](https://github.com/enjin/platform-sample-game-server) to handle all blockchain-related actions like minting, melting, reading players' wallets, and transferring NFTs.

-----

## Quick Start Guide

This guide will get you up and running quickly.

### **Prerequisites**

  - ✅ Unity Hub with Unity Editor version `6000.0.24f1`
  - ✅ Git

-----

### **Step 1: Set Up the Game Server**

⚠️ **Important:** Before you can run this Unity client, you **must set up and run the game server first**.

Please follow the setup instructions in the [game server repository](https://github.com/enjin/platform-sample-game-server).

After running the server for the first time, it will generate and log a **Collection ID** in the terminal. **Copy this ID** – you'll need it in a moment.

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

-----

### **Step 4: Configure the Project**

1.  In the Unity Editor's `Project` window, navigate to `Assets/Enjin Integration/Scripts/Data/Items`.
2.  You will see three `Enjin Item` assets: `GemGreen`, `GoldCoin`, and `GoldCoinBlue`.
3.  Click on **each one** of these items. In the `Inspector` window, find the **Collection Id** field and paste the `Collection ID` you copied from the game server.

> **Note:** If you are running the game server on a different machine or port, you'll also need to update the server URL. Select the `EnjinManager` prefab in `Assets/Enjin Integration/Prefabs/` and change the `Host` property in the `Enjin API Service` component.

-----

### **Step 5: Run the Game & How to Play 🎮**

Make sure the game server is still running in the background.
1.  In the Unity Editor's `Project` window, navigate to `Assets/HappyHarvest/Scenes`.
2.	Double click on the `Farm_Outdoor` scene.
3.	Press the **Play** button at the top of the Unity Editor to start the game.

Once the game is running:

1.  **Login:** Click the **Menu** button (top-right), then **Login**. Enter any email and password to register a new player. This will automatically create a managed wallet for you.
2.  **Move:** Use the **W, A, S, D** keys to move your character.
3.  **Harvest:** Walk up to a crop and click on it to harvest it.
4.  **Collect NFTs:** Keep harvesting until a resource item (like a gem or coin) appears. Click on the item to collect it. This action will mint the item as an NFT to your player's wallet.
5.  **View Your NFTs:** Click on the **Backpack** button (top-right) to see your collected NFTs. From here, you can **Melt** them or **Send** them to another wallet.

-----

## Full Documentation

For a complete, step-by-step walkthrough that covers setting up the Enjin Platform, the Wallet Daemon, the game server, and this client, please refer to our **[in-depth guide](https://docs.enjin.io)**.