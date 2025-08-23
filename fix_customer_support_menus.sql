-- Fix Customer Support Menus
-- This script will delete existing outdated Customer Support submenus and add the correct ones

-- First, let's identify the CustomerSupport parent menu
DECLARE @CustomerSupportMenuId INT;
SELECT @CustomerSupportMenuId = Id FROM Menus WHERE Name = 'CustomerSupport';

-- Delete existing Customer Support sub-menus
DELETE FROM RoleMenus WHERE MenuId IN (
    SELECT Id FROM Menus WHERE ParentId = @CustomerSupportMenuId
);

DELETE FROM Menus WHERE ParentId = @CustomerSupportMenuId;

-- Add new Customer Support sub-menus
IF @CustomerSupportMenuId IS NOT NULL
BEGIN
    INSERT INTO Menus (Name, DisplayName, Area, Controller, Action, Url, ActiveMenuUrl, Icon, ParentId, [Order], IsActive, AllowAnonymous, RequireAuthentication)
    VALUES 
        ('SupportHome', 'Overview', 'CustomerSupport', 'Home', 'Index', '/CustomerSupport/Home', '/CustomerSupport/Home', 'fas fa-tachometer-alt', @CustomerSupportMenuId, 1, 1, 0, 1),
        ('SupportTickets', 'Ticket Management', 'CustomerSupport', 'Ticket', 'Index', '/CustomerSupport/Ticket', '/CustomerSupport/Ticket', 'fas fa-ticket-alt', @CustomerSupportMenuId, 2, 1, 0, 1),
        ('TicketDashboard', 'Ticket Dashboard', 'CustomerSupport', 'Ticket', 'Dashboard', '/CustomerSupport/Ticket/Dashboard', '/CustomerSupport/Ticket/Dashboard', 'fas fa-chart-pie', @CustomerSupportMenuId, 3, 1, 0, 1),
        ('SupportConfiguration', 'Configuration', 'CustomerSupport', 'Configuration', 'Index', '/CustomerSupport/Configuration', '/CustomerSupport/Configuration', 'fas fa-cog', @CustomerSupportMenuId, 4, 1, 0, 1);
    
    PRINT 'Customer Support menus updated successfully';
END
ELSE
BEGIN
    PRINT 'CustomerSupport parent menu not found';
END