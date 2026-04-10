# Stealth Documentation

## Overview
The **Stealth Mechanic** package is a scalable stealth system for 3D Godot projects. It allows developers to implement their own scalable system by having enemies who react to the player's flashlight. Key features include:
- First person player who can use toggle a flashlight
- Enemies that run on a finite state machine that dictate their behavior
- An array of way points which are implemented to tell where the enemy will patrol to
- Easily changeable Player sound effects 

---

### Addition to project
To add to your project:
1. Download either the entire project or just the "player.tscn" and "enemy.tscn" packed scenes.
2. Adjust any inspector values for the areas placed on the enemy for wanted behaviors.
3. Adjust the players flashlight values to get desired area of illumination.
4. Instantiate both scenes into any desired level scene.

---

### Changing Sound Assets
If other sound assets are wanted, ensure that the correct path to the sound file is entered into the
inspector values for both the player and enemy.

---

### Methods of Detection
Currently there are two different methods of detection that communicate with eachother to finally allow
the enemy to detect the player. Firstly, there are colliders arranged around the enemy that checks for the players
proximitey in regards to it's hearing range. Secondly, when the player has their flashlight turned on, it
sends out a raycast, on collision with an enemy, it will trigger a state change in the enemy to chase the player.

---

### Additions
To add in any more behavior to states in the enemy, auxiliary functions can be created and called in the match
statement in the enemies _process() method. This ensures for a flexible mechanic that can be implemented to a project
and further implemented on to fit any needs for horror/stealth games.

---

### Conclusion
Overall the stealth system should give developers a good system to use and even build off of to create
thrilling and challenging gameplay.
