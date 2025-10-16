using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    private string server = "127.0.0.1";
    private string database = "game_database";
    private string user = "game_user";
    private string password = "your_strong_user_password"; // <-- ЗАМЕНИТЕ НА ВАШ ПАРОЛЬ

    private MySqlConnection connection;

    void Start()
    {
        // Строка подключения с параметрами, которые решают проблемы совместимости
        string connectionString = $"Server={server};Database={database};User={user};Password={password};";
        connection = new MySqlConnection(connectionString);

        try
        {
            Debug.Log("Подключаемся к MySQL...");
            connection.Open();
            Debug.Log("Соединение успешно установлено!");

            // --- Демонстрация работы с вашей БД ---

            // 1. Создаем нового персонажа через прямой INSERT-запрос
            // Персонажу будет 20 лет
            long newCharacterId = CreateNewCharacter("Arion", 20, "Male", false);

            if (newCharacterId > 0)
            {
                // 2. Проверяем, может ли наш 20-летний персонаж начать квесты, используя прямой SELECT-запрос
                CheckQuestAvailability(newCharacterId, 1); // Квест с id=1 (требуется 10 лет)
                CheckQuestAvailability(newCharacterId, 2); // Квест с id=2 (требуется 25 лет)

                // 3. Пытаемся завершить квест, который ему доступен
                AttemptToCompleteQuest(newCharacterId, 1);

                // 4. Пытаемся завершить квест, который ему НЕ доступен
                AttemptToCompleteQuest(newCharacterId, 2);
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"[MySQL Error] {ex.Number}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Произошла ошибка: " + ex.Message);
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                Debug.Log("Соединение закрыто.");
            }
        }
    }

    /// <summary>
    /// Создает нового персонажа с помощью прямого SQL-запроса INSERT.
    /// </summary>
    /// <returns>ID нового персонажа или -1 в случае ошибки.</returns>
    public long CreateNewCharacter(string name, int age, string gender, bool isNpc)
    {
        Debug.Log($"--- Попытка создать персонажа '{name}' (возраст {age}) ---");
        try
        {
            // Мы объединяем два запроса: INSERT для создания и SELECT LAST_INSERT_ID() для получения его ID.
            // ExecuteScalar вернет результат второго запроса.
            string sql = "INSERT INTO `Character` (name, age, gender, character_NPC) VALUES (@name, @age, @gender, @is_npc); SELECT LAST_INSERT_ID();";
            MySqlCommand cmd = new MySqlCommand(sql, connection);

            // Используем параметры для безопасности (защита от SQL-инъекций)
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@age", age);
            cmd.Parameters.AddWithValue("@gender", gender);
            cmd.Parameters.AddWithValue("@is_npc", isNpc);

            // ExecuteScalar выполняет запрос и возвращает значение из первой колонки первой строки результата.
            // В нашем случае это будет ID нового персонажа.
            long newId = Convert.ToInt64(cmd.ExecuteScalar());

            Debug.Log($"Персонаж '{name}' успешно создан с ID: {newId}");
            return newId;
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Не удалось создать персонажа. Ошибка от БД: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Проверяет доступность квеста, выполняя логику функции CanStartQuest напрямую в C#.
    /// </summary>
    public bool CheckQuestAvailability(long characterId, int questId)
    {
        Debug.Log($"--- Проверка доступности квеста ID={questId} для персонажа ID={characterId} ---");
        bool canStart = false;

        // Этот запрос получает возраст персонажа и требуемый возраст для квеста за один раз.
        string sql = @"
            SELECT c.age, q.condition_of_age_start 
            FROM `Character` c, Quests q 
            WHERE c.id_character = @char_id AND q.id_quests = @quest_id";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@char_id", characterId);
        cmd.Parameters.AddWithValue("@quest_id", questId);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read()) // Если запись найдена
            {
                int characterAge = reader.GetInt32("age");
                int requiredAge = reader.GetInt32("condition_of_age_start");

                if (characterAge >= requiredAge)
                {
                    canStart = true;
                }
            }
        } // reader закроется здесь автоматически

        Debug.Log($"Может ли персонаж начать квест? -> {canStart}");
        return canStart;
    }

    /// <summary>
    /// Выполняет логику завершения квеста, сначала проверяя условия в коде.
    /// </summary>
    public void AttemptToCompleteQuest(long characterId, int questId)
    {
        Debug.Log($"--- Попытка завершить квест ID={questId} для персонажа ID={characterId} ---");

        // Шаг 1: Проверяем, может ли персонаж выполнить квест, вызвав наш C# метод.
        // Это создает дополнительный запрос к БД, но разделяет логику.
        bool canComplete = CheckQuestAvailability(characterId, questId);

        // Шаг 2: Если проверка пройдена, выполняем "логику" квеста.
        if (canComplete)
        {
            // В реальном проекте здесь был бы INSERT в инвентарь или UPDATE счета персонажа.
            // Сейчас мы просто выводим сообщение об успехе.
            Debug.Log($"Квест ID={questId} успешно завершен (логика награды здесь).");
        }
        else
        {
            // Если проверка не пройдена, выводим ошибку, как это делал SIGNAL SQLSTATE в процедуре.
            Debug.LogError($"Не удалось завершить квест ID={questId}. Причина: Character does not meet the age requirement for this quest.");
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