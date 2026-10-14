-- DeluxeNET database setup
-- 1. Put your password below (use letters/numbers only)
-- 2. Run this file in HeidiSQL or any MySQL/MariaDB client as root

CREATE DATABASE IF NOT EXISTS deluxenet;
CREATE USER IF NOT EXISTS 'deluxe'@'localhost' IDENTIFIED BY 'pick-a-strong-password';
GRANT ALL PRIVILEGES ON deluxenet.* TO 'deluxe'@'localhost';
FLUSH PRIVILEGES;
