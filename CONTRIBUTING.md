# Contributing to Nights of Terror

## Report a bug

Include the game, mod and BepInEx versions, plus your kingdom, day and reproduction steps. For a display or controls issue, include the UI scale and input scheme. Share the relevant log excerpt with local user paths removed.

## Build and test

Use the .NET SDK, Python with Pillow, the managed assemblies from your game installation and a BepInEx 5 profile. Set the `GameDir` and `ProfileDir` environment variables to the game and profile directories. Run `python package.py` to build the local ZIP in `dist`.

Run `tests/VerifyRuntime.ps1` in PowerShell 7 against that package and the installed assemblies. For live testing, use a copied save. Keep game binaries, save files, personal logs and local build output out of commits.

## Submit a change

Fork the repository, make the change on a branch and describe the player-visible effect and the tests you ran. Use Conventional Commits for commit messages and pull request titles.
