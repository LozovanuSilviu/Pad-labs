
-- Drop the database if it exists
DROP DATABASE IF EXISTS identity;

-- Create the database with specified settings
CREATE DATABASE identity
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- Connect to the newly created database
\c identity;

-- Create the "identity" schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS identity;

-- Create the "Users" table
CREATE TABLE identity."Users"
(
    "userId" uuid  NOT NULL,
    "userName" varchar(255) NOT NULL,
    "idnp" bigint NOT NULL,
    "hashedPassword" varchar(255) NOT NULL,
    "email" varchar(255) NOT NULL,
    "userType" int NOT NULL,
    "libraryId" uuid NULL
    
) TABLESPACE pg_default;

CREATE TABLE identity."Libraries"
(
    "libraryId" uuid  NOT NULL,
    "name" varchar(255) NOT NULL,
    "address" varchar(255) NOT NULL
) TABLESPACE pg_default;

