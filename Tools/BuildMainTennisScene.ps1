$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)
function Meta($path) {
    if (Test-Path "$path.meta") { return ([regex]::Match((Get-Content "$path.meta" -Raw), 'guid: (\w+)')).Groups[1].Value }
    $guid = [guid]::NewGuid().ToString('N')
    Set-Content "$path.meta" "fileFormatVersion: 2`nguid: $guid"
    return $guid
}
New-Item -ItemType Directory -Force Assets/04_Materials/Tennis | Out-Null
Get-ChildItem Assets/02_Scripts/Tennis -Filter *.cs -Recurse | ForEach-Object { $null = Meta $_.FullName }
$materials = @{}
$colors = @{ Court='0.08, g: 0.32, b: 0.48'; Surround='0.045, g: 0.13, b: 0.19'; White='0.9, g: 0.96, b: 1'; Player='0.05, g: 0.75, b: 0.95'; Enemy='1, g: 0.3, b: 0.15'; Ball='0.8, g: 1, b: 0.05'; Net='0.12, g: 0.2, b: 0.25' }
foreach ($name in $colors.Keys) {
    $mat = Get-Content Assets/04_Materials/Ground_Blue.mat -Raw
    $mat = $mat.Replace('m_Name: Ground_Blue', "m_Name: Tennis_$name")
    $mat = [regex]::Replace($mat, '(_BaseColor|_Color): \{[^}]+\}', ('$1: {r: ' + $colors[$name] + ', a: 1}'))
    $path = "Assets/04_Materials/Tennis/$name.mat"
    Set-Content $path $mat
    $materials[$name] = Meta $path
}
$source = (Get-Content Assets/01_Scenes/MainScene.unity -Raw).Replace("`r`n", "`n")
function Template($id) { return [regex]::Match($source, "(?ms)^--- !u!\d+ &$id\n.*?(?=^--- !u!|\z)").Value.TrimEnd() }
$script:objects = [System.Collections.Generic.List[object]]::new()
$script:nextId = 10000
function Obj($name, $parent=0, $position='0, y: 0, z: 0', $scale='1, y: 1, z: 1', $mesh=0, $material='White') {
    $id = $script:nextId
    $script:nextId += 10
    $o = @{ id=$id; name=$name; parent=$parent; position=$position; scale=$scale; mesh=$mesh; material=$material; rotation='0, y: 0, z: 0, w: 1'; components=[System.Collections.Generic.List[object]]::new(); tag='Untagged' }
    $script:objects.Add($o)
    return $o
}
function Script($o, $class, $fields) {
    $guid = Meta "Assets/02_Scripts/Tennis/$class.cs"
    $id = $o.id + 4 + $o.components.Count
    $o.components.Add(@{ id=$id; text=@"
--- !u!114 &$id
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $($o.id)}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $guid, type: 3}
  m_Name:
  m_EditorClassIdentifier: Assembly-CSharp::TennisGame.$class
$fields
"@ })
    return $id
}
$court = Obj 'Court'
$courtId = Script $court TennisCourt "  halfWidth: 4.115`n  halfLength: 11.885`n  serviceLength: 6.4`n  netHeight: 0.914`n  runOff: 3"
$cv = Obj 'Visual' ($court.id+1)
$null = Obj 'Ground' ($cv.id+1) '0, y: -0.17, z: 0' '21, y: 0.3, z: 34' 10202 Surround
$null = Obj 'CourtSurface' ($cv.id+1) '0, y: -0.045, z: 0' '10.97, y: 0.08, z: 23.77' 10202 Court
$lines = Obj 'CourtLines' ($cv.id+1)
foreach ($x in @(-5.485,-4.115,4.115,5.485)) { $null = Obj "Sideline_$x" ($lines.id+1) "$x, y: 0.012, z: 0" '0.05, y: 0.02, z: 23.82' 10202 }
foreach ($z in @(-11.885,11.885)) { $null = Obj "Baseline_$z" ($lines.id+1) "0, y: 0.012, z: $z" '11.02, y: 0.02, z: 0.05' 10202 }
foreach ($z in @(-6.4,6.4)) { $null = Obj "ServiceLine_$z" ($lines.id+1) "0, y: 0.012, z: $z" '8.28, y: 0.02, z: 0.05' 10202 }
$null = Obj 'CenterServiceLine' ($lines.id+1) '0, y: 0.012, z: 0' '0.05, y: 0.02, z: 12.8' 10202
$net = Obj 'Net' ($cv.id+1)
$null = Obj 'TopTape' ($net.id+1) '0, y: 0.914, z: 0' '11.2, y: 0.055, z: 0.06' 10202
foreach ($x in @(-5.6,5.6)) { $null = Obj "Post_$x" ($net.id+1) "$x, y: 0.5, z: 0" '0.12, y: 1, z: 0.12' 10202 Net }
foreach ($y in @(.15,.3,.45,.6,.75)) { $null = Obj "MeshRow_$y" ($net.id+1) "0, y: $y, z: 0" '11.2, y: 0.015, z: 0.015' 10202 Net }
for ($i=-22; $i -le 22; $i++) { $null = Obj "MeshStrand_$i" ($net.id+1) "$($i*.25), y: 0.46, z: 0" '0.015, y: 0.86, z: 0.015' 10202 Net }
$player = Obj 'Player' 0 '2.1, y: 0, z: -12.485'
$pv = Obj 'Visual' ($player.id+1)
$null = Obj 'Body_ReplaceWithAsset' ($pv.id+1) '0, y: 1, z: 0' '0.75, y: 1, z: 0.75' 10208 Player
$null = Obj 'Racket' ($pv.id+1) '0.9, y: 0.9, z: 0.2' '0.5, y: 0.7, z: 0.08' 10207 White
$playerId = Script $player TennisActor "  visual: {fileID: $($pv.id+1)}`n  side: 0"
$enemy = Obj 'Enemy' 0 '-1.05, y: 0, z: 10.885'
$ev = Obj 'Visual' ($enemy.id+1)
$null = Obj 'Body_ReplaceWithAsset' ($ev.id+1) '0, y: 1, z: 0' '0.75, y: 1, z: 0.75' 10208 Enemy
$null = Obj 'Racket' ($ev.id+1) '-0.9, y: 0.9, z: -0.2' '0.5, y: 0.7, z: 0.08' 10207 White
$enemyId = Script $enemy TennisActor "  visual: {fileID: $($ev.id+1)}`n  side: 1"
$ball = Obj 'Ball' 0 '2.9, y: 1.8, z: -12.485'
$bv = Obj 'Visual' ($ball.id+1)
$null = Obj 'Sphere_ReplaceWithAsset' ($bv.id+1) '0, y: 0, z: 0' '0.24, y: 0.24, z: 0.24' 10207 Ball
$ballId = Script $ball TennisBall "  visual: {fileID: $($bv.id+1)}"
$markers = Obj 'CourtIndicators'
$landing = Obj 'LandingMarker' ($markers.id+1) '0, y: 0.035, z: 4' '0.5, y: 0.025, z: 0.5' 10207 Ball
$aim = Obj 'AimMarker' ($markers.id+1) '0, y: 0.035, z: 8' '0.5, y: 0.025, z: 0.5' 10207 Player
$camera = Obj 'MainCamera' 0 '0, y: 22, z: -27'
$camera.rotation = '0.3420201, y: 0, z: 0, w: 0.9396927'
$camera.tag = 'MainCamera'
$null = Script $camera TennisCamera "  player: {fileID: $($player.id+1)}`n  court: {fileID: $($court.id+1)}`n  follow: 0.12"
foreach ($pair in @(@(9934658,6),@(9934657,7),@(9934660,8))) {
    $id = $camera.id + $pair[1]
    $t = Template $pair[0]
    $t = $t.Replace("&$($pair[0])", "&$id").Replace('m_GameObject: {fileID: 9934656}', "m_GameObject: {fileID: $($camera.id)}")
    if ($pair[0] -eq 9934658) { $t = $t.Replace('field of view: 60','field of view: 48') }
    $camera.components.Add(@{id=$id;text=$t})
}
$light = Obj 'Directional Light' 0 '0, y: 6, z: 0'
$light.rotation = '0.40821788, y: -0.23456968, z: 0.10938163, w: 0.8754261'
$light.components.Add(@{id=($light.id+4);text=(Template 2143152531).Replace('&2143152531',"&$($light.id+4)").Replace('m_GameObject: {fileID: 2143152530}',"m_GameObject: {fileID: $($light.id)}")})
$balancePath = 'Assets/MainTennisBalance.asset'
$balanceGuid = Meta $balancePath
$settingsGuid = Meta Assets/02_Scripts/Tennis/TennisSettings.cs
Set-Content $balancePath @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $settingsGuid, type: 3}
  m_Name: MainTennisBalance
  m_EditorClassIdentifier: Assembly-CSharp::TennisGame.TennisSettings
  playerSpeed: 7.5
  enemySpeed: 6.3
  chargeMoveMultiplier: 0.6
  chargeSeconds: 0.85
  stamina: 100
  chargeDrain: 14
  sprintDrain: 24
  staminaRecovery: 20
  shotSpeed: 15
  serveSpeed: 18
  shotArc: 2.2
  chargedPower: 1.3
  hitReach: 2.1
  maxHitHeight: 3.2
  swingBuffer: 0.16
  bounceSpeed: 0.65
  bounceArc: 1.25
  bounceDistance: 4.2
  reactionSeconds: 0.25
  aimError: 0.45
  gamesPerSet: 3
  setsToWin: 1
  pointDelay: 1.6
"@
$match = Obj 'GameManager'
$null = Script $match TennisMatch "  balance: {fileID: 11400000, guid: $balanceGuid, type: 2}`n  court: {fileID: $courtId}`n  player: {fileID: $playerId}`n  enemy: {fileID: $enemyId}`n  ball: {fileID: $ballId}`n  landingMarker: {fileID: $($landing.id+1)}`n  aimMarker: {fileID: $($aim.id+1)}"
$output = [System.Text.StringBuilder]::new()
$null = $output.Append($source.Substring(0,$source.IndexOf('--- !u!1 &')))
foreach ($o in $objects) {
    $components = "  - component: {fileID: $($o.id+1)}"
    if ($o.mesh) { $components += "`n  - component: {fileID: $($o.id+2)}`n  - component: {fileID: $($o.id+3)}" }
    foreach ($c in $o.components) { $components += "`n  - component: {fileID: $($c.id)}" }
    $children = @($objects | Where-Object parent -eq ($o.id+1))
    $childText = '  m_Children: []'
    if ($children.Count) { $childText = "  m_Children:`n" + (($children | ForEach-Object { "  - {fileID: $($_.id+1)}" }) -join "`n") }
    $null = $output.AppendLine(@"
--- !u!1 &$($o.id)
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
$components
  m_Layer: 0
  m_Name: $($o.name)
  m_TagString: $($o.tag)
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$($o.id+1)
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $($o.id)}
  serializedVersion: 2
  m_LocalRotation: {x: $($o.rotation)}
  m_LocalPosition: {x: $($o.position)}
  m_LocalScale: {x: $($o.scale)}
  m_ConstrainProportionsScale: 0
$childText
  m_Father: {fileID: $($o.parent)}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"@)
    if ($o.mesh) {
        $renderer = (Template 993595832).Replace('&993595832',"&$($o.id+2)").Replace('m_GameObject: {fileID: 993595828}',"m_GameObject: {fileID: $($o.id)}").Replace('m_Enabled: 0','m_Enabled: 1').Replace('31321ba15b8f8eb4c954353edc038b1d',$materials[$o.material])
        $filter = (Template 993595833).Replace('&993595833',"&$($o.id+3)").Replace('m_GameObject: {fileID: 993595828}',"m_GameObject: {fileID: $($o.id)}").Replace('fileID: 10208',"fileID: $($o.mesh)")
        $null = $output.AppendLine($renderer).AppendLine($filter)
    }
    foreach ($c in $o.components) { $null = $output.AppendLine($c.text) }
}
$null = $output.AppendLine("--- !u!1660057539 &9223372036854775807`nSceneRoots:`n  m_ObjectHideFlags: 0`n  m_Roots:")
foreach ($o in $objects | Where-Object parent -eq 0) { $null = $output.AppendLine("  - {fileID: $($o.id+1)}") }
$scenePath = 'Assets/01_Scenes/MainTennis.unity'
Set-Content $scenePath $output.ToString()
$sceneGuid = Meta $scenePath
$build = Get-Content ProjectSettings/EditorBuildSettings.asset -Raw
$build = [regex]::Replace($build, '(?s)  m_Scenes:.*?  m_configObjects:', "  m_Scenes:`n  - enabled: 1`n    path: $scenePath`n    guid: $sceneGuid`n  m_configObjects:")
Set-Content ProjectSettings/EditorBuildSettings.asset $build
Write-Output "Created MainTennis with $($objects.Count) editable scene objects."
