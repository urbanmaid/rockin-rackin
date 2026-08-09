# Planning

- Basic concept - Vampire Survivors + Super Monkey Ball

## Gameplay

- The player is located on a tiltable field, and within the field there is an enemy entity that can deal damage to the player.
The player's objects can be moved by the player manipulating the slope of the stage.
The player and the enemies are in the same stage, and they are all affected by the same stage gravity. The enemy, likewise, is affected and moves under the influence of the tilted stage.
Through tilting more, the tilt angle can be amplified while pressing.
- The player object has a fixed amount of health, and after a certain amount is used up, it becomes game over.
The player must keep moving and survive by eating health items.
    - The amount of stamina that can be fully replenished is limited.
    - The levels of changes in physical fitness are divided as follows.
        - Time-course decrease: slow and steady
        - Red contact damage: provides a brief but instantaneous invincibility period
        - Fall out of the field: return to the center after taking major damage
        - Health item: offsets the reduction rate or partially restores
- If enemy objects are present around the player object, the push-out function can be used.
    - However, the push-back function requires a cooldown period of a certain amount of time.
- When the stage is tilted, depending on the position of the central axis, players or enemy objects may suddenly lift off when the stage is tilted. This causes a floating effect.
    - Make use of this to enhance the sense of depth in gameplay. Tilting the player object to the center of the map, leaving only the enemy objects around the player object to float, so that the player can use the space lifted by enemy objects as evasion space.
    - Through manipulation, it provides the player with a way to indirectly attack by dealing fall damage to enemies. Enemy damage is applied only when they hit the stage at a speed above a certain level.
    When a certain amount of health is consumed, the enemy entity is destroyed and replaced with a health item.
- Player objects can increase their score by consuming health items, and upon reaching a certain score, they can upgrade the following items. Three of the following items are randomly selected.
    - Stamina item density
    - Maximum health
    - Degree of leaning further
    - Push-out cooldown / power / range
    - Luck - Increases the drop rate of health items upon enemy defeat, increases the chance of high-rank upgrades appearing, and increases the chance of critical hits occurring from fall damage
    The camera is fixed so as to track the player while facing forward.

## Manipulation

- Keyboard and mouse
    - WASD and arrow keys - tilt the stage, and some smoothing should be applied when tilting
    - Use the space - push function
    - Use Shift - Tilt More
- Gamepad
    - L-Steak - Tilt the stage
    - Use the X - Push function
    - A - Use tilt more

## Theme

- A combination of a fantasy theme and an alchemy theme
    - During the witch’s alchemical experiments, they succeed in creating a substance that absorbs matter and takes up its weight without increasing its volume, and they experiment with how much matter can be absorbed.
    - While other substances that interfere with the absorption of the substance fall together, we decide to increase the weight of the substance by avoiding them as much as possible.

# Development Method

- Generated in a local directory using ChatGPT 5.5, based on the plan specification.
After reviewing the plan specifications and actual gameplay, it was developed while repeatedly requesting revisions for any differences from the play experience.
Most of the game code and shader code use AI.
    - Some parts of the game code, as well as art assets, UI, and particles, do not use AI.