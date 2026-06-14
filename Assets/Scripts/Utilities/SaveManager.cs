using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string SavePath => Application.persistentDataPath + "/save.json";

    [System.Serializable]
    public class SaveData
    {
        public float playerX, playerY, playerZ;
        public float playerHealth;
        public int activeSpellIndex;
    }

    public static void Save()
    {
        SaveData data = new SaveData();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            data.playerX = pos.x;
            data.playerY = pos.y;
            data.playerZ = pos.z;
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            data.playerHealth = health.CurrentHealth;

        SpellManager spells = Object.FindFirstObjectByType<SpellManager>();
        if (spells != null)
            data.activeSpellIndex = spells.ActiveSpellIndex;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved to " + SavePath);
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found at " + SavePath);
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public static void ApplySave(SaveData data)
    {
        if (data == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null && data.playerHealth > 0f)
            health.Heal(data.playerHealth);

        SpellManager spells = Object.FindFirstObjectByType<SpellManager>();
        if (spells != null)
            spells.SwitchToSpell(data.activeSpellIndex);

        Debug.Log("Save data applied.");
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }
}
