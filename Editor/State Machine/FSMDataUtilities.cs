
using System.Collections.Generic;
using UnityEditor;

public static class FSMDataUtilities
{
    private static List<CharacterData> m_CharacterData;
    public static bool isDataCollected;

    public static bool CollectCharacterData()
    {
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(CharacterData).Name);  //FindAssets uses tags check documentation for more info
        m_CharacterData = new List<CharacterData>(guids.Length);
        for(int i =0;i<guids.Length;i++) 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            m_CharacterData.Add(AssetDatabase.LoadAssetAtPath<CharacterData>(path));
        }

        isDataCollected = true;
        return isDataCollected;
    }

    public static bool AreThereCharacters()
    {
        CollectCharacterData();
        return m_CharacterData.Count > 0;
    }

    public static List<string> GetCharacterNames()
    {
        List<string> characterNames = new List<string>();
        foreach (CharacterData charData in m_CharacterData)
        {
            if (charData != null)
                characterNames.Add(charData.character.name);
        }

        return characterNames;
    }

    public static CharacterData GetCharacterData(int i)
    {
        if (i < 0 || i >= m_CharacterData.Count) return null;

        return m_CharacterData[i];
    }
}