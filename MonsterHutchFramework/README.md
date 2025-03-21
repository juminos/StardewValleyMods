﻿# Monster Hutch Framework

Monster Hutch Framework is a framework that allows monsters to be raised and produce in a new 'Monster Hutch' building using a new 'Monster Incubator' machine, along with custom 'charmer' rings.
- new building 'Monster Hutch' can accommodate up to 40 monsters
- new craftable 'Monster Incubator' produces custom monsters based on input
- custom monsters that are 'watered' can produce items overnight
- new 'charmer' rings can be defined to prevent damage from (and to) monsters

## Table of Contents
- [Monster Hutch Framework](#monster-hutch-framework)
	- [Table of Contents](#table-of-contents)
    - [Features](#features)
        - [Monster Hutch](#monster-hutch)
        - [Monster Incubator](#monster-incubator)
        - [Monsters](#monsters)
        - [Charmer Rings](#charmer-rings)
	- [Configuration Options](#configuration-options)
	- [Modder Guide](#modder-guide)
		- [Monster Hutch Data](#monster-hutch-data)
			- [Monster Example](#monster-example)
		- [Charmer Ring Data](#charmer-ring-data)
			- [Ring Example](#ring-example)

## Features

### Monster Hutch
- The Monster Hutch is a new building that can be found in Robin's menu and uses the pre-1.6 Slime Hutch building exterior (basically just a bigger version of the current slime hutch)
- The interior is considered a slime hutch, so slime breeding will still function there (default 20 slime capacity limit), and it can accommodate up to 40 total monsters
- Monsters added by this mod do not multiply on their own like slimes do and must be incubated in the new 'Monster Incubator'
- An optional basement expansion can be turned on in the configuration options which adds a slightly smaller 'basement' area that monsters cannot visit, but casks will work.

### Monster Incubator
- The Monster Incubator has a similar appearance to the Slime Incubator and is used to incubate monsters added by content packs for this framework
- The recipe is acquired upon receiving a letter from the wizard upon reaching 6 hearts
- Crafting it requires 2 Iridium Bars and 50 Void Essence
- Valid input items are defined by content packs, and 10 Void Essence is also consumed as 'fuel' to start incubation

### Monsters
- The monsters created by this mod are based on a subset of the vanilla game monsters, so their behavior will replicate the base type used
- These monsters will target and inflict damage to the player like their wild counterparts (unless the right monster charmer ring is worn)
- The water troughs are used by both slimes and this mod's monsters with the same ratio (1 trough 'waters' up to 5 monsters)
- If a monster has been 'watered', it can produce items overnight similar to coop animals (Auto-Grabber will work depending on the item type)

### Charmer Rings
- Vanilla or mod-added rings can be defined as 'charmer' rings for these new monsters, similar to the Slime Charmer Ring
- In addition to preventing damage to the player, these charmer rings (as well as the Slime Charmer Ring) will also prevent the player from dealing damage to the relevant monster.

## Configuration Options

- HutchExpansion: Whether to add a basement expansion upgrade for the Monster Hutch to Robin's build menu (the basement cannot be used by monsters, but casks will work there). Default: false
- RandomizeMonsterPositions: Whether to randomize monster positions in the hutch every day (to prevent them from clumping up). Default: false
- SkipRandomizeSlimePositions: Whether the above setting should leave slime positions alone. Default: false
- HutchSlimeCapacity: The maximum slime capacity for the Monster Hutch. Default: 20
- HutchMonsterCapacity: The total monster capacity (including slimes) for the Monster Hutch. Default: 40
- DoubleNodeDrops: Whether breaking nodes (ores, gems, geodes, etc) yields double in the Monster Hutch. Default: true
- NoFlooringSpawn: Whether monster produce should never spawn on flooring. Default: true
- IncubatorIsAffectedByCoopmaster: Whether incubation time is affected by coopmaster profession. Default: true
- IncubatorWobblesWhileIncubating: Whether monster incubator should wobble while it's working. Default: true
- DefaultIncubatorAdditionalRequiredItemID: The item ID for the additional required item to start incubation in the monster incubator. Default: "(O)769"
- IncubatorAdditionalRequiredItemCount: The amount of the above required to start incubation. Default: 10
- DefaultIncubatorRecipe: The crafting recipe for the monster incubator. Default: "337 2 769 50"
- DefaultIncubatorRecipeUnlock: The condition for unlocking the monster incubator recipe. Default: "f Wizard 6"
- LethalRings: Whether the rings defined in CharmerRingData allow the player to deal damage to the associated monster(s). Default: false
- VanillaSlimeRing: Whether the Slime Charmer Ring retains its vanilla properties allowing the player to damage slimes while worn. Default: false

## Modder Guide

This framework defines two data models: MonsterHutchData for adding monsters to the hutch, and CharmerRingData for applying slime charmer ring qualities to other rings and monsters.

### Monster Hutch Data

You can add monsters to be raised in the Monster Hutch by editing the juminos.MonsterHutchFramework/MonsterHutchData asset.

This consists of a string -> model lookup where...
- The key is a unique string ID for the monster.
- The value is a model with the fields listed below.

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `MonsterType` | The name of one of the game's monster types that this hutch monster will be based on.* | `string` |
| `Name` | The name for this monster (should be unique). | `string` |
| `Drops` | (_Optional_) The items to drop on monster death. This consists of a list of models defined below. | `List<Drops>` |
| `SpeedOverride` | (_Optional_) The value to set as the monster's default speed (overriding the base monster speed). | `int` |
| `FarmerCollision` | (_Optional_) Whether the monster has collision with the farmer. Default: false | `bool` |
| `MoveTowardPlayerThresholdOverride` | (_Optional_) The distance (in tiles) before the monster sees the player. Default: 2 | `int` |
| `DamageToFarmerOverride` | (_Optional_) The amount of damage the monster deals when it attacks (overriding the base monster damage). | `int` |
| `MaxHealthOverride` | (_Optional_) The amount of health the monster has (overriding the base monster health) | `int` |
| `ScaleMin` | (_Optional_) The minimum size scale percentage (random value chosen between ScaleMin and ScaleMax when monster is created). <br>Example: The game uses this to randomize Dust Sprite size with ScaleMin = 75 and ScaleMax = 101 | `int` |
| `ScaleMax` | (_Optional_) The maximum size scale percentage (ScaleMin and ScaleMax can be set to the same value for fixed scaling). | `int` |
| `TexturePath` | (_Optional_) The texture to use for this monster (if not defined will use base monster texture) | `string` |
| `NumberRequiredToProduce` | (_Optional_) How many watered monsters of this type are required for each overnight produce chance. Default: 1 <br>e.g. In the vanilla game it takes 5 watered slimes to produce a slime ball. | `int` |
| `ProduceChance` | (_Optional_) The percent chance this monster will produce overnight if watered conditions are met. Default: 100 | `int` |
| `ProduceCondition` | (_Optional_) An additional condition (game state query) that must pass for this moster to produce overnight. | `string` |
| `DeluxeChance` | (_Optional_) If ProduceChance passes, the percent chance the produce item is selected from the deluxe produce list instead (i.e. both ProduceChance and DeluxeChance must pass for deluxe produce to drop). Default: 33 | `int` |
| `DeluxeCondition` | (_Optional_) An additional condition (game state query) that must pass for this monster to drop a deluxe itme overnight. | `string` |
| `ProduceData` | (_Optional_) The item(s) produced by this monster. This consists of a list of models defined below. | `List<ProduceData>` |
| `DeluxeProduceData` | (_Optional_) The deluxe item(s) produced by this monster. This consists of a list of models defined below. | `List<DeluxeProduceData>` |
| `InputItemId` | The item id to place in the Monster Incubator to create this monster. | `string` |
| `InputItemCount` | (_Optional_) The number of the input item required to start incubation. Default: 1 | `int` |
| `IncubationTime` | (_Optional_) The number of days until incubation is complete. Default: 2 | `int` |
| `OutputWeight` | (_Optional_) The weighted chance this monster is selected when incubation is complete (if more than 1 monster is found with the same input item Id). Default: 1 | `int` |

\* Monster types that work: Angry Roger, Bat, Frost Bat, Lava Bat, Iridium Bat (produces strange results), Haunted Skull, Magma Sprite, Magma Sparker, Big Slime, Blue Squid, Bug, Pepper Rex, Dust Spirit, Dwarvish Sentry, Fly, Ghost, Carbon Ghost, Putrid Ghost, Grub (doesn't really move), Hot Head, Lava Lurk, Spider, Metal Head, Mummy**, Rock Crab, Lava Crab, Stick Bug, Iridium Crab, Truffle Crab (often gets stuck), False Magma Cap, Stone Golem, Wilderness Golem, Serpent, Royal Serpent, Shadow Brute, Shadow Shaman, Shadow Sniper, Skeleton, Skeleton Mage, Squid Kid 
<br><br>\*\* if Mummy is cut down and rises again, its output damage value resets ignoring modifications made by this mod

#### Drops

Each entry in the drops list is a model consisting of the fields listed below:

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `Id` | The ID for this entry in the list (unique to the list). | `string` |
| `ItemId` | The item ID. | `string` |
| `Chance` | (_Optional_) The percent chance this item drops. Default: 100 | `int` |
| `Quantity` | (_Optional_) The amount of this item to drop. Default: 1 | `int` |

#### ProduceData / DeluxeProduceData

Each entry in the ProduceData and DeluxeProduceData lists is a model consisting of the fields listed below:

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `Id` | The ID for this entry in the list (unique to the list). | `string` |
| `ItemId` | The item ID. | `string` |
| `Count` | (_Optional_) The number of this item to produce. Default: 1 | `int` |
| `IsDropped` | (_Optional_) Whether this item gets dropped as debris (rather than spawn like forage). Default: false | `bool` |
| `Weight` | (_Optional_) The weighted value for this item in the list when choosing which item to produce. Default: 1 | `int` |

Example:
```
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "juminos.MonsterHutchFramework/MonsterHutchData",
            "Entries": {
                "{{ModId}}_MagmaSprite": {
                    "MonsterType": "Magma Sprite",
                    "Name": "{{i18n:magma-sprite.name}}",
                    "Drops": [
                        {
                            "Id": "CinderShard",
                            "ItemId": "(O)848",
                            "Chance": 100,
                            "Quantity": 2
                        }
                    ],
                    "SpeedOverride": 1,
                    "FarmerCollision": false,
                    "MoveTowardPlayerThresholdOverride": 2,
                    "DamageToFarmerOverride": 6,
                    "MaxHealthOverride": 25,
                    "ScaleMin": 100,
                    "ScaleMax": 100,
                    "TexturePath": "Characters\\Monsters\\Magma Sprite",
                    "NumberRequiredToProduce": 2,
                    "ProduceChance": 100,
                    "DeluxeChance": 10,
                    "ProduceData": [
                        {
                            "Id": "CinderStone1",
                            "ItemId": "(O)843",
                            "Count": 1,
                            "IsDropped": false,
                            "Weight": 1
                        },
                        {
                            "Id": "CinderStone2",
                            "ItemId": "(O)844",
                            "Count": 1,
                            "IsDropped": false,
                            "Weight": 1
                        }
                    ],
                    "DeluxeProduceData": [
                        {
                            "Id": "Obsidian",
                            "ItemId": "(O)575",
                            "Count": 1,
                            "IsDropped": false,
                            "Weight": 1
                        }
                    ],
                    "InputItemId": "(O)848",
                    "InputItemCount": 10,
                    "IncubationTime": 2,
                    "OutputWeight": 1
                },
            }
        },
    ]
}
```

### Charmer Ring Data

You can define monsters to be 'charmed' (cannot deal damage or collide with player) by a ring by editing the juminos.MonsterHutchFramework/CharmerRingData asset.

This consists of a string -> model lookup where...
- The key is the Item ID for the ring.
- The value is a model with the fields listed below.

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `CharmedMonsters` | The monsters this ring will 'charm'. This consists of a list of models defined below. | `List<CharmedMonsterData>` |

#### CharmedMonsterData

Each entry in the CharmedMonsters list is a model consisting of the fields listed below:

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `Id` | The ID for this entry in the list (unique to the list). | `string` |
| `MonsterName` | The name of monster (must be exact match to either the name of a vanilla monster as found in Data/Monsters or the `Name` field for a monster in MonsterHutchData). | `string` |
| `Sound` | (_Optional_) The sound this monster makes when 'petted'. | `string` |
| `SpeechCondition` | (_Optional_) A condition (game state query) to check for this monster to display a speech bubble when 'petted'. | `string` |
| `SpeechBubbles` | (_Optional_) The content and appearance of speech bubbles when 'petting' this monster. This consists of a list of models defined below. If no entries are found in this list, it will default to a heart. | `List<SpeechBubbles>` |

#### SpeechBubbleData

Each entry in the SpeechBubbles list is a model consisting of the fields listed below:

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| `Id` | The ID for this entry in the list (unique to the list). | `string` |
| `Text` | The text to display in the speech bubble. | `string` |
| `Weight` | (_Optional_) The weighted chance of choosing this entry in the list. Default: 1 | `int` |
| `Pretimer` | (_Optional_) The amount of time (in milliseconds) to delay the speech bubble after it is triggered. Default: 0 | `int` |
| `Duration` | (_Optional_) The amount of time (in milliseconds) to display the speech bubble. Default: 1500 | `int` |
| `Style` | (_Optional_) The style of the text (not sure what this actually does but feel free to experiment). Default: 2 | `int` |
| `Color` | (_Optional_) The number value for the color of the displayed text (-1 = Default, 1 = Blue, 2 = Red, 3 = Purple, 4 = White, 5 = Orange, 6 = Green, 7 = Cyan, 8 = Gray, 9 = JojaBlue, _ = Black). Default: -1 | `int` |

Example:
```
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "juminos.MonsterHutchFramework/CharmerRingData",
            "Entries": {
    // Assign monsters to custom ring
                "{{ModId}}_Magma": {
                    "RingId": "{{ModId}}_MagmaSprite",
                    "CharmedMonsters": [ 
                        {
                            "Id": "MagmaSprite",
                            "MonsterName": "Magma Sprite",
                            "Sound": "magma_sprite_spot",
                            "SpeechBubbles": [
                                {
                                    "Id": "hello",
                                    "Text": "Hello there",
                                    "Weight": 2,
                                    "Pretimer": -1,
                                    "Duration": 1500,
                                    "Style": 2,
                                },
                                {
                                    "Id": "heart",
                                    "Text": "<", // this character is displayed as a heart
                                    "Weight": 1,
                                    "Pretimer": -1,
                                    "Duration": 1500,
                                    "Style": 2,
                                }
                            ]
                        },
                        {
                            "Id": "{{ModId}}_MagmaSprite",
                            "MonsterName": "{{ModId}}_MagmaSprite",
                            "Sound": "magma_sprite_spot",
                            "SpeechBubbles": []
                        },
                    ]
                },

    // Assign monsters to vanilla game ring (810 is ID for the Crabshell Ring)
                "{{ModId}}_Crabs": {
                    "RingId": "810",
                    "CharmedMonsters": [ 
                        {
                            "Id": "RockCrab",
                            "MonsterName": "Rock Crab",
                            "Sound": "skeletonHit",
                            "SpeechBubbles": []
                        },
                        {
                            "Id": "{{ModId}}_IronCrab",
                            "MonsterName": "{{ModId}}_IronCrab",
                            "Sound": "skeletonHit",
                            "SpeechBubbles": []
                        },
                    ]
                }
            }
        }
    ]
}
```
