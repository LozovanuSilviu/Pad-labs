-- Database: renting_pad_lab

DROP DATABASE IF EXISTS renting_reservation_pad_lab;

CREATE DATABASE renting_reservation_pad_lab
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

\c renting_reservation_pad_lab;

CREATE SCHEMA IF NOT EXISTS rentings_reservations;

--------------------------------

CREATE TABLE rentings_reservations."Rentings"
(
    "leaseId" uuid               NOT NULL,
    "leaseStartDate" date        NOT NULL,
    "returnDate"  date           NOT NULL,
    "bookId"  uuid               NOT NULL,
    "customerName" varchar(255)  NOT NULL,
    PRIMARY KEY ("leaseId")
) TABLESPACE pg_default;

CREATE TABLE rentings_reservations."Reservations"
(
    "reservationId" uuid        NOT NULL,
    "bookId" uuid               NOT NULL,
    "reservedUntil" date        NOT NULL,
    "customerName" varchar(255) NOT NULL,
    PRIMARY KEY ("reservationId")
) TABLESPACE pg_default;
