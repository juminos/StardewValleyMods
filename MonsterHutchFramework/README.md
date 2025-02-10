# Monster Hutch Framework

Monster Hutch Framework is a framework mod which expands the Slime Hutch into a Monster Hutch and allows custom monsters to be added to the game that function like slime hutch slime monsters.
- new craftable 'Monster Incubator' produces custom monsters based on input
- custom monsters that are 'watered' can produce items overnight
- custom 'charmer' rings can be defined to prevent damage from (and to) monsters

## Table of Contents
- [Monster Hutch Framework](#monster-hutch-framework)
	- [Table of Contents](#table-of-contents)
	- [Data Models](#data-models)
		- [Monster Hutch Data](#monster-hutch-data)
			- [Monster Example](#monster-example)
		- [Charmer Ring Data](#charmer-ring-data)
			- [Ring Example](#ring-example)
	- [Configuration Options](#configuration-options)


## Data Models

This framework defines two data models: MonsterHutchData for adding custom monsters to the hutch and CharmerRingData for applying slime charmer ring qualities to other rings and monsters.

### Monster Hutch Data

You can add monsters to be raised in the Monster Hutch by editing the juminos.MonsterHutchFramework/MonsterHutchData asset.

This consists of a string -> model lookup where...
- The key is a unique string ID for the monster.
- The value is a model with the fields listed below.

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| MonsterType | The name of the monster this hutch monster will be based on. <br>Most monsters found in the game's monster data will work. <br>(Known to work: Bat, Frost Bat, Lava Bat, Iridium Bat, Magma Sprite, Magma Sparker, Dust Spirit, Spider, Rock Crab, Stick Bug) | string |
| Name | The unique name for this hutch monster (should match the key for this entry). | string |
| Sound | (_Optional_)<br>The name of the sound used by this mod (currently only used when 'petting' the monster). | string |
| Drops | (_Optional_)<br>The items to drop on monster death. This consists of a list of models defined below. | List\<Drops\> |
| SpeedOverride | (_Optional_)<br>The value to set as the monster's default speed (overriding the base monster speed). | int |
| FarmerCollision | (_Optional_)<br>Whether the monster has collision with the farmer. <br>Default: false | bool |
| MoveTowardPlayerThresholdOverride | (_Optional_)<br>The distance (in tiles) before the monster sees the player. <br>Default: 2 | int |
| DamageToFarmerOverride | (_Optional_)<br>The amount of damage the monster deals when it attacks (overriding the base monster damage). | int |
| MaxHealthOverride | (_Optional_)<br>The amount of health the monster has (overriding the base monster health) | int |
| HideShadow | (_Optional_)<br>Whether to hide the monster's shadow (most monsters in the game seem to have this set to 'true') <br>Default: true | bool |
| ScaleMin | (_Optional_)<br>The minimum scale percentage to randomize size (random value chosen between ScaleMin and ScaleMax on monster creation) <br>Example: The game uses this to randomize Dust Sprite size with ScaleMin = 75 and ScaleMax = 101 | int |
| ScaleMax | (_Optional_)<br>The maximum scale percentage to randomize size | int |
| TexturePath | The texture to use for this monster (if not defined will use base monster texture) | string |
| NumberWatered | (_Optional_)<br>The number of this monster type that 1 filled water trough will water (only watered monsters will produce items). <br>Default: 1 | int |
| NumberToProduce | (_Optional_)<br>How many watered monsters are required to produce overnight <br>e.g. in the vanilla game it takes 5 watered slimes to produce a slime ball <br>Default: 1 | int |
| ProduceChance | (_Optional_)<br>The percent chance the monster will produce overnight if watered conditions are met. <br>Default: 100 | int |
| DeluxeChance | (_Optional_)<br>The percent chance that the produce is is replaced by deluxe produce. <br>Default: 33 | int |
| ProduceData | (_Optional_)<br>The item(s) produced by this monsters. This consists of a list of models defined below (same as DeluxeProduceData). | List\<ProduceData\> |
| DeluxeProduceData | (_Optional_)<br>The deluxe item(s) produced by this monster. This consists of a list of models defined below (same as ProduceData). | List\<DeluxeProduceData\> |
| InputItemId | The item id to place in the Monster Incubator to create this monsters. | string |
| InputItemCount | (_Optional_)<br>The number of the input item required to start incubation. <br>Default: 1 | int |
| IncubationTime | (_Optional_)<br>The number of days until incubation is complete. <br>Default: 2 | int |
| OutputWeight | (_Optional_)<br>Only used if more than one monster has the same input item id. <br>The weighted chance this monster is selected when incubation is complete. <br>Default: 1 | int |

#### Drops

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| Id | The ID for this entry in the list (unique to the list). | string |
| ItemId | The item ID. | string |
| Chance | (_Optional_)<br>The percent chance this item drops. <br>Default: 100 | int |

#### Produce Data / Deluxe Produce Data

|    Field    | Description | Type |
| :----------- | :----------- | :----: |
| Id | The ID for this entry in the list (unique to the list). | string |
| ItemId | The item ID. | string |
| Count | (_Optional_)<br>The number of this item to produce. <br>Default: 1 | int |
| IsDropped | (_Optional_)<br>Whether this item get dropped as debris (rather than spawn like forage). <br>Default: false | bool |
| Weight | (_Optional_)<br>The weighted value for this item in the list when choosing which item to produce. <br>Default: 1 | int |

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

// Define custom ring
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Entries": {
                "{{ModId}}_Magma": {
                    "Name": "Magma Sprite Ring",
                    "DisplayName": "{{i18n:magmasprite-ring.name}}",
                    "Description": "{{i18n:magmasprite-ring.description}}",
                    "Type": "Ring",
                    "Category": -96,
                    "Price": 1500,
                    "Texture": "Mods/{{ModId}}/MoreRings",
                    "SpriteIndex": 2,
                    "Edibility": -300,
                    "ExcludeFromRandomSale": true,
                    "ContextTags": []
                },
            }
        },

// Assign monsters to rings in CharmerRingData
        {
            "Action": "EditData",
            "Target": "juminos.MonsterHutchFramework/CharmerRingData",
            "Entries": {

    // Add monsters to custom ring
                "{{ModId}}_Magma": {
                    "CharmedMonsters": [ "Magma Sprite", "Magma Sparker" , "{{ModId}}_MagmaSprite", "{{ModId}}_MagmaSparker" ]
                },

    // Add monsters to vanilla game ring (810 is ID for the Crabshell Ring)
                "810": {
                    "CharmedMonsters": [ "Rock Crab", "Lava Crab", "{{ModId}}_CopperCrab", "{{ModId}}_GoldCrab" ]
                }
            }
        }
    ]
}
```

## Configuration Options

