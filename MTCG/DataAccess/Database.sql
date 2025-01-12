-- Erstellen der Tabelle für Benutzer
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    coins INT DEFAULT 20,
    games_played INT DEFAULT 0,
    games_won INT DEFAULT 0,
    elo INT DEFAULT 100,
    created_at TIMESTAMP DEFAULT NOW()
);
-- Erstellen der Tabelle für Karten
CREATE TABLE card_types (
    id SERIAL PRIMARY KEY,
    type_name VARCHAR(255) UNIQUE NOT NULL
);

INSERT INTO card_types (type_name) VALUES
    ('monster'),
    ('spell');

CREATE TABLE cards (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    damage FLOAT NOT NULL,
    type VARCHAR(255) REFERENCES card_types(type_name) ON DELETE CASCADE,
    owner_id INT REFERENCES users(id) ON DELETE SET NULL
);
-- Erstellen der Tabelle für Pakete
CREATE TABLE packages (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Tabelle zur Zuordnung von Karten zu Paketen
CREATE TABLE package_cards (
    package_id INT REFERENCES packages(id) ON DELETE CASCADE,
    card_id UUID REFERENCES cards(id) ON DELETE CASCADE,
    PRIMARY KEY (package_id, card_id)
);

-- Erstellen der Tabelle für Decks
CREATE TABLE decks (
    id SERIAL PRIMARY KEY,
    user_id INT UNIQUE REFERENCES users(id) ON DELETE CASCADE
);

-- Tabelle zur Zuordnung von Karten zu Decks
CREATE TABLE deck_cards (
    deck_id INT REFERENCES decks(id) ON DELETE CASCADE,
    card_id UUID REFERENCES cards(id) ON DELETE CASCADE,
    PRIMARY KEY (deck_id, card_id)
);

-- Erstellen der Tabelle für Handelsangebote
CREATE TABLE trades (
    id UUID PRIMARY KEY,
    user_id INT REFERENCES users(id) ON DELETE CASCADE,
    card_to_trade UUID REFERENCES cards(id) ON DELETE CASCADE,
    wanted_type VARCHAR(255) REFERENCES card_types(type_name) ON DELETE CASCADE,
    minimum_damage FLOAT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);
-- Erstellen der Tabelle für Kämpfe
CREATE TABLE battles (
    id SERIAL PRIMARY KEY,
    player1_id INT REFERENCES users(id) ON DELETE CASCADE,
    player2_id INT REFERENCES users(id) ON DELETE CASCADE,
    winner_id INT REFERENCES users(id),
    battle_log TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Tabelle zur Zuordnung von Karten zu einem Kampf (für den Verlauf)
CREATE TABLE battle_cards (
    battle_id INT REFERENCES battles(id) ON DELETE CASCADE,
    card_id UUID REFERENCES cards(id) ON DELETE CASCADE,
    player_id INT REFERENCES users(id) ON DELETE CASCADE,
    PRIMARY KEY (battle_id, card_id, player_id)
);



-- Erstellen der Benutzer
INSERT INTO users (username, password, coins) VALUES ('user1', 'password1', 20);
INSERT INTO users (username, password, coins) VALUES ('user2', 'password2', 20);
INSERT INTO users (username, password, coins) VALUES ('admin', 'adminpass', 20);


-- Monster-Karten
INSERT INTO cards (id, name, damage, type) VALUES
  (gen_random_uuid(), 'Fire Goblin', 10.0, 'monster'),
  (gen_random_uuid(), 'Dragon', 50.0, 'monster'),
  (gen_random_uuid(), 'Knight', 15.0, 'monster'),
  (gen_random_uuid(), 'Ice Troll', 30.0, 'monster'),
  (gen_random_uuid(), 'Fire Elf', 22.0, 'monster'),
  (gen_random_uuid(), 'Earth Elemental', 40.0, 'monster'),
  (gen_random_uuid(), 'Water Elemental', 18.0, 'monster'),
  (gen_random_uuid(), 'Stone Giant', 45.0, 'monster'),
  (gen_random_uuid(), 'Shadow Assassin', 42.0, 'monster'),
  (gen_random_uuid(), 'Phoenix', 55.0, 'monster'),
  (gen_random_uuid(), 'Hydra', 48.0, 'monster'),
  (gen_random_uuid(), 'Lava Golem', 50.0, 'monster'),
  (gen_random_uuid(), 'Wind Dancer', 19.0, 'monster'),
  (gen_random_uuid(), 'Dark Knight', 47.0, 'monster'),
  (gen_random_uuid(), 'Mystic Archer', 36.0, 'monster');

-- Spell-Karten
INSERT INTO cards (id, name, damage, type) VALUES
  (gen_random_uuid(), 'Water Spell', 20.0, 'spell'),
  (gen_random_uuid(), 'Magic Sword', 25.0, 'spell'),
  (gen_random_uuid(), 'Lightning Bolt', 35.0, 'spell'),
  (gen_random_uuid(), 'Fireball', 28.0, 'spell'),
  (gen_random_uuid(), 'Wind Spirit', 32.0, 'spell'),
  (gen_random_uuid(), 'Thunder Strike', 37.0, 'spell'),
  (gen_random_uuid(), 'Solar Flare', 38.0, 'spell'),
  (gen_random_uuid(), 'Moonlight Mage', 26.0, 'spell'),
  (gen_random_uuid(), 'Electric Surge', 33.0, 'spell'),
  (gen_random_uuid(), 'Ice Barrier', 29.0, 'spell'),
  (gen_random_uuid(), 'Arcane Blast', 34.0, 'spell'),
  (gen_random_uuid(), 'Inferno Dragon', 60.0, 'spell'),
  (gen_random_uuid(), 'Ocean Wave', 31.0, 'spell'),
  (gen_random_uuid(), 'Crystal Guardian', 44.0, 'spell'),
  (gen_random_uuid(), 'Lightning Cloud', 30.0, 'spell');


  -- Erstellen der Packages
INSERT INTO packages DEFAULT VALUES; -- Package 1
INSERT INTO packages DEFAULT VALUES; -- Package 2

-- Karten in Package 1 hinzufügen
INSERT INTO package_cards (package_id, card_id) VALUES
  (1, (SELECT id FROM cards WHERE name = 'Fire Goblin')),
  (1, (SELECT id FROM cards WHERE name = 'Dragon')),
  (1, (SELECT id FROM cards WHERE name = 'Water Spell')),
  (1, (SELECT id FROM cards WHERE name = 'Ice Troll')),
  (1, (SELECT id FROM cards WHERE name = 'Magic Sword'));

-- Karten in Package 2 hinzufügen
INSERT INTO package_cards (package_id, card_id) VALUES
  (2, (SELECT id FROM cards WHERE name = 'Knight')),
  (2, (SELECT id FROM cards WHERE name = 'Fire Elf')),
  (2, (SELECT id FROM cards WHERE name = 'Lightning Bolt')),
  (2, (SELECT id FROM cards WHERE name = 'Stone Giant')),
  (2, (SELECT id FROM cards WHERE name = 'Fireball'));


  -- Deck für user1 erstellen
INSERT INTO decks (user_id) VALUES ((SELECT id FROM users WHERE username = 'user1'));

-- Karten zu user1's Deck hinzufügen
INSERT INTO deck_cards (deck_id, card_id) VALUES
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user1')), (SELECT id FROM cards WHERE name = 'Fire Goblin')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user1')), (SELECT id FROM cards WHERE name = 'Dragon')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user1')), (SELECT id FROM cards WHERE name = 'Water Spell')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user1')), (SELECT id FROM cards WHERE name = 'Ice Troll'));


  -- Deck für user2 erstellen
INSERT INTO decks (user_id) VALUES ((SELECT id FROM users WHERE username = 'user2'));

-- Karten zu user2's Deck hinzufügen
INSERT INTO deck_cards (deck_id, card_id) VALUES
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user2')), (SELECT id FROM cards WHERE name = 'Knight')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user2')), (SELECT id FROM cards WHERE name = 'Fire Elf')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user2')), (SELECT id FROM cards WHERE name = 'Lightning Bolt')),
  ((SELECT id FROM decks WHERE user_id = (SELECT id FROM users WHERE username = 'user2')), (SELECT id FROM cards WHERE name = 'Stone Giant'));