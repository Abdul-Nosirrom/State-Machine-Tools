# State-Machine-Tools
A variety of tools and editors for building hierarchal state machines in Unity, for both player characters and AI. Each characters state machine is stored in a scriptable object, which olds all states & event data related to it. Fairly stable at the moment, though wouldn't say it's quite drag & drop just yet (Might be, but some quirks need to be better documented in the initial creation process), some gameplay code left over in the state manager that are likely not transferrable as well as initial setup of InputData objects. If anything else, should hopefully be a neat learning resource for someone. Thanks to Tara Doak for their wonderful learning resources on jump-starting this process. Will hopefully document it more and make doubly it's working "out of the box" once I finalize it in a current project.

#

# Current Functionality

## Utility Scripts
There are multiple utility scripts provided, from Singleton classes, game/audio managers & systems, vector extensions, and a series of static helper methods.

## Input Buffer
![Alt text](Screenshots/InputData.png?raw=true "Character State Editor Example")
![Alt text](Screenshots/InputManager.png?raw=true "Character State Editor Example")

An Input Buffer system is provided, using a circular queue. The input buffer is managed by the Input Manager and a collection of InputData object, each InputData object holds an Input Map then auto generates buttom inputs. The unit of time for the input buffer is Time.FixedDeltaTime.

# State Machine Tools

## Character Manager
![Alt text](Screenshots/CharacterManager.png?raw=true "Character State Editor Example")

Small editor to handle the creation of character data objects. Here's where you can link an animator controller given how much states & animations are tied.

## State Editor

![Alt text](Screenshots/StateEditor.png?raw=true "Character State Editor Example")

Here you may create different states for a selected character. Assigning a CharacterStateManager script to a character object will allow it to access the state machine of the character its' assigned to. When creating a state, the default options provided are:

- State Length: Refers to animation length in units of Time.FixedDeltaTime
- Blend Rate: The blending parameter into other animations
- Loop: Whether the state should loop - which causes the state time counter to reset at the end of time Length
- Event Function Calls: What and when certain event functions are called
- Has Cooldown: Whether or not the state has a given cooldown. If checked, a cooldown time (in seconds) can be set
- Is Unlockable: Whether or not the state must be unlocked before it can be used. If checked, then this will be checked for when searching for next valid states.

## Event Editor

![Alt text](Screenshots/EventEditor.png?raw=true "Event Script Editor Example")

Events are scriptable object based. To create an event, inherit from "StateEventObject" and implement an Execute() method which takes in the StateManager as the first parameter. The editor will read through the parameters of the method and automatically fill out parameters in the State Editor.
To add new types, do so in the GenericValueWrapper script as well as add its appropriate editor field in the CharacterStateEditor script.

## Attack Editor

![Alt text](Screenshots/AttackEditor.png?raw=true "Attack Editor Example")

The attack editor allows you to add various attacks to a given state - and various parameters associated with that attack. To set up the hitbox for a given attack, just edit the hitbox attatched to the selected character and then click Apply Hitbox, which will automatically set the hitbox size and position to what you just set when the attack initiates, in the given attack duration/time set in the slider above. Animation Direction is just a parameter passed into the object that has been hit to blend between different hit directions in its HIT state.

## State Machine Editor

![Alt text](Screenshots/MoveListEditor.png?raw=true "State Machine Editor Example")

Once you've set up your states, events, and attacks - you can now link them in and specify their transition conditions in the State Machine Editor. You may also specify different State machines to be used - therefore acting like a Hierarchal FSM. There are a few different values to edit within a given state in this editor:

- Directional Input: Seen in the Air Launch state, specifies specific directional input that must be satisfied to enter the state
- Button Input: Top right, specifies the button input associated with the state
- State: Specifies the character state, set up in the State Editor, associated with the state
- Priority: Specifies the priority of a state - useful when you have two states with the same button input, but one includes a directional input - setting the one with a directional input to higher priority will prevent that state always being skipped over as the state manager gets data from the input buffer.
- Limit State Enter: If this is checked, you can limit how many times you can enter this state in while in that state machine. This counter is reset once you transition to another state machine.
- Is Entry State: One state per state machine should have this checked, serves as the entry state as the name implies.

Conditions are tied to the transition as opposed to the state itself. Interrupts, when set, will go to the state the interrupt is associated with regardless of anything.

In an active scene during playmode, selecting an object with a state manager component will then give realtime updates in the state machine editor:

![Alt text](Screenshots/realtime.png?raw=true "Real Time Updates Example")


## AI State Machine Editor

![Alt text](Screenshots/AIStateMachine.png?raw=true "AI State Machine Editor Example")

Not much is different except for the removal of inputs. Can also now set the probability for each state, which can be evaluated for when searching for a state transition.

