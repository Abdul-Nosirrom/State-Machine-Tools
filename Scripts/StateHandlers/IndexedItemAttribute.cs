using UnityEngine;


public class IndexedItemAttribute : PropertyAttribute 
{
    public enum IndexedItemType
    {
        SCRIPTS, 
        STATES, 
        CONDITIONS, 
        RAW_INPUTS, 
        CHAIN_COMMAND, 
        COMMAND_STATES, 
        MOTION_COMMAND
    }


    public IndexedItemType type;

    public IndexedItemAttribute(IndexedItemType type) 
    {
        this.type = type; 
    }
}


