using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    private string server = "127.0.0.1";
    private string database = "game_database";
    private string user = "game_user";
    private string password = "your_strong_user_password"; // <-- �������� �� ��� ������

    private MySqlConnection connection;

    void Start()
    {
        // ������ ����������� � �����������, ������� ������ �������� �������������
        string connectionString = $"Server={server};Database={database};User={user};Password={password};";
        connection = new MySqlConnection(connectionString);

        try
        {
            Debug.Log("������������ � MySQL...");
            connection.Open();
            Debug.Log("���������� ������� �����������!");

            // --- ������������ ������ � ����� �� ---

            // 1. ������� ������ ��������� ����� ������ INSERT-������
            // ��������� ����� 20 ���
            long newCharacterId = CreateNewCharacter("Arion", 20, "Male", false);

            if (newCharacterId > 0)
            {
                // 2. ���������, ����� �� ��� 20-������ �������� ������ ������, ��������� ������ SELECT-������
                CheckQuestAvailability(newCharacterId, 1); // ����� � id=1 (��������� 10 ���)
                CheckQuestAvailability(newCharacterId, 2); // ����� � id=2 (��������� 25 ���)

                // 3. �������� ��������� �����, ������� ��� ��������
                AttemptToCompleteQuest(newCharacterId, 1);

                // 4. �������� ��������� �����, ������� ��� �� ��������
                AttemptToCompleteQuest(newCharacterId, 2);
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"[MySQL Error] {ex.Number}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError("��������� ������: " + ex.Message);
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                Debug.Log("���������� �������.");
            }
        }
    }

    /// <summary>
    /// ������� ������ ��������� � ������� ������� SQL-������� INSERT.
    /// </summary>
    /// <returns>ID ������ ��������� ��� -1 � ������ ������.</returns>
    public long CreateNewCharacter(string name, int age, string gender, bool isNpc)
    {
        Debug.Log($"--- ������� ������� ��������� '{name}' (������� {age}) ---");
        try
        {
            // �� ���������� ��� �������: INSERT ��� �������� � SELECT LAST_INSERT_ID() ��� ��������� ��� ID.
            // ExecuteScalar ������ ��������� ������� �������.
            string sql = "INSERT INTO `Character` (name, age, gender, character_NPC) VALUES (@name, @age, @gender, @is_npc); SELECT LAST_INSERT_ID();";
            MySqlCommand cmd = new MySqlCommand(sql, connection);

            // ���������� ��������� ��� ������������ (������ �� SQL-��������)
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@age", age);
            cmd.Parameters.AddWithValue("@gender", gender);
            cmd.Parameters.AddWithValue("@is_npc", isNpc);

            // ExecuteScalar ��������� ������ � ���������� �������� �� ������ ������� ������ ������ ����������.
            // � ����� ������ ��� ����� ID ������ ���������.
            long newId = Convert.ToInt64(cmd.ExecuteScalar());

            Debug.Log($"�������� '{name}' ������� ������ � ID: {newId}");
            return newId;
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"�� ������� ������� ���������. ������ �� ��: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// ��������� ����������� ������, �������� ������ ������� CanStartQuest �������� � C#.
    /// </summary>
    public bool CheckQuestAvailability(long characterId, int questId)
    {
        Debug.Log($"--- �������� ����������� ������ ID={questId} ��� ��������� ID={characterId} ---");
        bool canStart = false;

        // ���� ������ �������� ������� ��������� � ��������� ������� ��� ������ �� ���� ���.
        string sql = @"
            SELECT c.age, q.condition_of_age_start 
            FROM `Character` c, Quests q 
            WHERE c.id_character = @char_id AND q.id_quests = @quest_id";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@char_id", characterId);
        cmd.Parameters.AddWithValue("@quest_id", questId);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read()) // ���� ������ �������
            {
                int characterAge = reader.GetInt32("age");
                int requiredAge = reader.GetInt32("condition_of_age_start");

                if (characterAge >= requiredAge)
                {
                    canStart = true;
                }
            }
        } // reader ��������� ����� �������������

        Debug.Log($"����� �� �������� ������ �����? -> {canStart}");
        return canStart;
    }

    /// <summary>
    /// ��������� ������ ���������� ������, ������� �������� ������� � ����.
    /// </summary>
    public void AttemptToCompleteQuest(long characterId, int questId)
    {
        Debug.Log($"--- ������� ��������� ����� ID={questId} ��� ��������� ID={characterId} ---");

        // ��� 1: ���������, ����� �� �������� ��������� �����, ������ ��� C# �����.
        // ��� ������� �������������� ������ � ��, �� ��������� ������.
        bool canComplete = CheckQuestAvailability(characterId, questId);

        // ��� 2: ���� �������� ��������, ��������� "������" ������.
        if (canComplete)
        {
            // � �������� ������� ����� ��� �� INSERT � ��������� ��� UPDATE ����� ���������.
            // ������ �� ������ ������� ��������� �� ������.
            Debug.Log($"����� ID={questId} ������� �������� (������ ������� �����).");
        }
        else
        {
            // ���� �������� �� ��������, ������� ������, ��� ��� ����� SIGNAL SQLSTATE � ���������.
            Debug.LogError($"�� ������� ��������� ����� ID={questId}. �������: Character does not meet the age requirement for this quest.");
        }
    }

    /// <summary>
    /// �������� ������ ���������� � ���������� ��������� �� ��� ID.
    /// </summary>
    /// <returns>������ � ������� ��������� ��� null, ���� �� �� ������.</returns>
    public CharacterData GetCharacterDetails(long characterId)
    {
        Debug.Log($"--- ��������� ������ ��� ��������� ID={characterId} ---");
        CharacterData characterData = null;

        // ������ �������� ��� �������� ���� �� ������� Character
        string sql = "SELECT name, age, gender, character_NPC FROM `Character` WHERE id_character = @char_id";
        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@char_id", characterId);

        try
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read()) // ���� �������� ������
                {
                    characterData = new CharacterData
                    {
                        Name = reader.GetString("name"),
                        Age = reader.GetInt32("age"),
                        Gender = reader.GetString("gender"),
                        IsNpc = reader.GetBoolean("character_NPC")
                    };
                    Debug.Log($"������ ��������: {characterData.Name}, �������: {characterData.Age}");
                }
                else
                {
                    Debug.LogWarning($"�������� � ID={characterId} �� ������.");
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"������ ��� ��������� ������ ���������: {ex.Message}");
        }

        return characterData;
    }

    /// <summary>
    /// �������� ������ ���� �������, ������� �������� ��������� �� ��������.
    /// </summary>
    /// <returns>������ �������� ��������� �������.</returns>
    public List<string> GetAvailableQuestsForCharacter(long characterId)
    {
        Debug.Log($"--- ����� ��������� ������� ��� ��������� ID={characterId} ---");
        List<string> availableQuests = new List<string>();

        // ���� ������ ��������� ������� Character � Quests, ����� �������� ������� ���������
        // � ��������� ��������� ��� ������� ������.
        string sql = @"
            SELECT q.completion_condition 
            FROM `Character` c, Quests q 
            WHERE c.id_character = @char_id AND c.age >= q.condition_of_age_start";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@char_id", characterId);

        try
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read()) // �������� �� ���� ��������� �������
                {
                    availableQuests.Add(reader.GetString("completion_condition"));
                }
            }
            Debug.Log($"������� ��������� �������: {availableQuests.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"������ ��� ������ �������: {ex.Message}");
        }

        return availableQuests;
    }

    /// <summary>
    /// �������� ���������� � ������� �� ���������� ����������� ������.
    /// </summary>
    /// <returns>�������� ��������-������� ��� null, ���� ������� ��� ��� ����� �� ������.</returns>
    public string GetQuestRewardItemName(int questId)
    {
        Debug.Log($"--- ��������� ������� �� ����� ID={questId} ---");
        string rewardItemName = null;

        // ������ ��������� ������� Quests � Items �� �������� �����,
        // ����� �������� ��� �������� �� ������� Items �� ID, ���������� � ������.
        string sql = @"
            SELECT i.name 
            FROM Quests q
            JOIN Items i ON q.id_items = i.id_items
            WHERE q.id_quests = @quest_id";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@quest_id", questId);

        try
        {
            // ���������� ExecuteScalar, ��� ��� ��� ����� ������ ���� �������� (��� ��������)
            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                rewardItemName = Convert.ToString(result);
                Debug.Log($"������� �� ����� ID={questId}: '{rewardItemName}'");
            }
            else
            {
                Debug.LogWarning($"������� ��� ������ ID={questId} �� �������.");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"������ ��� ��������� �������: {ex.Message}");
        }

        return rewardItemName;
    }

    /// <summary>
    /// ������� ���� ���������� (NPC), ������� ��������� � ������������ ������� (���������).
    /// </summary>
    /// <returns>������ ���� NPC � ��������� �������.</returns>
    public List<string> FindNpcsByLocation(string locationName)
    {
        Debug.Log($"--- ����� NPC � �������: '{locationName}' ---");
        List<string> npcNames = new List<string>();

        // ���� ������ ��������� ������� Character � Checkpoint, ����� �������������
        // ���������� �� �������� �� �������� ���������.
        string sql = @"
            SELECT c.name 
            FROM `Character` c
            JOIN Checkpoint ch ON c.id_checkpoint = ch.id_checkpoint
            WHERE c.character_NPC = TRUE AND ch.name = @location_name";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@location_name", locationName);

        try
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    npcNames.Add(reader.GetString("name"));
                }
            }
            Debug.Log($"� ������� '{locationName}' ������� NPC: {npcNames.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"������ ��� ������ NPC: {ex.Message}");
        }

        return npcNames;
    }

    /// �������� ������ ���� �������, ������� ����� �������� � ��������� ��������.
    /// </summary>
    /// <returns>������ �������� �������, ��������� � ���������� ���������.</returns>
    public List<string> GetEventsLeadingToEnding(int endingId)
    {
        Debug.Log($"--- ����� �������, ������� � �������� ID={endingId} ---");
        List<string> eventDescriptions = new List<string>();

        // ���� ������ ��������� ������� Events � Answers, ����� ����� ��� �������,
        // ������ � ������� ������� � ��������� ID ��������.
        // DISTINCT ������������, ����� �������� ������������ �������, ���� ���������
        // ������� � ����� ������� ����� � ����� � ��� �� ��������.
        string sql = @"
            SELECT DISTINCT e.description 
            FROM Events e
            JOIN Answers a ON e.id_events = a.id_events
            WHERE a.id_endings = @ending_id";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ending_id", endingId);

        try
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read()) // �������� �� ���� ��������� ��������
                {
                    eventDescriptions.Add(reader.GetString("description"));
                }
            }
            Debug.Log($"������� �������, ������� � �������� ID={endingId}: {eventDescriptions.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"������ ��� ������ ������� ��� ��������: {ex.Message}");
        }

        return eventDescriptions;
    }

    void OnApplicationQuit()
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
        }
    }
}