-- PostgreSQL Database Setup for PEP Scanner
-- Run this script as postgres superuser

-- Create database
CREATE DATABASE pepscanner;
CREATE DATABASE pepscanner_dev;

-- Create user (optional - you can use existing postgres user)
-- CREATE USER pepscanner_user WITH PASSWORD 'your_secure_password';

-- Grant privileges
-- GRANT ALL PRIVILEGES ON DATABASE pepscanner TO pepscanner_user;
-- GRANT ALL PRIVILEGES ON DATABASE pepscanner_dev TO pepscanner_user;

-- Connect to the database and create extensions
\c pepscanner;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

\c pepscanner_dev;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";