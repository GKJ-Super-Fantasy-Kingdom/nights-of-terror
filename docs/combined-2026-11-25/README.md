# Nights of Terror combined playtest

November 25, 2026. Super Fantasy Kingdom 0.8.068 and BepInEx 5.4.23.5 on Windows. The private player loaded the exact plugin files from GKJ - Controller Test, including the published Hold the Line 0.2.3 DLL, plus Nights of Terror 0.1.0, Cursed Relics 0.1.0 and Cursed Equipment 0.1.0 extracted from their local ZIPs. The Hold the Line release ZIP and profile DLL matched. [Loaded versions and DLL hashes](loaded-runtime.txt)

All runs used a copied Human kingdom checkpoint for day 13 and a separate Unity identity, save directory and preferences. The test launcher held the shared `Local\SFKControllerRetest` mutex and closed each private player when its scenario ended.

## Controlled gameplay

| Input and scale | Setup | Result |
| --- | --- | --- |
| Virtual gamepad, 1× and 1.5× | The probe kept one original enemy alive until midnight. D-pad moved from Pass to Coin Wager, and South staked one coin. | Three reinforcements overlapped the surviving original. The arrivals were approximately 0.083 scaled seconds apart at 5× game speed. North opened the tavern after one combat end and one survival. Each successful wager paid two coins in one transaction with zero enemies alive and all three reinforcements dead. Both Continue runs restored the paid receipt and 12-coin balance without a second payment or wave. |
| Keyboard/mouse, 1× | The probe cleared the ordinary wave early at 3× game speed. The mouse clicked Coin Wager and operated the clock. | Early tavern entry was rejected. The three reinforcements arrived and died once, followed by one combat end and automatic tavern entry. Hold the Line paid two coins once after clearance. |
| Keyboard/mouse, 1.5× | The copied profile began with Auto Rush enabled. The ordinary wave cleared early at 5× game speed. The mouse clicked Coin Wager and operated the clock. | Enter opened the tavern after the three reinforcements died. There was one combat end and one survival. Hold the Line paid two coins once. The original wave retained its Rush flag, one point of bonus glory and its saved Auto Rush preference. |

The separate 1.5× loss fixture kept enemies alive until one hit the wall through native combat at sunset. HP fell from 30 to 29.61022. The probe then repaired the wall and controlled the remaining enemies. One original enemy and all three reinforcements overlapped at midnight. The tavern opened once after all four cleared. The stake stayed Lost with zero payouts, and a fresh Continue restored the Lost receipt and 10-coin balance. [Wall loss and midnight](../screenshots/combined-2026-11-25/wall-loss-midnight-gamepad-1.5x.png)

## Natural combat

A separate 1× run used native combat for the entire encounter. It ran at 2× game speed with no forced kills, repositioned enemies or scripted damage. An enemy attack through `UnitHealth.TakeHealthDamage` reduced the wall from 30 to 29.63065 HP during the original wave and moved the stake to Lost. The kingdom saw 119 original spawns and three reinforcements, then fell in evening overtime with zero payout. [Natural midnight](../screenshots/combined-2026-11-25/midnight-natural-1x.png)

## HUD evidence

The dusk and midnight alerts rendered in the native notification stack alongside the combined HUD. All files are fresh 1920 × 1080 game framebuffer captures.

| UI scale | Gamepad | Keyboard/mouse |
| --- | --- | --- |
| 1× | [Dusk](../screenshots/combined-2026-11-25/dusk-gamepad-1x.png) · [Midnight](../screenshots/combined-2026-11-25/midnight-gamepad-1x.png) | [Midnight](../screenshots/combined-2026-11-25/midnight-keyboard-mouse-1x.png) |
| 1.5× | [Dusk](../screenshots/combined-2026-11-25/dusk-gamepad-1.5x.png) · [Midnight](../screenshots/combined-2026-11-25/midnight-gamepad-1.5x.png) | [Midnight](../screenshots/combined-2026-11-25/midnight-keyboard-mouse-1.5x.png) |

## Package and coverage limits

The gameplay cases reproduced no Nights of Terror or partner-mod defect. The first probe dismissed the wager card during setup. The rerun targeted Hold the Line's active card and staked through native inputs. The 64 managed checks also passed against the private 0.8.068 assemblies and released Hold the Line 0.2.3. [Selected probe logs](scenario-results.txt)

The existing NightsOfTerror-0.1.0.zip was tested directly. Its SHA-256 is `5c486d9542c937f81d5023aeff90810dbf87e0dbb8c79545a1f8c8de7acc7931`. The DLL extracted from the ZIP matched the DLL loaded in each scenario.

Coverage used one Human kingdom and virtual input. A hand-played victory, a genuine first wall hit from a reinforcement, an outstanding reward chest, other races or nights, and the two Cursed mods' reward and equipment interactions remain outside this pass. The natural run reached midnight and ended in defeat. The real wall hit in the surviving test occurred at sunset and the repair was scripted.
