# Game Server Smoke

A scripted end-to-end exercise of the **Unity client &rarr; sample C# game server**
REST flow, intended as a wire-format / `JsonUtility` sanity check before you
click through the actual game UI.

It calls the same `EnjinApiService` singleton that the production menus call:

1. `GET  /api/auth/health-check`
2. `POST /api/auth/register`  (doubles as login)
3. `GET  /api/wallet/get-tokens`  (baseline)
4. `POST /api/token/mint`         &rarr; get-tokens
5. `POST /api/token/melt`         &rarr; get-tokens
6. `POST /api/token/transfer`     &rarr; get-tokens

All steps log to the Console and (optionally) to a UGUI `Text` element.

## Usage

1. Start the sample game server on `http://localhost:3000`
   (`dotnet run --project ../platform-sample-game-server`).
2. In Unity: **Enjin &rarr; Open Game Server Smoke Scene**.
   This creates `Assets/GameServerSmoke/GameServerSmoke.unity` and a fresh
   `GameServerSmokeConfig.asset` if neither exists.
3. Select the config asset in the Project window. Fill in:
   - `serverHost` (default `http://localhost:3000`)
   - `email` and `password` &mdash; either a brand-new pair (creates a player
     + managed wallet, gets dripped 1 ENJ) or an existing pair.
   - token ids / amounts &mdash; defaults assume Gold Coin (`1`) and a 5-mint /
     2-melt / 1-transfer sequence to the canary daemon wallet.
4. Press **Play**. Watch the Console for `[GameServerSmoke]` lines.

The runner aborts on the first failure and prints which step broke. If
everything passes you should see the managed wallet's Gold Coin balance go
`0 (or prior) &rarr; +5 &rarr; -2 &rarr; -1` across the four `get-tokens` dumps.

## What's gitignored

The generated scene file, the populated config asset, and their `.meta`
files are gitignored (see `.gitignore`). The config can hold a password and
the scene references a config GUID that's unique to your machine.

## Scope

`GameServerSmoke` exercises the **REST contract between the Unity client and
our own C# sample server**. The server is what talks to the platform; the
Unity client does not import any platform SDK.
