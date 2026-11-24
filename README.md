# Nights of Terror

The midnight group is 20% of the ordinary wave's original spawn quota, rounded up to a minimum of 10 and a maximum of 30. An ordinary night with a zero quota has no reinforcement. Midnight begins when the evening timer first expires, even if the original wave is still fighting. The dusk warning shows the count. Each reinforcement comes from the same enemy pool selected by dusk, using native tier difficulty and higher odds for recent tiers.

The default spacing is 0.075 game seconds between arrivals, the same as native Rush. Set `RushedArrival = false` in `BepInEx/config/gkj.nightsofterror.cfg` for one-second spacing. Player Rush controls, unlocks, Auto Rush and Rush glory follow the base game.

Clear both groups and collect any required chest to open the tavern. The native tavern controls and automatic entry setting apply. Dusk and midnight use the game's alerts with either input scheme. Each monster grants its ordinary kill rewards. The game settles the night once. Hold the Line can keep a wager active across both groups.

Boss nights, day 29, and the final day of a shortened challenge use the original encounter. A replayed dawn schedules its own encounter. The mod writes no save data.

## Local installation

Import `NightsOfTerror-0.1.2.zip` into a Super Fantasy Kingdom mod-manager profile, or extract `plugins/NightsOfTerror.dll` into the profile's `BepInEx/plugins` folder. Install the manifest's BepInEx dependencies for manual use. Remove the DLL to uninstall.

## Build

Requires the .NET SDK, Python with Pillow, the installed game's managed assemblies and a BepInEx 5 profile. Set the `GameDir` and `ProfileDir` environment variables to those locations. Run `python package.py` to build and validate the local-import ZIP in `dist`. Use a copied save for playtesting.
