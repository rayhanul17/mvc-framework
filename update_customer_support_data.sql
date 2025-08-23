-- Update Customer Support Menus and Permissions
-- This script updates the Customer Support area with correct menus and permissions

-- Step 1: Delete existing Customer Support sub-menus
DELETE FROM RoleMenus WHERE MenuId IN (
    SELECT Id FROM Menus WHERE ParentId = (SELECT Id FROM Menus WHERE Name = 'CustomerSupport')
);

DELETE FROM Menus WHERE ParentId = (SELECT Id FROM Menus WHERE Name = 'CustomerSupport');

-- Step 2: Insert new Customer Support sub-menus
SET @CustomerSupportMenuId = (SELECT Id FROM Menus WHERE Name = 'CustomerSupport');

INSERT INTO Menus (Name, DisplayName, Area, Controller, Action, Url, ActiveMenuUrl, Icon, ParentId, `Order`, IsActive, AllowAnonymous, RequireAuthentication)
VALUES 
    ('SupportHome', 'Overview', 'CustomerSupport', 'Home', 'Index', '/CustomerSupport/Home', '/CustomerSupport/Home', 'fas fa-home', @CustomerSupportMenuId, 1, 1, 0, 1),
    ('SupportDashboard', 'Support Dashboard', 'CustomerSupport', 'Dashboard', 'Index', '/CustomerSupport/Dashboard', '/CustomerSupport/Dashboard', 'fas fa-tachometer-alt', @CustomerSupportMenuId, 2, 1, 0, 1),
    ('TicketManagement', 'Tickets', 'CustomerSupport', 'Ticket', 'Index', '/CustomerSupport/Ticket', '/CustomerSupport/Ticket', 'fas fa-ticket-alt', @CustomerSupportMenuId, 3, 1, 0, 1),
    ('ManageTickets', 'Manage Tickets', 'CustomerSupport', 'ManageTicket', 'Index', '/CustomerSupport/ManageTicket', '/CustomerSupport/ManageTicket', 'fas fa-tasks', @CustomerSupportMenuId, 4, 1, 0, 1),
    ('SupportTickets', 'My Support Tickets', 'CustomerSupport', 'SupportTicket', 'MyTickets', '/CustomerSupport/SupportTicket/MyTickets', '/CustomerSupport/SupportTicket', 'fas fa-user-circle', @CustomerSupportMenuId, 5, 1, 0, 1),
    ('TicketDashboard', 'Analytics', 'CustomerSupport', 'Ticket', 'Dashboard', '/CustomerSupport/Ticket/Dashboard', '/CustomerSupport/Ticket/Dashboard', 'fas fa-chart-pie', @CustomerSupportMenuId, 6, 1, 0, 1),
    ('SupportConfiguration', 'Configuration', 'CustomerSupport', 'Configuration', 'Index', '/CustomerSupport/Configuration', '/CustomerSupport/Configuration', 'fas fa-cog', @CustomerSupportMenuId, 7, 1, 0, 1);

-- Step 3: Add missing permissions for SupportTicket controller if they don't exist
INSERT INTO Permissions (Name, Area, Controller, Action, Description, IsActive, CreatedAt, CreatedBy)
SELECT * FROM (
    SELECT 'Support.Ticket.Details' AS Name, 'CustomerSupport' AS Area, 'Ticket' AS Controller, 'Details' AS Action, 'View ticket details' AS Description, 1 AS IsActive, NOW() AS CreatedAt, 'System' AS CreatedBy
    UNION ALL
    SELECT 'Support.ManageTicket.Statistics', 'CustomerSupport', 'ManageTicket', 'Statistics', 'View ticket statistics', 1, NOW(), 'System'
    UNION ALL
    SELECT 'Support.ManageTicket.Unassigned', 'CustomerSupport', 'ManageTicket', 'Unassigned', 'View unassigned tickets', 1, NOW(), 'System'
    UNION ALL
    SELECT 'Support.ManageTicket.Details', 'CustomerSupport', 'ManageTicket', 'Details', 'View ticket details', 1, NOW(), 'System'
    UNION ALL
    SELECT 'Support.SupportTicket.MyTickets', 'CustomerSupport', 'SupportTicket', 'MyTickets', 'View my support tickets', 1, NOW(), 'System'
    UNION ALL
    SELECT 'Support.SupportTicket.Details', 'CustomerSupport', 'SupportTicket', 'Details', 'View support ticket details', 1, NOW(), 'System'
    UNION ALL
    SELECT 'Support.SupportTicket.Resolve', 'CustomerSupport', 'SupportTicket', 'Resolve', 'Resolve support tickets', 1, NOW(), 'System'
) AS new_permissions
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.Name = new_permissions.Name
);

-- Step 4: Clear permission cache (this needs to be done in the application)
-- The application should clear its permission cache after running this script

SELECT 'Customer Support menus and permissions updated successfully' AS Result;