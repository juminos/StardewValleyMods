# Monster Hutch Framework

Monster Hutch Framework is a framework mod which expands the Slime Hutch into a Monster Hutch allowing custom monsters to be added to the game that function like slime hutch slimes.
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
- The Slime Hutch is replaced by the Monster Hutch which has the same appearance as the Slime Hutch
- The interior is double the size of a Slime Hutch and allows up to 40 monsters (including slimes)
- Monsters added by this mod do not multiply on their own like slimes do, but any slimes in the hutch will continue to multiply until total monster count has reached 40
- The optional basement expansion can be turned on in the configuration options and allows the basement area to be used for casks, but monsters will not use it.

### Monster Incubator
- The Monster Incubator has a similar appearance to the Slime Incubator but is only used to incubate monsters added by content packs for this framework
- The recipe is acquired upon receiving a letter from the wizard after reaching 6 hearts
- Crafting it requires 2 Iridium Bars and 50 Void Essence
- Input items are defined by content packs, and 10 Void Essence is also consumed to start incubation

### Monsters
- The monsters created by this mod are based on a subset of the vanilla game monsters, so their behavior will be similar
- These monsters will take and inflict damage to the player like their wild counterparts (unless the right monster charmer ring is worn)
- The same water troughs that slimes use to produce slime balls are also used by this mod's monsters
- If a monster has been 'watered', it can produce items overnight similar to coop animals (Auto-Grabber will work depending on the item type)

### Charmer Rings
- Vanilla or modded rings can be defined as 'charmer' rings similar to the Slime Charmer Ring
- In addition to preventing damage to the player, these charmer rings (and the slime charmer ring) will also prevent the player from dealing damage to the relevant monster.

## Configuration Options

- ReplaceHutchInterior: Whether to expand the interior of the slime hutch and rename it 'Monster Hutch' to accomodate up to 40 monsters. Default: true
- HutchExpansion: Whether to add basement expansion upgrade for the Monster Hutch to Robin's build menu (the basement cannot be used by monsters, but casks will work there). Default: false
- RandomizeMonsterPositions: Whether to randomize monster positions in the hutch every day (to prevent them from clumping up). Default: true
- SkipRandomizeSlimePositions: Whether the above setting should leave slime positions alone. Default: false
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
| MonsterType | The name of the monster this hutch monster will be based on. <br>Most monsters found in the game's monster data will work. <br>(Known to work: Bat, Frost Bat, Lava Bat, Iridium Bat, Magma Sprite, Magma Sparker, Dust Spirit, Spider, Rock Crab, Stick Bug) | string |
| Name | The unique name for this hutch monster (should match the key for this entry). | string |
| Sound | (_Optional_) The name of the sound used by this mod (currently only used when 'petting' the monster). | string |
| Drops | (_Optional_) The items to drop on monster death. This consists of a list of models defined below. | List\<Drops\> |
| SpeedOverride | (_Optional_) The value to set as the monster's default speed (overriding the base monster speed). | int |
| FarmerCollision | (_Optional_) Whether the monster has collision with the farmer. Default: false | bool |
| MoveTowardPlayerThresholdOverride | (_Optional_) The distance (in tiles) before the monster sees the player. Default: 2 | int |
| DamageToFarmerOverride | (_Optional_) The amount of damage the monster deals when it attacks (overriding the base monster damage). | int |
| MaxHealthOverride | (_Optional_) The amount of health the monster has (overriding the base monster health) | int |
| HideShadow | (_Optional_) Whether to hide the monster's shadow (most monsters in the game seem to have this set to 'true'). Default: true | bool |
| ScaleMin | (_Optional_) The minimum scale percentage to randomize size (random value chosen between ScaleMin and ScaleMax on monster creation) <br>Example: The game uses this to randomize Dust Sprite size with ScaleMin = 75 and ScaleMax = 101 | int |
| ScaleMax | (_Optional_) The maximum scale percentage to randomize size | int |
| TexturePath | The texture to use for this monster (if not defined will use base monster texture) | string |
| NumberWatered | (_Optional_) The number of this monster type that 1 filled water trough will water (only watered monsters will produce items). <br>Default: 1 | int |
| NumberToProduce | (_Optional_) How many watered monsters are required to produce overnight. Default: 1 <br>e.g. In the vanilla game it takes 5 watered slimes to produce a slime ball. | int |
| ProduceChance | (_Optional_) The percent chance the monster will produce overnight if watered conditions are met. Default: 100 | int |
| DeluxeChance | (_Optional_) The percent chance that the produce is is replaced by deluxe produce. Default: 33 | int |
| ProduceData | (_Optional_) The item(s) produced by this monsters. This consists of a list of models defined below (same as DeluxeProduceData). | List\<ProduceData\> |
| DeluxeProduceData | (_Optional_) The deluxe item(s) produced by this monster. This consists of a list of models defined below (same as ProduceData). | List\<DeluxeProduceData\> |
| InputItemId | The item id to place in the Monster Incubator to create this monsters. | string |
| InputItemCount | (_Optional_) The number of the input item required to start incubation. Default: 1 | int |
| IncubationTime | (_Optional_) The number of days until incubation is complete. Default: 2 | int |
| OutputWeight | (_Optional_) The weighted chance this monster is selected when incubation is complete (if more than 1 monster is found with the same input item Id). Default: 1 | int |

#### Drops

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| Id | The ID for this entry in the list (unique to the list). | string |
| ItemId | The item ID. | string |
| Chance | (_Optional_) The percent chance this item drops. Default: 100 | int |

#### Produce Data / Deluxe Produce Data

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| Id | The ID for this entry in the list (unique to the list). | string |
| ItemId | The item ID. | string |
| Count | (_Optional_) The number of this item to produce. Default: 1 | int |
| IsDropped | (_Optional_) Whether this item gets dropped as debris (rather than spawn like forage). Default: false | bool |
| Weight | (_Optional_) The weighted value for this item in the list when choosing which item to produce. Default: 1 | int |

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
                    "Name": "{{ModId}}_MagmaSprite",
                    "Sound": "magma_sprite_spot",
                    "Drops": [
                        {
                            "Id": "CinderShard",
                            "ItemId": "(O)848",
                            "Chance": 100
                        }
                    ],
                    "SpeedOverride": 1,
                    "FarmerCollision": false,
                    "MoveTowardPlayerThresholdOverride": 2,
                    "DamageToFarmerOverride": 6,
                    "MaxHealthOverride": 25,
                    "HideShadow": true,
                    "ScaleMin": 100,
                    "ScaleMax": 100,
                    "TexturePath": "Characters\\Monsters\\Magma Sprite",
                    "NumberWatered": 5,
                    "NumberToProduce": 2,
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

### Ring Charmer Data

You can define monsters to be 'charmed' (cannot deal damage or collide with player) by a ring by editing the juminos.MonsterHutchFramework/CharmerRingData asset.

This consists of a string -> model lookup where...
- The key is the Item ID for the ring.
- The value is a model with the fields listed below.

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| CharmedMonsters | A list of monster names that this ring will affect. This can be the name of a monster as defined in the game's monster data or added by a mod. | List\<string\> |

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
                    "CharmedMonsters": [ "Magma Sprite", "Magma Sparker" , "{{ModId}}_MagmaSprite", "{{ModId}}_MagmaSparker" ]
                },

    // Assign monsters to vanilla game ring (810 is ID for the Crabshell Ring)
                "810": {
                    "CharmedMonsters": [ "Rock Crab", "Lava Crab", "{{ModId}}_CopperCrab", "{{ModId}}_GoldCrab" ]
                }
            }
        }
    ]
}
```
