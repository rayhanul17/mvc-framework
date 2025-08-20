-- Update Dashboard menu to point to correct action
UPDATE Menus 
SET Action = 'Dashboard' 
WHERE Name = 'Dashboard' 
  AND Controller = 'Home' 
  AND Action = 'Index';

-- Verify the update
SELECT * FROM Menus WHERE Name = 'Dashboard';