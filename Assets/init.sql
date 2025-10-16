-- Указываем, что работаем с базой данных, созданной через переменные окружения
USE game_database;

-- -----------------------------------------------------
-- Section 1: Создание таблиц (без изменений)
-- -----------------------------------------------------

CREATE TABLE Graphics (
    id_graphics INT AUTO_INCREMENT PRIMARY KEY,
    type_of_graphics VARCHAR(255),
    file_path VARCHAR(255),
    resolution VARCHAR(50),
    fps INT
);

CREATE TABLE Checkpoint (
    id_checkpoint INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255),
    location VARCHAR(255),
    description TEXT
);

CREATE TABLE Management (
    id_management INT AUTO_INCREMENT PRIMARY KEY,
    action VARCHAR(255),
    button VARCHAR(50)
);

CREATE TABLE Items (
    id_items INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255),
    type_of_item VARCHAR(255),
    description_of_item TEXT,
    characteristic VARCHAR(255)
);

CREATE TABLE Endings (
    id_endings INT AUTO_INCREMENT PRIMARY KEY,
    type VARCHAR(255),
    `condition` VARCHAR(255),
    description TEXT
);

CREATE TABLE `Character` (
    id_character INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255),
    age INT,
    gender VARCHAR(50),
    character_NPC BOOLEAN,
    id_graphics INT,
    id_management INT,
    id_checkpoint INT,
    FOREIGN KEY (id_graphics) REFERENCES Graphics(id_graphics),
    FOREIGN KEY (id_management) REFERENCES Management(id_management),
    FOREIGN KEY (id_checkpoint) REFERENCES Checkpoint(id_checkpoint)
);

CREATE TABLE Events (
    id_events INT AUTO_INCREMENT PRIMARY KEY,
    type_event VARCHAR(255),
    description TEXT,
    choice VARCHAR(255),
    id_character INT,
    FOREIGN KEY (id_character) REFERENCES `Character`(id_character)
);

CREATE TABLE Quests (
    id_quests INT AUTO_INCREMENT PRIMARY KEY,
    condition_of_age_start INT,
    completion_condition VARCHAR(255),
    reward VARCHAR(255),
    id_items INT,
    FOREIGN KEY (id_items) REFERENCES Items(id_items)
);

CREATE TABLE Answers (
    id_answers INT AUTO_INCREMENT PRIMARY KEY,
    text TEXT,
    choice VARCHAR(255),
    id_events INT,
    id_quests INT,
    id_endings INT,
    FOREIGN KEY (id_events) REFERENCES Events(id_events),
    FOREIGN KEY (id_quests) REFERENCES Quests(id_quests),
    FOREIGN KEY (id_endings) REFERENCES Endings(id_endings)
);

-- -----------------------------------------------------
-- Section 2: Триггеры, функции и процедуры
-- -----------------------------------------------------

-- Изменение разделителя команд для создания сложных объектов
DELIMITER $$

-- -----------------------------------------------------
-- Триггер
-- -----------------------------------------------------

-- Триггер 1: Проверка возраста персонажа перед созданием или обновлением.
-- Запрещает установку отрицательного или нулевого возраста для обеспечения целостности данных.
CREATE TRIGGER before_character_insert_update
BEFORE INSERT ON `Character`
FOR EACH ROW
BEGIN
    IF NEW.age <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Character age must be positive.';
    END IF;
END$$

-- -----------------------------------------------------
-- Функция
-- -----------------------------------------------------

-- Функция 1: Проверка, может ли персонаж начать квест.
-- Возвращает TRUE, если возраст персонажа соответствует условию квеста, иначе FALSE.
-- Полезна для вызова как из других процедур, так и напрямую из кода приложения.
CREATE FUNCTION CanStartQuest(
    p_character_id INT,
    p_quest_id INT
)
RETURNS BOOLEAN
DETERMINISTIC
BEGIN
    DECLARE character_age INT;
    DECLARE quest_req_age INT;

    SELECT age INTO character_age FROM `Character` WHERE id_character = p_character_id;
    SELECT condition_of_age_start INTO quest_req_age FROM Quests WHERE id_quests = p_quest_id;

    IF character_age >= quest_req_age THEN
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END$$

-- -----------------------------------------------------
-- Хранимые процедуры
-- -----------------------------------------------------

-- Процедура 1: Безопасное создание нового персонажа.
-- Инкапсулирует логику вставки и возвращает ID нового персонажа.
-- Это упрощает код приложения, так как ему нужно вызвать только одну процедуру.
CREATE PROCEDURE CreateCharacter(
    IN p_name VARCHAR(255),
    IN p_age INT,
    IN p_gender VARCHAR(50),
    IN p_is_npc BOOLEAN,
    OUT p_new_character_id INT
)
BEGIN
    INSERT INTO `Character` (name, age, gender, character_NPC)
    VALUES (p_name, p_age, p_gender, p_is_npc);

    SET p_new_character_id = LAST_INSERT_ID();
END$$

-- Процедура 2: Логика завершения квеста.
-- Проверяет условия и служит каркасом для добавления логики выдачи наград.
CREATE PROCEDURE CompleteQuest(
    IN p_character_id INT,
    IN p_quest_id INT
)
BEGIN
    DECLARE reward_item_id INT;

    -- Шаг 1: Проверяем, может ли персонаж вообще начать этот квест, используя нашу функцию.
    IF NOT CanStartQuest(p_character_id, p_quest_id) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Character does not meet the age requirement for this quest.';
    END IF;

    -- Шаг 2: Определяем награду.
    SELECT id_items INTO reward_item_id FROM Quests WHERE id_quests = p_quest_id;

    -- Шаг 3: Логика выдачи награды.
    -- В реальном проекте здесь была бы вставка в таблицу инвентаря персонажа.
    -- Например: INSERT INTO Character_Inventory (id_character, id_item) VALUES (p_character_id, reward_item_id);
    -- Сейчас это место оставлено для будущей реализации.

END$$

-- Возвращаем стандартный разделитель
DELIMITER ;