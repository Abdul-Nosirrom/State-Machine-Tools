# State-Machine-Tools
A variety of tools and editors for building hierarchal state machines in Unity, for both player characters and AI. The data of each state is stored in a scriptable object, named Action Data. Creating an asset object will then allow you to edit the SO data in the following editors.

#

# TODO:
The tools provided are currently WIP, the priority now is to extend the State Machine for use with AIs, the conditions window in the State Machine Editor below is the first step, and will remove tying conditions to Character States, but instead state transitions.

The tools are also very prone to bugs at the moment, as some recent development has broken a bit of the functionality when it comes to the State Machine Editor

# Current Functionality

## Utility Scripts
There are multiple utility scripts provided, from Singleton classes, game/audio managers & systems, vector extensions, and a series of static helper methods.

## Systems
Some basic environment editor systems are provided. Allowing the designer to set certain environmental behaviors like move a platform from X to Y based on a curve if a certain event is triggered. This system is still very basic.

## Input Buffer
An Input Buffer system is provided, collecting input that's specified in the Action Data scriptable object. The unit of time for the input buffer is Time.FixedDeltaTime.

# State Machine Tools

## State Editor

![Alt text](Screenshots/StateEditor.png?raw=true "Character State Editor Example")

Here you may create different states for a selected character. Assigning a CharacterStateManager script to a character object will allow it to access the state machine of the character its' assigned to. When creating a state, the default options provided are:

- State Length: Refers to animation length in units of Time.FixedDeltaTime
- Blend Rate: The blending parameter into other animations
- Loop: Whether the state should loop - which causes the state time counter to reset at the end of time Length
- Various Conditions: Grounded/On Rail - these are temporary at the moment and will be moved into the State Machine Editor below.
- Event Function Calls: What and when certain event functions are called
- Interrupts: States that when an interrupt is encountered (Like GROUND from an aerial state), what state to automatically transition to.


## Event Editor

![Alt text](Screenshots/EventEditor.png?raw=true "Event Script Editor Example")

The script editor is to add various function calls that can be applied to your states. The caveat here is that the Event Script Name MUST match the function name (Spaces don't matter). Once you've written your function and added the equivalent event script, set up its' default parameters as in the function call itself. The current supported data types are:
- Float & Numeric types
- Booleans
- Animation Curves
However, adding more types is trivial and can be done through the StateAttributes script in the Event parameter class, then add the equivalent check in the Character State Editor script and Event Script Editor script.

## Attack Editor

![Alt text](Screenshots/AttackEditor.png?raw=true "Attack Editor Example")

The attack editor allows you to add various attacks to a given state - and various parameters associated with that attack. To set up the hitbox for a given attack, just edit the hitbox attatched to the selected character and then click Apply Hitbox, which will automatically set the hitbox size and position to what you just set when the attack initiates, in the given attack duration/time set in the slider above. Animation Direction is just a parameter passed into the object that has been hit to blend between different hit directions in its HIT state.

## State Machine Editor

![Alt text](Screenshots/MoveListEditor.png?raw=true "State Machine Editor Example")

Once you've set up your states, events, and attacks - you can now link them in and specify their transition conditions in the MoveListEditor. You may also specify different State machines (Command States) to be used - therefore acting like a Hierarchal FSM. There are four different values to edit within a given state in this editor:

- Directional Input: Seen in the Air Launch state, specifies specific directional input that must be satisfied to enter the state
- Button Input: Top right, specifies the button input associated with the state
- State: Specifies the character state, set up in the State Editor, associated with the state
- Priority: Specifies the priority of a state - useful when you have two states with the same button input, but one includes a directional input - setting the one with a directional input to higher priority will prevent that state always being skipped over as the state manager gets data from the input buffer.

Conditions are also something that are currently WIP, mainly set up for work to start on the AI extension of these editors, however, they're also useful for player editors. They are tied to state transitions.