-- Database: inventory_pad_lab

-- Drop the database if it exists
DROP DATABASE IF EXISTS inventory_pad_lab;

-- Create the database with specified settings
CREATE DATABASE inventory_pad_lab
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- Connect to the newly created database
\c inventory_pad_lab;

-- Create the "inventory" schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS inventory;

-- Create the "Books" table
CREATE TABLE inventory."Books"
(
    "bookName" varchar(255)  NOT NULL,
    "bookAuthor" varchar(255) NOT NULL,
    "availableCount" int NOT NULL,
    "bookId" uuid NOT NULL,
    "libraryId" uuid NOT NULL,
    "reservedCount" int NOT NULL
) TABLESPACE pg_default;

