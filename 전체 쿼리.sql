-- 데이터베이스 생성
CREATE DATABASE IF NOT EXISTS game_world;
USE game_world;

-- 테이블 생성
CREATE TABLE IF NOT EXISTS players (
    player_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    level INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE TABLE IF NOT EXISTS items (
    item_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    value INT DEFAULT 0
);

CREATE TABLE IF NOT EXISTS inventories (
    inventory_id INT AUTO_INCREMENT PRIMARY KEY,
    player_id INT,
    item_id INT,
    quantity INT DEFAULT 1,
    FOREIGN KEY (player_id) REFERENCES players(player_id),
    FOREIGN KEY (item_id) REFERENCES items(item_id)
);

CREATE TABLE IF NOT EXISTS quests (
    quest_id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    reward_exp INT DEFAULT 0,
    reward_item_id INT,
    FOREIGN KEY (reward_item_id) REFERENCES items(item_id)
);

CREATE TABLE IF NOT EXISTS player_quests (
    player_id INT,
    quest_id INT,
    status ENUM('시작', '진행중', '완료') DEFAULT '시작',
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    PRIMARY KEY (player_id, quest_id),
    FOREIGN KEY (player_id) REFERENCES players(player_id),
    FOREIGN KEY (quest_id) REFERENCES quests(quest_id)
);

-- 초기 데이터 삽입

-- 플레이어 데이터
INSERT INTO players (username, email, password_hash, level) VALUES
('hero123', 'hero123@email.com', 'hashed_password_1', 5),
('dragon_slayer', 'slayer@email.com', 'hashed_password_2', 7),
('magic_user', 'magic@email.com', 'hashed_password_3', 3),
('archer_pro', 'archer@email.com', 'hashed_password_4', 4),
('healer_main', 'healer@email.com', 'hashed_password_5', 6);

-- 아이템 데이터
INSERT INTO items (name, description, value) VALUES
('나무 검', '초보자용 검입니다.', 10),
('철 검', '일반적인 검입니다.', 50),
('미스릴 검', '희귀한 검입니다.', 200),
('가죽 갑옷', '기본적인 방어구입니다.', 30),
('철 갑옷', '튼튼한 갑옷입니다.', 100),
('빨간 포션', '체력을 회복합니다.', 15),
('파란 포션', '마나를 회복합니다.', 15),
('부활 스크롤', '죽었을 때 부활할 수 있습니다.', 500),
('텔레포트 스크롤', '마을로 돌아갑니다.', 50),
('행운의 반지', '아이템 드롭률이 증가합니다.', 300);

-- 인벤토리 데이터
INSERT INTO inventories (player_id, item_id, quantity) VALUES
(1, 1, 1),  -- hero123: 나무 검 1개
(1, 6, 5),  -- hero123: 빨간 포션 5개
(1, 7, 3),  -- hero123: 파란 포션 3개
(2, 2, 1),  -- dragon_slayer: 철 검 1개
(2, 5, 1),  -- dragon_slayer: 철 갑옷 1개
(2, 6, 10), -- dragon_slayer: 빨간 포션 10개
(3, 7, 15), -- magic_user: 파란 포션 15개
(3, 9, 5),  -- magic_user: 텔레포트 스크롤 5개
(4, 8, 1),  -- archer_pro: 부활 스크롤 1개
(5, 10, 1); -- healer_main: 행운의 반지 1개

-- 퀘스트 데이터
INSERT INTO quests (title, description, reward_exp, reward_item_id) VALUES
('첫 모험', '마을 주변의 몬스터를 3마리 처치하세요.', 100, 6),
('무기의 선택', '첫 번째 무기를 장만하세요.', 150, 1),
('포션 제작자', '포션 제작자를 도와 재료를 수집하세요.', 200, 7),
('던전 탐험', '첫 번째 던전을 클리어하세요.', 500, 2),
('고급 장비', '희귀한 장비를 획득하세요.', 1000, 3);

-- 플레이어 퀘스트 데이터
INSERT INTO player_quests (player_id, quest_id, status) VALUES
(1, 1, '완료'),
(1, 2, '진행중'),
(2, 1, '완료'),
(2, 2, '완료'),
(2, 3, '진행중'),
(3, 1, '진행중'),
(4, 1, '완료'),
(4, 2, '진행중'),
(5, 1, '완료'),
(5, 3, '진행중');

-- 완료된 퀘스트 완료 시간 업데이트
UPDATE player_quests 
SET completed_at = DATE_SUB(CURRENT_TIMESTAMP, INTERVAL FLOOR(RAND() * 10) DAY)
WHERE STATUS = '완료';


DELIMITER //
CREATE PROCEDURE buy_item(IN p_player_id INT, IN p_shop_id INT, IN p_item_id INT, IN p_quantity INT)
BEGIN
	DECLARE item_price INT;
	DECLARE total_price INT;
	DECLARE player_gold INT;
	
	-- 아이템 가격 확인 
	SELECT price INTO item_price FROM shop_inventory
	WHERE shop_id = p_shop_id AND item_id = p_item_id;

	SET total_price = item_price * p_quantity;
	
	-- 플레이어의 골드 확인
	SELECT gold INTO player_gold FROM player_wallet 
	WHERE player_id = p_player_id;
	
	-- 구매 가능 여부 확인 처리 
	IF player_gold >= total_price THEN
		--플레이어의 골드 차감 
		UPDATE player_wallet SET gold = gold - total_price
		WHERE player_id = p_player_id;
		
		-- 상점 재고 감소 
		UPDATE shop_inventory SET stock = stock - p_quantity
		WHERE shot_id = p_shop_id AND item_id = p_item_id;
		
		-- 플레이어 인벤토리에 아이템 추가 
		INSERT INTO inventories (player_id , item_id , quantity)
		VALUE (p_player_id , p_item_id, p_quantity)
		ON DUPLICATE KEY UPDATE quantity = quantity + p_quantity;
		
		SELECT 'Purchase successful' AS result;
		
	ELSE
		SELECT 'Insufficient gold' AS result;
	END IF;
END
DELIMITER //
