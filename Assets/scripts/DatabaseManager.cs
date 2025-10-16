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

    void OnApplicationQuit()
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
        }
    }
}