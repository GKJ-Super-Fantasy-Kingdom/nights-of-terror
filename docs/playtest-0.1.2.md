# Nights of Terror 0.1.2 playtest

November 25, 2026. Tested the packaged DLL in a foreground window on Super Fantasy Kingdom 0.8.068, with a copied Human day-13 save and the combined Controller Test profile. Gameplay ran at normal speed with a 1.5× interface and brief pauses for the alerts. The driver controlled enemy clearance to isolate spawning and settlement.

- At dusk, the game reported a quota of 119 ordinary enemies and 15 remaining to spawn. The mod used the original quota to calculate 24 reinforcements. The warning displayed 24.
- The game spawned 119 ordinary enemies and exactly 24 reinforcements. Every reinforcement matched a prefab and its native difficulty in the 12-tier dusk pool. This draw produced 13 Shrooms, four Shamans, three Liches, two Efreets, one Satyr and one Psion.
- After both groups cleared, one combat end opened the tavern. Hold the Line 0.2.3 returned two coins once. The payout log recorded zero living enemies and 24 reinforcement deaths.
- The packaged DLL passed 266 managed checks including quota rounding, size limits, tier selection and settlement. The private player exited with code 0 and released the shared playtest lock.

The [dusk warning](screenshots/0.1.2/dusk-1.5x.png) and [midnight arrival in progress](screenshots/0.1.2/midnight-1.5x.png) were captured in the running game. Full-night balance and the late-game 30-enemy cap in live gameplay remain unverified.
