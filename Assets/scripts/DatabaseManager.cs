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

    /// <summary>
    /// Получает полную информацию о конкретном персонаже по его ID.
    /// </summary>
    /// <returns>Объект с данными персонажа или null, если он не найден.</returns>
    public CharacterData GetCharacterDetails(long characterId)
    {
        Debug.Log($"--- Получение данных для персонажа ID={characterId} ---");
        CharacterData characterData = null;

        // Запрос выбирает все основные поля из таблицы Character
        string sql = "SELECT name, age, gender, character_NPC FROM `Character` WHERE id_character = @char_id";
        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@char_id", characterId);

        try
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read()) // Если персонаж найден
                {
                    characterData = new CharacterData
                    {
                        Name = reader.GetString("name"),
                        Age = reader.GetInt32("age"),
                        Gender = reader.GetString("gender"),
                        IsNpc = reader.GetBoolean("character_NPC")
                    };
                    Debug.Log($"Найден персонаж: {characterData.Name}, возраст: {characterData.Age}");
                }
                else
                {
                    Debug.LogWarning($"Персонаж с ID={characterId} не найден.");
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Ошибка при получении данных персонажа: {ex.Message}");
        }

        return characterData;
    }

    /// <summary>
    /// Получает список всех квестов, которые доступны персонажу по возрасту.
    /// </summary>
    /// <returns>Список названий доступных квестов.</returns>
    public List<string> GetAvailableQuestsForCharacter(long characterId)
    {
        Debug.Log($"--- Поиск доступных квестов для персонажа ID={characterId} ---");
        List<string> availableQuests = new List<string>();

        // Этот запрос соединяет таблицы Character и Quests, чтобы сравнить возраст персонажа
        // с требуемым возрастом для каждого квеста.
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
                while (reader.Read()) // Проходим по всем найденным квестам
                {
                    availableQuests.Add(reader.GetString("completion_condition"));
                }
            }
            Debug.Log($"Найдено доступных квестов: {availableQuests.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Ошибка при поиске квестов: {ex.Message}");
        }

        return availableQuests;
    }

    /// <summary>
    /// Получает информацию о награде за выполнение конкретного квеста.
    /// </summary>
    /// <returns>Название предмета-награды или null, если награды нет или квест не найден.</returns>
    public string GetQuestRewardItemName(int questId)
    {
        Debug.Log($"--- Получение награды за квест ID={questId} ---");
        string rewardItemName = null;

        // Запрос соединяет таблицы Quests и Items по внешнему ключу,
        // чтобы получить имя предмета из таблицы Items по ID, указанному в квесте.
        string sql = @"
            SELECT i.name 
            FROM Quests q
            JOIN Items i ON q.id_items = i.id_items
            WHERE q.id_quests = @quest_id";

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@quest_id", questId);

        try
        {
            // Используем ExecuteScalar, так как нам нужно только одно значение (имя предмета)
            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                rewardItemName = Convert.ToString(result);
                Debug.Log($"Награда за квест ID={questId}: '{rewardItemName}'");
            }
            else
            {
                Debug.LogWarning($"Награда для квеста ID={questId} не найдена.");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Ошибка при получении награды: {ex.Message}");
        }

        return rewardItemName;
    }

    /// <summary>
    /// Находит всех персонажей (NPC), которые находятся в определенной локации (чекпоинте).
    /// </summary>
    /// <returns>Список имен NPC в указанной локации.</returns>
    public List<string> FindNpcsByLocation(string locationName)
    {
        Debug.Log($"--- Поиск NPC в локации: '{locationName}' ---");
        List<string> npcNames = new List<string>();

        // Этот запрос соединяет таблицы Character и Checkpoint, чтобы отфильтровать
        // персонажей по названию их текущего чекпоинта.
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
            Debug.Log($"В локации '{locationName}' найдено NPC: {npcNames.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Ошибка при поиске NPC: {ex.Message}");
        }

        return npcNames;
    }

    /// Получает список всех событий, которые могут привести к указанной концовке.
    /// </summary>
    /// <returns>Список описаний событий, связанных с конкретной концовкой.</returns>
    public List<string> GetEventsLeadingToEnding(int endingId)
    {
        Debug.Log($"--- Поиск событий, ведущих к концовке ID={endingId} ---");
        List<string> eventDescriptions = new List<string>();

        // Этот запрос соединяет таблицы Events и Answers, чтобы найти все события,
        // ответы в которых связаны с указанным ID концовки.
        // DISTINCT используется, чтобы избежать дублирования событий, если несколько
        // ответов в одном событии ведут к одной и той же концовке.
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
                while (reader.Read()) // Проходим по всем найденным событиям
                {
                    eventDescriptions.Add(reader.GetString("description"));
                }
            }
            Debug.Log($"Найдено событий, ведущих к концовке ID={endingId}: {eventDescriptions.Count}");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Ошибка при поиске событий для концовки: {ex.Message}");
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