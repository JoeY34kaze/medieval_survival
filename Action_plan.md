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
- Bounty system (18)
- Insurance system (19)
- Markets (slave, horse, items) (20)
- Island hopping (Sailing) (20)
- Central City (21)

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

##### Expanded feature 3.1: Base upkeep
	- Players are limited to 2 types of bases:
		1.) Guild base / main base
		2.) Raiding base / Temporary shitshack.
	Guild Base:
		Player builds a shitshack and places a guild emblem somewhere (wall) which prevents the building from decay. For the crest to funnction it needs to be fed a resource ( probably gold), bigger the base = more resource consumption. Consumption is calculated on the amount of blocks in a base * average distance of block from the crest.
		All of the players in the guild are limited to one Guild base, if player has his own base it becomes a Shit shack after joining a guild. Guild Master's base always takes priority in designation of guild base.
		If the emblem runs out of resources, players get a warning and base slowly starts taking damage from decay.
	Temporary shitshack:
		The fist base that a player can build, it's just a regular base without emblem. If a player is already in a guild with a guild house he cannot build a new guild base unless he leaves the guild. 
		Guild can place Shitshacks without limits.
		Shitshacks are subject to Decay.
		Shitshacks cannot be placed too close to emblems of other guilds.
		Usually they will be used for temporary bases when raiding, hunting, shit like that.
##### Expanded feature 3.2: Stockpile
	Players can keep their loot within their base in items designed specificly for the purpose of storage:
		1.) Stockpile:
			An area withing the base or a large chest? where all manner of items can be placed.
			Every crafting table and Player that belongs to the guild can access this stockpile automatically when crafting. This eliminates the need to look over chests for required materials, placing them in inventory, placing them in crafting table and crafting. You just press craft and it does it.
			Stockpile cannot be secured. In the case of raid everything in stockpile can be taken by raiders.
			
		2.) Chest:
			A much smaller storage unit designed specifically to store high value items like gold, best gear and high level resources.
			Unlike stockpile this container can be locked and raiders have to spend more time and effort to get whats inside.
			When the item is destroyed by raiders, a certain percentage of items inside are destroyed.

####  Feature 4 - Biological needs:
	Player has a meter for thirst and food. When they fal below or to 0 they get increasing debuffs that end when player dies or when he raises the meter above 0. Thirst is raised by water, hunger is raised by eating.
	Player can contract diseases from various sources including but not limited to eating uncooked food, being stabbed, being poisoned, being bitten by wild animal, randomly contracting something while in a certain biome like swamp or forest, having unprotected sex with your mother, etc. Upon contracting a disease or poison the player must drink an antidote which removes the illness and or poisons from his body. Diseases might have a chance to spread to nearby players?.

####  Feature 5 - Personal weapons:
This is quite a broad feature and encompassess all of the weapons that a player can use in regular PVP. Generally this includes (can be changed):
 - one handed bladed weapons
   * Arming sword: A simple yet prestigous one handed sword used by knights and lower ranking members alike. Arming sword was a relatively poor weapon against contemporary armour but it had low weight and was extremely efficient against an unarmoured opponent. Good at slashing, good at stabbing, Bad reach.
   * Viking sword: A relatively primitive weapon compared to an arming sword. They were heavier and less balanced, but were extremely durable and in some cases their workmanship surpassed most weapons from the later eras. They were almost always used in a combination with a shield. Good at slashing, average at stabbing, bad reach.
   *seax: A germanic one bladed weapon used for cutting. It was employed as a personal weapon for a common tribesman. Good for slashing and cutting, pretty shit for stabbing very bad reach. 
   * messer/falchion: Single bladed weapon used in the later medieval era and used mostly by non noble soldiers and mercenaries. Despite being a choice for poorer troops, the weapons was of relative high quality and was able to deliver powerfull blows without chipping away. Very good at slashing, poor at stabbing, bad reach.
   
 - one handed blunt weapons
   * Knopped mace: A light mace wth a wooden handle usually made from cast bronze and iron. Historically it was employed mostly by poor tribesmen in the east or East european soldiers. A very cost effiencent weapon with Average swings and slight armour penetration, very bad reach. ![Progression](https://memestatic1.fjcdn.com/comments/+_858c82ae2c4e4e26382ff09c6aaa450e.jpg)
   * Flanged mace: A heavier yet suprisingly balanced mace used historically by islamic armies and later European ones. Earlier type of flanged maces had wooden handle and iron macehead, later variantions were made entirely made from steel. Powerfull swings and extreme armour penetration, very bad reach.
   Spiked mace: A mace with protruding spikes. Effiecent against earlier types of armour like chainmail, uneffiecent against plate. GOOD swings, Average armour penetration, very bad reach.
   Horsemans hammer: A hammer used by knights, usually from horseback. It had a blunt side, with a spike on the other. Powerfull swings, good armour penetration, very bad/bad reach.
   
 - two handed bladed weapons
   * Longsword: A two handed sword that required a lot of knowledge and skill to use. As the time went on the the armour progressed, the tip of the blace became more and more pointed and sharp in order to compete with it. Very Good as slashing, very good at stabbing, slight armour penetration with stab attacks, average reach.
   Grosse messer/military falchion: A two handed single blade sword, used mostly by poorer troops and merceneries. While poorer quality tham longsword, it was still able to deliver powerfull blows. Superior at slashing, bad at stabbing.
   * Claymore: A slightly larger and less "wieldy" type of long sword, used by Scotish highlanders. Superior at slashing, good at sta stabbing.
   * Zveihandler: The biggest swoords on the battlefield used by the biggest men on the battlefield. It was used to defend critical formation and to disrupt pike formation. The size of the weapon alone made caused internal injuries even while wearing and armour. Superior at slashing, poor at stabbing, slight armour penetration. 
   
 - two handed blunt weapons (polearms) 
   * War hammer: A two handed hammer used mostly by noble fighters. It's shorter than most polearms but is faster, more agile and made especially to ruin the day for even the most armoured opponents. Good blows. average stabs, very good armour penetration.
   * Halberd: Am multi purposeful weapons of good range used for slashing, stabbing, and hooking. Later variantions were designed to penetrate armour. Good slashes, good stabs, average armour penetration.
   +Poleaxe: A smaller polearm with an axehead, spike, and a hammer, used mostly by knights. Very good slashing, average stabs.
   
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
 		Probably going to be like witcher 3-ish.
 ####  Feature 17 - Inventory system:
 		Player has a loadout of items that are currently equipped. These items do not calculate into the inventory weight.
		Aside from this, player has a certain amount of inventory slots where he can place items inside. Inventory is limited by its weight. To compensate for this, player can craft backpacks.
		
		Backpack:
			Player crafted item, one of the most basic and common items in the game. Greately increases the weight available for player to carry. The heavier the backpack, the slower the player walks. Items inside the backpack are bound to backpack, not the player.
			When player enters combat he can choose to drop a backpack, allowing him to be unburdened and free to fight his agressor on par.
			Backpack can be placed / dropped on the ground. It can then be picked up by any player by interacting with it.
			Backpack can be placed on a horse. A horse can carry around 4 backpacks worth of weight, can be changed incase horses will have different stats.
			Backpacks can be placed on wagons, wagon can hold a fuckload of weight. Used for gathering resources and raiding.
 ####  Feature 18 - Bounty system:
  	Scenario:
	
	Player A is a farmer. while farming for resources, he gets ganked by player B. Player A is angry and goes to Central City and places a bounty on player B. Player C is focused on PVP and is notified about a newly created bounty. He travels to the nearest notice board and accepts the bounty contract. Player C finds Player B and knocks him out. He then draggs him or his head (specified by bounty) to the Central city's office where he trades in his prey in exchange for a bounty reward, paid by player A.
		There are several types of bounties:
			- 1) Kill player: Bounty reward is paid when a bounty hunter brings the head of player B to the central city's office.
			
			- 2) Kidnap player: Bounty hunter needs to knock player B out and drag him to the office. 
			Contract is more expensive.
			
			- 3) Kill relative: Same as 1, but it targets player B's relative ( child or spouse).
			
			- 4) kidnap relative : Same as 2, but it targets player B's relative ( child or spouse).
			
			- 5) Kill guild member: Same as 1, but it targets player B or his guild members.
			
			- 6) kidnap guild member: Same as 1, but it targets player B or his guild members.
		
		Player A can place several bounties ( max X ), where he pays the reward in advance, allowing for bigger persistence of threat upon player B.
		
		One time contracts:
			- Conquest: The most expensive contract, set by the central city, not the player A.
				Player C has to raid player B nad bring some proof of it to the central city to be paid.
				
 ####  Feature 19 - Insurance system:
 		Player can pay a large amount of gold to insure his equipped item for a certain amount of time.
		Insurance is valid when he is outside his base.
		Insured items are not dropped upon death, but are transferred to central city storage.
		Player who  insured it can get them after a certain amount of time.
		This is mostly meant for bounty hunters who suck.
 ####  Feature 20 - Markets (slave, horse, items):
 		Described in Feature 22.
		
 ####  Feature 21 - Island hopping ( Sailing ):
 
 ####  Feature 22 - Central City:
 		Somewhere in the middle of the map is an npc city where combat is forbidden. City offers several services that the player can use:
		1.) Slave market
			Broken slaves can be auctioned off to other players. Players place their bid on a slave. Each slave is being sold for an amount of time specified by the seller. Seller can set an optional buyout price at which a buyer can instantly buy the slave. If no buyout price is placed or paid, the slave is sold to the highest bidder once the timer runs out.
			If no bids are placed the slave will be teleported inside the sellers house, close to the wheel of pain/emblem or something like that. The seller is notified that no one wanted to buy his slave.
		2.) Horse market
			-Horses will probably be able to be bred, as such some horses are more valuable than others. Selling works by the same principle as slaves.
		3.) Auction house / item market:
			- Player can auction some of the items he possesses off by the same system as slaves. Player is limited to X amount of items that are currently selling.
		4.) Player item insurance:
			- Player can pay a large amount of gold in order to insure his equipped item/s. (Feature 19)
		5.) Player bounty registration and claiming.
 			- (Feature 18)
 
