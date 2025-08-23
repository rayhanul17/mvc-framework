-- Add missing DueDate column to Tickets table if it doesn't exist
ALTER TABLE Tickets 
ADD COLUMN IF NOT EXISTS DueDate datetime(6) NULL;

-- Add any other potentially missing columns
ALTER TABLE Tickets 
ADD COLUMN IF NOT EXISTS Source varchar(100) NULL;

ALTER TABLE Tickets 
ADD COLUMN IF NOT EXISTS Tags varchar(500) NULL;