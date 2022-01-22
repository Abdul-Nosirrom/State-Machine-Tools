/// <summary>
/// Nothing is needed here. Whichever game object has this script attached to it will be persistent
/// throughout the game. Then we can attach sub-systems as children which will consequentially also be
/// persistent [e.g Audio System and so on] - will continue to live throughout scene changes. One single
/// dont destroy on load master object.
/// </summary>
public class Systems : PersistentSingleton<Systems>
{
    
}