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
   * Arming sword: A simple yet prestigous one handed sword used by knights and lower ranking members alike. Arming sword was a relatively poor weapon against contemporary armour but it had low weight and was extremely efficient against an unarmoured opponent. Good at slashing, good at stabbing. 
   * Viking sword: A relatively primitive weapon compared to an arming sword. They were heavier and less balanced, but were extremely durable and in some cases their workmanship surpassed most weapons from the later eras. They were almost always used in a combination with a shield. Good at slashing, average at stabbing.
   *seax: A germanic one bladed weapon used for cutting. It was employed as a personal weapon for a common tribesman. Good for slashing and cutting, pretty shit for stabbing. 
   * messer/falchion: Single bladed weapon used in the later medieval era and used mostly by non noble soldiers and mercenaries. Despite being a choice for poorer troops, the weapons was of relative high quality and was able to deliver powerfull blows without chipping away. Very good at slashing, poor at stabbing.
   
 - one handed blunt weapons
   * Knopped mace: A light mace wth a wooden handle usually made from cast bronze and iron. Historically it was employed mostly by poor tribesmen in the east or East european soldiers. A very cost effiencent with Average swings and slight armour penetration. ![Progression](https://memestatic1.fjcdn.com/comments/+_858c82ae2c4e4e26382ff09c6aaa450e.jpg)
   * Flanged mace: A heavier yet suprisingly balanced mace used historically by islamic armies and later European ones. Earlier type of flanged maces had wooden handle and iron macehead, later variantions were made entirely made from steel. Powerfull swings and extreme armour penetration.
   Spiked mace: A mace with protruding spikes. Effiecent against earlier types of armour like chainmail, uneffiecent against plate. GOOD swings, Average armour penetration
   Horsemans hammer: A hammer used by knights, usually from horseback. It had a blunt side, with a spike on the other. Powerfull swings, good armour penetration.
   
   
   
 - two handed bladed weapons
 - two weaponsanded blunt (polearms) 
 - spears (1/2 handed)
 - shields (1 handed)
 - daggers (dual wielded)
 - bows (2 handed)
 - crossbows (2handed)
 
 
 Under this feature also belong some status effects like CC, knockout, cripple, bleed, poison.
 
 ####  Feature 6 - Siege weapons:
 Player will need special weapons to deal damage to player structures which arent complete shit-tier. These weapons include:
 - Trebuchet
 
 		Player crafts a trebuchet starter kit that he can then deploy at a suitable flat terrain that is not in enemy zone of control or inside a no-build zone. Upon placing a the starter kit, the player must supply it with resources needed for the trebuchet to build itself up.
		After the trebuchet was built a player can craft 3 different types of ammunition:
			- Practice shot	: cheaply made shot used for calibration. Deals 0 damage.
			- Stone shot	: Projectile made from stone, a bit costly and deals moderate splash damage to everything.
			- Tar shot		: Stone shot covered in tar and lit on fire. Upon impact deals splash damage and sets shit on fire yo.
		Trebuchet is vulnerable to fire damage and can be destroyed by players if not defended.
 ####  Feature 7 - Swimming & water:
 
 ####  Feature 8 - Team system & pinging:
 		Players are grouped in teams/clans allowing them to better coordinate with eachother.
		Damage to teammates is reduced by a certain percentage.
		Player can hold MMouse button to bring up a radial menu to ping for locations / objects / enemies. Teammates can then ping on said ping to acknowledge his ping and in case of a pinged item, mark it for themselves for a certain amount of time. Basically Apex.
 ####  Feature 9 - Sliding:
 	Apex
 ####  Feature 10 - Animals:
 Animals have always been the bread and butter of civilizations. Or mybe just butter.
 There are 2 types of animals:
 - Non - interactable animals:
 	Ducks, deer, bears, etc
- Interactable animals:
- - **Dog**
		Good boy.
			Can be used as a companion when going outside of a base.
			Can be used as help for hunting / tracking prey.
			Can be used for tracking hostile players.
			
			Has several states:
			- Track prey: Tracks nearest non hostile animal for hunt.
			- Track Players: Tracks nearest hostile player that is not in certain range od his base. When player engages in combat or if the dog is attacked, the dog becomes hostile.
			- Ignore: Dog doesnt attack under any condition.
			- Defend self: Dog will become hostile after being attacked.
			- Defend Player: Dog will become hostile after him or player is attacked.
			- Assist: Dog will become hostile after player attacks a living thing.
			- Hostile: Dog will attack nearest enemy, but prioritize the enemy that the player is attacking.
			
			Dog moves somewhat faster than player and as such is usefull in chasing down fleeing players.
- - **Horse**
		Strong boy.
		
			Can be ridden by a player.
			Horse is faster than a player.
			Player can place his inventory bags on a horse, 2-4 max.
			Horses can draw carriage, allowing players to carry even more items.
			Horse-drawn carriages can be instructed to move to various POI's where a player owned slave is present to collect gathered resources. In this case the carriage must be accompanied by a player owned slave. Enemy players can attack the carriage, causing it to drop items. Horses and the slave are immune to damage during this process. Nearby players and owner are notified if someone is attacking a resource wagon.
- - **Oxen**
		Beast of burden.
			Used to draw carriages instead of a horse. Is slower than a horse, but can carry more.
			
 ####  Feature 11 - Respawn mechanic:
 		When player's health reaches 0 from combat damage he is downed. If he fails to pick himself up the player dies.
		If a player is poisoned the player skips the downed state and straight up dies depending on the poison used.
		Once the player has died a box spawns next to his body, containing the entirety of his inventory.
		The player then respawns at the position of his eldest offspring. The said offspring is destroyed and player's avatar takes the appearence of the destroyed offspring. Any romance modifiers are reset. Actually all modifiers are reset.
		If player has no offspring he plays as his next sibling.
		if no sibling the player has to wait for a set amount of time? then he spawns next to his last spouse/mother, sister-wife, idk lol, who magically got impregnated by a local postman apparently.
 ####  Feature 12 - Player progeniture:
 probably nothing too graphic, kinda like HEAT? I'm not sure i give a shit about a rating though so who knows.
 
 		Player uses offspring and siblings as a means to speed up his respawn timers.
		To create an offspring a player must find a suitable mate of the opposite sex.
		Player can mate with other players of they are of different sex.
		Player can mate with NPC's that he successfully woos.
		Player can have sex with his slaves freely.
		
 ####  Feature 13 - PVE points of interest:
 		Places on the map where building is not allowed. Some places are rich in resources (mines) and can be used to send slaves to gather resources. These places can be contested for control by players.
		Some places give you some buffs?
		Some places teach you some emotes or recipes stuff like that.
 ####  Feature 14 - Easy server hosting:
 		It is in our interest for every baindead retard to be able to host a server.
 ####  Feature 15 - Steam integration:
 		It's gonna be hosted on steam.
 ####  Feature 16 - Armor & clothes system:
 		
 ####  Feature 17 - Inventory system:
 
