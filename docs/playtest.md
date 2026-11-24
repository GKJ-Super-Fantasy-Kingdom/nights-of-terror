# Nights of Terror 0.1.0 playtest

Tested the DLL extracted from the local-import ZIP on Super Fantasy Kingdom 0.8.068 for Windows. Each night started from a copy of the same human kingdom dawn checkpoint, on day 13. The test player used its own Unity identity, preferences and save directory. Sessions used the shared `Local\SFKControllerRetest` mutex.

## Gameplay results

| Scenario | Observed result |
| --- | --- |
| Early clearance, standalone, default rushed arrivals | At 5× test speed, no original enemies remained at midnight. Early tavern entry was rejected. The game registered 119 ordinary spawns, three reinforcements, one combat end and one survival into the tavern. |
| Keyboard/mouse at 1×, slower arrival setting | At 3× game speed, the three enemies arrived about 1.049 scaled seconds apart. Ordinary opponents were cleared by the test driver. The reinforcements fought and died through native combat. The game emitted three reinforcement kill-reward events and automatically entered the tavern once. A mouse click on the native clock registered while the dusk warning was visible. |
| Keyboard/mouse at 1.5×, Auto Rush enabled | The ordinary wave spawned 119 enemies and cleared early. All three reinforcements spawned and emitted native kill-reward events. The mouse operated the clock and Enter opened the tavern after clearance. The normal wave retained its Rush flag, one point of Rush glory and its saved Auto Rush preference. |
| Gamepad at 1×, clean wall wager | One ordinary enemy remained when the group arrived. All four were counted together. The reinforcement intervals measured 0.083 scaled seconds at 5× test speed. The North button entered the tavern after clearance. Hold the Line paid three coins once. A fresh Continue restored the paid receipt and its 13-coin balance without another spawn, payout or settlement. |
| Gamepad at 1.5×, damaged wall wager | The wager remained staked at midnight while both groups overlapped. A one-point native wall hit forfeited the stake. Immediate repair preserved the loss. The North button entered the tavern after clearance. Continue restored the lost receipt and its 10-coin balance with zero extra spawns or settlements. |

### Screenshots from the running game

Each image is a 1920 × 1080 framebuffer capture on a private Windows desktop.

- Keyboard/mouse at 1×. [Dusk](screenshots/0.1.0/dusk-keyboard-1x.png) · [Midnight](screenshots/0.1.0/midnight-keyboard-1x.png)
- Keyboard/mouse at 1.5×. [Midnight with Auto Rush enabled](screenshots/0.1.0/midnight-keyboard-1.5x.png)
- Gamepad at 1.5×. [Dusk](screenshots/0.1.0/dusk-gamepad-1.5x.png) · [Midnight with the wall-damage notification](screenshots/0.1.0/midnight-gamepad-1.5x.png)

## Native integration

The game has no named midnight event. The mod schedules at dusk and uses the first evening-expiry tick to dispatch the group. That tick also runs when enemies hold the clock in overtime. For each arrival, the mod passes the prepared daily prefab and difficulty to the game's pooled spawn routine. The game initializes and registers the enemy, sends its spawn event, and removes it on death.

The two day-clock clearance decisions wait for the remaining arrivals. Combat lists, kill rewards, chests and provocation continue through their native code. After clearance, the manual and automatic tavern routes share one survival guard.

## Regression coverage and limits

The packaged DLL passed 64 PowerShell assertions against the installed assemblies and Hold the Line 0.1.5. These covered both arrival cadences, immediate kills between arrivals, skipped ticks, duplicate dusk and survival calls, cancelled arrivals, replayed dawn, and exclusion of days 7, 14, 21, 28 and 29. The checks also exercised both shipped Harmony transforms, verified the native day-21 Rush unlock in the spawner and HUD, and resolved all 50 runtime member references. [Regression script](../tests/VerifyRuntime.ps1)

The input devices were virtual. The probe used native health calls to clear ordinary opponents, held one opponent back for overlap, and prepared the stake through Hold the Line's wager code. The controlled wall hit and repair tested settlement timing. Full-night balance, other races, other game versions and a live run with an outstanding chest remain unverified. Boss exclusion was checked against the installed calendar and scheduling code.
