-- Create database if not exists
CREATE DATABASE IF NOT EXISTS modular_app;
USE modular_app;

-- Initial data for testing
-- Insert super admin user (password: Admin123!)
INSERT INTO Users (UserName, Email, PasswordHash, IsSuperAdmin, IsActive, CreatedAt, CreatedBy) 
VALUES ('admin', 'admin@modularhost.com', '$2a$11$rBNqZfgjCJGZpNMPV5pFpOH5XPqHGxpR3Jm6StGJYlqGhf5ksI2K6', 1, 1, NOW(), 'System');

-- Insert regular roles
INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy) VALUES
('Administrator', 'Full system access', 1, NOW(), 'System'),
('Editor', 'Can edit content', 1, NOW(), 'System'),
('Viewer', 'Read-only access', 1, NOW(), 'System');

-- Insert sample menus
INSERT INTO Menus (Title, Url, ParentId, `Order`, IsVisible, Icon, CreatedAt, CreatedBy) VALUES
('Dashboard', '/', NULL, 1, 1, 'fas fa-tachometer-alt', NOW(), 'System'),
('Users', NULL, NULL, 2, 1, 'fas fa-users', NOW(), 'System'),
('Manage Users', '/Users', 2, 1, 1, 'fas fa-user-cog', NOW(), 'System'),
('Roles', '/Roles', 2, 2, 1, 'fas fa-user-shield', NOW(), 'System'),
('Settings', '/Settings', NULL, 4, 1, 'fas fa-cog', NOW(), 'System'),
('Reports', '/Reports', NULL, 5, 1, 'fas fa-chart-bar', NOW(), 'System');

-- Insert sample role permissions
INSERT INTO RolePermissions (RoleId, Url, HttpMethod, Description, CreatedAt, CreatedBy) VALUES
(1, '/*', NULL, 'Full access to all URLs', NOW(), 'System'),
(2, '/Users', 'GET', 'View users list', NOW(), 'System'),
(3, '/', 'GET', 'View dashboard', NOW(), 'System'),
(3, '/Reports', 'GET', 'View reports', NOW(), 'System'),

-- Create indexes for better performance
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_username ON Users(UserName);
CREATE INDEX idx_logs_created ON Logs(CreatedAt);
CREATE INDEX idx_logsarchive_created ON LogArchives(CreatedAt);
CREATE INDEX idx_logsarchive_archived ON LogArchives(ArchivedAt);
CREATE INDEX idx_rolepermissions_roleid ON RolePermissions(RoleId);
CREATE INDEX idx_userroles_userid ON UserRoles(UserId);
CREATE INDEX idx_userroles_roleid ON UserRoles(RoleId);
CREATE INDEX idx_notifications_expires ON Notifications(ExpiresAt);