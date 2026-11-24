param(
    [string]$GameDir = 'D:\SteamLibrary\steamapps\common\Super Fantasy Kingdom',
    [string]$ProfileDir = "$PSScriptRoot/../.analysis/profile",
    [string]$Package = "$PSScriptRoot/../dist/NightsOfTerror-0.1.0.zip",
    [string]$WagerPackage,
    [string]$CopiedSave
)
# PowerShell 7. Execute the shipped scheduler and inspect its native integration points.
$ErrorActionPreference = 'Stop'
$core = (Resolve-Path "$ProfileDir/BepInEx/core").Path
$managed = Join-Path $GameDir 'Super Fantasy Kingdom_Data/Managed'
foreach ($name in 'UnityEngine.CoreModule','UnityEngine','Assembly-CSharp') {
    [void][Reflection.Assembly]::LoadFrom("$managed/$name.dll")
}
foreach ($name in 'Mono.Cecil','MonoMod.Utils','MonoMod.RuntimeDetour','0Harmony','BepInEx') {
    [void][Reflection.Assembly]::LoadFrom("$core/$name.dll")
}
function FromZip($path, $entry) {
    $zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path $path))
    try {
        $inputStream = $zip.GetEntry($entry).Open()
        $stream = [IO.MemoryStream]::new()
        try { $inputStream.CopyTo($stream); return ,$stream.ToArray() }
        finally { $stream.Dispose(); $inputStream.Dispose() }
    } finally { $zip.Dispose() }
}
function Check([bool]$ok, $name) {
    if (!$ok) { throw "Failed: $name" }
    "PASS $name"
}
$packedBytes = FromZip $Package 'plugins/NightsOfTerror.dll'
$mod = [Reflection.Assembly]::Load($packedBytes)
$flags = [Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$scheduler = $mod.GetType('NightsOfTerror.Reinforcement')
function Call($target, $method, [object[]]$arguments) {
    $target.GetType().GetMethod($method, $flags).Invoke($target, $arguments)
}
foreach ($rush in $true,$false) {
    $group = [Activator]::CreateInstance($scheduler,$true)
    Check ((Call $group ForClearance 0) -eq 1) "early clearance held until midnight (rush=$rush)"
    Check ((Call $group ForClearance 7) -eq 7) 'overlapping original group keeps its real count'
    foreach ($t in @(100,102,104)) {
        Check (Call $group IsDue ([single]$t)) 'fast-forward or delayed tick accepts the next arrival'
        Call $group Arrived @([single]$t,$rush)
        Check (!(Call $group IsDue ([single]$t))) 'repeated tick does not spawn another monster'
        if ($t -lt 104) {
            Check ((Call $group IsDue ([single]($t+0.08))) -eq $rush) 'group-local rushed spacing'
            Check ((Call $group ForClearance 0) -eq 1) 'instant kills between arrivals keep tavern closed'
        }
    }
    Check (!(Call $group IsDue ([single]1000))) 'overtime never creates a fourth reinforcement'
    Check ((Call $group ForClearance 2) -eq 2 -and (Call $group ForClearance 0) -eq 0) 'native living population alone controls final clearance'
}
$replay = [Activator]::CreateInstance($scheduler,$true)
Check ((Call $replay get_Remaining @()) -eq 3) 'fresh dawn schedule starts with all three arrivals'
Call $replay Cancel @()
Check ((Call $replay ForClearance 0) -eq 0 -and !(Call $replay IsDue ([single]999))) 'interrupted group releases its reservation'
$instance = [Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($mod.GetType('NightsOfTerror.Plugin'))
Check ((Call $instance BeginSettlement @()) -and !(Call $instance BeginSettlement @())) 'only first survival request can reach state observers'
Call $instance ResetNight @()
Check (Call $instance BeginSettlement @()) 'replayed dawn resets the settlement claim'

# Instantiate only managed fields used by these calls. A nonzero cached pointer
# marks an object as present for Unity's null test. No engine call dereferences it.
function GameObjectFixture([type]$type) {
    $obj = [Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($type)
    [UnityEngine.Object].GetField('m_CachedPtr',$flags).SetValue($obj,[IntPtr]1)
    return ,$obj
}
$clock = GameObjectFixture ([SuperFantasyKingdom.DaytimeManager])
$spawner = GameObjectFixture ([SuperFantasyKingdom.Spawner.MonsterSpawner])
$combat = GameObjectFixture ([SuperFantasyKingdom.CombatManager])
$enemy = GameObjectFixture ([SuperFantasyKingdom.Monster])
$wave = [SuperFantasyKingdom.Spawner.MonsterSpawner+Wave]::new()
$wave.monster = $enemy
$spawner.waves = @($wave)
[SuperFantasyKingdom.DaytimeManager]::Instance = $clock
[SuperFantasyKingdom.CombatManager]::Instance = $combat
[SuperFantasyKingdom.GameManager]::Instance = GameObjectFixture ([SuperFantasyKingdom.GameManager])
[MainManager]::Instance = GameObjectFixture ([MainManager])
$pluginType=$instance.GetType()
$pluginType.GetField('Active',$flags).SetValue($null,$instance)
$gates=$mod.GetType('NightsOfTerror.NightGates')
function Gate($name, [object[]]$argsForGate) { $gates.GetMethod($name,$flags).Invoke($null,$argsForGate) }
function Prefix($name,[object[]]$argsForGate) {
    $gates.GetNestedType($name,[Reflection.BindingFlags]::NonPublic).GetMethod('Prefix',$flags).Invoke($null,$argsForGate)
}
$dayField=$clock.GetType().GetField('m_CurrentDay',$flags)
Call $instance Dawn 1
foreach($day in 7,14,21,28,29) {
    $dayField.SetValue($clock,$day)
    Check (!(Call $instance Schedule @($clock,$spawner,$day))) "day $day excluded using native calendar"
}
$dayField.SetValue($clock,13)
Check (Call $instance Schedule @($clock,$spawner,13)) 'ordinary dusk schedules prepared native enemy'
Check (!(Call $instance Schedule @($clock,$spawner,13))) 'repeated dusk cannot announce or reserve twice'
Check ((Gate CountBeforeTavern 0) -eq 1 -and !(Prefix TavernEntry @())) 'empty early battlefield with pending group keeps both tavern paths closed'
Check (!(Prefix Survival ([SuperFantasyKingdom.GameState]::Survived))) 'premature survival cannot reach wager settlement'
Check (Prefix Survival ([SuperFantasyKingdom.GameState]::Lose)) 'real loss can interrupt the scheduled night'
$activeGroup=$pluginType.GetField('group',$flags).GetValue($instance)
$combat.GetType().GetField('m_Monsters',$flags).SetValue($combat,[Collections.Generic.List[SuperFantasyKingdom.MonsterBase]]::new())
$normal=GameObjectFixture ([SuperFantasyKingdom.Monster])
$combat.RegisterMonster($normal)
$enemies=@()
foreach ($t in 100,101,109) {
    $member=GameObjectFixture ([SuperFantasyKingdom.Monster])
    $combat.RegisterMonster($member)
    $enemies+=,$member
    Call $activeGroup Arrived @([single]$t,$true)
    Check ((Gate CountBeforeTavern ($combat.CurrentMonstersAmount())) -eq ($enemies.Count+1)) 'registered native enemy remains counted alongside the original opponent'
}
foreach($member in $enemies){ $combat.UnregisterMonster($member) }
Check ((Gate CountBeforeTavern ($combat.CurrentMonstersAmount())) -eq 1) 'original wave can finish last'
$combat.UnregisterMonster($normal)
Check ((Gate CountBeforeTavern ($combat.CurrentMonstersAmount())) -eq 0 -and (Gate CountBeforeNight 0) -eq 1) 'real zero permits native chest and provocation checks before auto-entry'
$combat.GetType().GetField('m_IsCombatOver',$flags).SetValue($combat,$true)
Check ((Gate CountBeforeNight 0) -eq 0 -and (Prefix TavernEntry @())) 'native combat completion releases evening and manual entry'
Check ((Prefix Survival ([SuperFantasyKingdom.GameState]::Survived)) -and !(Prefix Survival ([SuperFantasyKingdom.GameState]::Survived)) -and !(Prefix TavernEntry @())) 'manual and automatic survival share one claim'
Call $instance Dawn 13
Check ((Gate CountBeforeTavern 0) -eq 0 -and (Call $instance Schedule @($clock,$spawner,13))) 'replaying the same dawn can schedule a fresh group'
$pluginType.GetField('Active',$flags).SetValue($null,$null)

$resolver = [Mono.Cecil.DefaultAssemblyResolver]::new()
$resolver.AddSearchDirectory($managed)
$resolver.AddSearchDirectory($core)
$reader = [Mono.Cecil.ReaderParameters]::new()
$reader.AssemblyResolver = $resolver
$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly("$managed/Assembly-CSharp.dll",$reader)
$stream = [IO.MemoryStream]::new($packedBytes)
$pack = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($stream,$reader)
function Ops($type, $method) {
    ($game.MainModule.GetType("SuperFantasyKingdom.$type").Methods | Where-Object Name -eq $method).Body.Instructions
}
try {
    $refs = @($pack.MainModule.GetMemberReferences() | Where-Object { $_.DeclaringType.Scope.Name -match '^(Unity|Assembly-CSharp|0Harmony|BepInEx)' })
    foreach ($ref in $refs) { if ($null -eq $ref.Resolve()) { throw "Unresolved $ref" } }
    Check ($refs.Count -gt 0) "$($refs.Count) references resolve against installed assemblies"
    $nativeSpawn = @(Ops 'Spawner.MonsterSpawner' SpawnMonster).Operand.Name
    Check ($nativeSpawn -contains 'GetMonster' -and $nativeSpawn -contains 'Init' -and $nativeSpawn -contains 'MonsterSpawn') 'native spawn uses the pool, initialization and spawn event'
    Check ((Ops MonsterBase Init).Operand.Name -contains 'RegisterMonster' -and (Ops MonsterBase OnDeath).Operand.Name -contains 'UnregisterMonster') 'native enemy lifetime enters and leaves combat population'
    foreach ($m in 'Update','SwitchState') {
        $ops = @(Ops DaytimeManager $m)
        Check (@($ops | Where-Object { $_.Operand.Name -eq 'CurrentMonstersAmount' }).Count -eq 1) "$m has the single expected native clearance site"
    }
    $gates = $mod.GetType('NightsOfTerror.NightGates')
    $ops = [Collections.Generic.List[HarmonyLib.CodeInstruction]]::new()
    $native = [SuperFantasyKingdom.CombatManager].GetMethod('CurrentMonstersAmount')
    $ops.Add([HarmonyLib.CodeInstruction]::new([Reflection.Emit.OpCodes]::Callvirt,$native))
    foreach ($pair in 'PhaseAdvance/CountBeforeNight','TavernPrompt/CountBeforeTavern') {
        $class,$gate = $pair.Split('/')
        $patch = $gates.GetNestedType($class,[Reflection.BindingFlags]::NonPublic).GetMethod('Transpiler',$flags)
        $result = $patch.Invoke($null,(,$ops))
        Check ($result.Count -eq 2 -and $result[0].operand -eq $native -and $result[1].operand.Name -eq $gate) "shipped $class transpiler preserves the native count before its gate"
    }
    $update = @(Ops DaytimeManager Update).Operand.Name
    Check ($update -contains 'HasChestToCollect' -and $update -contains 'IsProvoked' -and $update -contains 'CombatEnd') 'native tavern clearance retains chest, provocation and combat-end checks'
    Check ((Ops DaytimeManager SwitchState).Operand.Name -contains 'ChangeState' -and (Ops GameManager EndDay).Operand.Name -contains 'ChangeState') 'automatic and manual settlement share the guarded state transition'
    foreach ($route in 'Spawner.MonsterSpawner/InitDay','UI.UIRushWave/OnMorningStart') {
        $class,$method = $route.Split('/')
        $body = @(Ops $class $method)
        $unlock = $body | Where-Object { $_.Operand.Name -eq 'GetMaxDay' -and $_.Next.Operand -eq 21 }
        $comparison = if ($class -eq 'UI.UIRushWave') { '^bge' } else { '^blt' }
        Check ($null -ne $unlock -and $unlock.Next.Next.OpCode.Name -match $comparison) "$route retains the native day-21 Rush unlock"
    }
    $names = $pack.MainModule.GetMemberReferences().Name
    Check (!($names | Where-Object { $_ -in 'Rush','SetInt','SetMonsters','ReleaseTheHorde','SetMonstersToSpawn','AddHeroExperience','AddToCoins','RegisterMonster','UnregisterMonster' })) 'plugin has no global Rush, normal quota, reward or registry mutations'
    if ($WagerPackage -and $CopiedSave) {
        $save = Get-Content -Raw -LiteralPath $CopiedSave | ConvertFrom-Json
        $wagerDll = [Reflection.Assembly]::Load((FromZip $WagerPackage 'plugins/HoldTheLine.dll'))
        $wagerType = $wagerDll.GetType('HoldTheLine.Wager')
        foreach ($damaged in $true,$false) {
            $wager = [Activator]::CreateInstance($wagerType,$true)
            $wager.Run=$save.runID; $wager.Day=$save.day+1; $wager.State='Staked'
            $ledger=[Collections.Generic.List[int]]::new()
            $credit=[Action[int]]{param($n) $ledger.Add($n)}
            if ($damaged) { Call $wager Hit @([single]30,[single]29); Call $wager Hit @([single]29,[single]30) }
            $paid=Call $wager Finish @($true,$credit)
            $duplicate=Call $wager Finish @($true,$credit)
            Check (($paid -eq !$damaged) -and !$duplicate -and $ledger.Count -eq [int](!$damaged)) "Hold the Line on copied night $($wager.Day), repaired hit=$damaged and exactly $($ledger.Count) settlement payments"
        }
    }
} finally {
    $pack.Dispose(); $stream.Dispose(); $game.Dispose(); $resolver.Dispose()
}
