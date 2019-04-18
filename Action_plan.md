# Implementation plan

## 1 Possible feature implementations:
#### The possible features to be developed in the future are listed here. The list is still incomplete and will be added in the future. Items in this list are also very general and usually contain several features below them as is described under each item in point 2 in this document.

- Server Database (S1)
- Avatar creation (1)
- Slavery (2)
- Base building (3)
- Biological needs (4)
- Personal weapons (5)
- Siege weapons (6)
- Swimming & water (7)
- Team system & pinging (8)
- Sliding ( like apex?) (9)
- Animals (10)
- Respawn mechanic (11)
- Player progeniture (12)
- PVE points of interest (13)
- Easy server hosting (14)
- Steam integration (15)
- Armor & clothes system (16)
- Inventory system (17)

## 2 Detailed descriptions of features:
####  Feature S1 - Server Database:

    Server has player's state stored in its own local database. Server also stores all of the server's objects in its database.
    Actual implementation depends on the development team and since the exact parameters wilkl change in the future during development i    won't be going into too much details.
    - When player connects to server he should get synchronized with the server.
    - When player disconnects he can reconnect to the server as one might expect.
    
####  Feature 1 - Avatar Creation:
  
    When player first connects to server he can design his own avatar which wilkl be his player object. We are using MCS where the avatars appearence is stored as a datatype with the name of DNA. This DNA is saved onto server's DB. Only server can change a player's appearence. Upon the attributes of the DNS certain limits are imposed since we do not want too much freedom when making a character as to prevent exploits and to keep a balance in the PVP.
    
####  Feature 2 - Slavery:

Player can enslave other players. An act during which the avatar of the submissive parner dies and respawns accordingly. The dominant player eventually receives a slave based upon the player that he has caught. This feature is large and is thus divided into sub features.
#####  Expanded Feature 2.1 - Player catching & binding:
	To begin the process of enslaving a person, a player must seek out another player and engage him in combat. During the combat, if a player is incapacitated or if the player's health reached zero, the player will be knocked down and will be lying on the ground.
	Player that wishes to enslave the knocked player much have a rope/chain equipped and can then interact with the downed player. After a certain time, during which the offending player is vulnerable to interruption, the player is bound and is being dragged on the floor behind the offending player.
	Downed player is stopped from being dragged behind the offending player if either of the players die or if the offending player unequips the rope/chain. If the submissive player is not dead at this point he can get up after a certain time with minimal amount of hp and some penalties ( status effects, slowed, unable to equip items untill he removes rope,??).
	The act of catching & binding a player completes successfully when the offending player drags the downed player into the object/station designed to break the captures player's spirit.
#####  Expanded Feature 2.2 - Slave breaking:
	Upon being delivered to the slave breaking station, the submissive player is killed and respawned accordingly. At the same time the slave breaking station receives a new object (a slave to break) with the DNA appearence of the captured player. After a certain time the slave is broken and you have yourself a slave.
#####  Expanded Feature 2.1 - Slave management:
	A clan is limited to a set amount of slaves they can have because otherwise it will fuck with server performance. 
	Slaves can be equipped with gear and have their own inventory, much like a player. 
	Owner of a slave can designate one slave as his follower who will follow him and fight with him. Slave's hostility can be set to following states:
	- Ignore: Slave doesnt attack under any condition.
	- Defend self: Slave will become hostile after being attacked.
	- Defend Player: Slave will become hostile after him or player is attacked.
	- Assist: Slave will become hostile after player attacks a living thing.
	- Hostile: Slave will attack nearest enemy, but prioritize the enemy that the player is attacking.

	Slaves can also be placed around the Owner's base where they will be hostile to unauthorized players. 
	Slaves can also potentially be tasked with gathering resources from Points Of Interest around the map if some requirements are met.
	Slaves of the opposite sex can be used for the purpose of Player's progeniture.

####  Feature 3 - Base building:
Base building will be the most similar to Conan Exiles. Big Blocks. Upgradable blocks rather that many blocks. Bases will have upkeep set accordingly. Base decorations and all that will be explained here at a later date.

####  Feature 4 - Biological needs:
	Player has a meter for thirst and food. When they fal below or to 0 they get increasing debuffs that end when player dies or when he raises the meter above 0. Thirst is raised by water, hunger is raised by eating.
	Player can contract diseases from various sources including but not limited to eating uncooked food, being stabbed, being poisoned, being bitten by wild animal, randomly contracting something while in a certain biome like swamp or forest, having unprotected sex with your mother, etc. Upon contracting a disease or poison the player must drink an antidote which removes the illness and or poisons from his body. Diseases might have a chance to spread to nearby players?.

####  Feature 5 - Personal weapons:
This is quite a broad feature and encompassess all of the weapons that a player can use in regular PVP. Generally this includes (can be changed):
 - one handed bladed weapons
 - one handed blunt weapons
 - two handed bladed weapons
 - two handed blunt weapons
 - spears (2 handed)
 - shields (1 handed)
 - daggers (dual wielded)
 - bows (2 handed)
 - crossbows (2handed)
 
 Under this feature also belong some status effects like CC, knockout, cripple, bleed, poison.
 
 ####  Feature 6 - Siege weapons:
