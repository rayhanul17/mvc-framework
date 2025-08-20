ALTER DATABASE CHARACTER SET utf8mb4;


CREATE TABLE `BlogCategories` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Slug` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DisplayOrder` int NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_BlogCategories` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `FileDocuments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FileName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OriginalFileName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FilePath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FileExtension` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FileSize` bigint NOT NULL,
    `ContentType` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Category` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Tags` longtext CHARACTER SET utf8mb4 NULL,
    `DownloadCount` int NOT NULL,
    `LastDownloadedAt` datetime(6) NULL,
    `UploadedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsPublic` tinyint(1) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_FileDocuments` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `Menus` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Area` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Controller` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Action` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Url` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Icon` varchar(50) CHARACTER SET utf8mb4 NULL,
    `ParentId` int NULL,
    `Order` int NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Menus` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Menus_Menus_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `Menus` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `Roles` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `IsActive` tinyint(1) NOT NULL,
    `Name` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`),
    CONSTRAINT `AK_Roles_Name` UNIQUE (`Name`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `SiteSettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Category` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ValidValues` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `IsRequired` tinyint(1) NOT NULL,
    `IsSystemSetting` tinyint(1) NOT NULL,
    `Order` int NOT NULL,
    `UpdatedBy` varchar(256) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_SiteSettings` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `Users` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FullName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `AvatarUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `IsActive` tinyint(1) NOT NULL,
    `IsSuperAdmin` tinyint(1) NOT NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `CustomerServiceRoleMappings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CustomerServiceRole` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `AspNetRoleName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `AspNetRoleId` varchar(450) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_CustomerServiceRoleMappings` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CustomerServiceRoleMappings_Roles_AspNetRoleName` FOREIGN KEY (`AspNetRoleName`) REFERENCES `Roles` (`Name`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_RoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RoleClaims_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RoleMenus` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `MenuId` int NOT NULL,
    `CanView` tinyint(1) NOT NULL,
    `CanCreate` tinyint(1) NOT NULL,
    `CanEdit` tinyint(1) NOT NULL,
    `CanDelete` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_RoleMenus` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RoleMenus_Menus_MenuId` FOREIGN KEY (`MenuId`) REFERENCES `Menus` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RoleMenus_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `BlogPosts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Slug` varchar(300) CHARACTER SET utf8mb4 NULL,
    `Summary` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FeaturedImageUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CategoryId` int NOT NULL,
    `IsPublished` tinyint(1) NOT NULL,
    `PublishedDate` datetime(6) NULL,
    `ViewCount` int NOT NULL,
    `Tags` varchar(500) CHARACTER SET utf8mb4 NULL,
    `MetaTitle` varchar(200) CHARACTER SET utf8mb4 NULL,
    `MetaDescription` varchar(500) CHARACTER SET utf8mb4 NULL,
    `MetaKeywords` varchar(500) CHARACTER SET utf8mb4 NULL,
    `AuthorId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `CommentsEnabled` tinyint(1) NOT NULL,
    `RequireCommentApproval` tinyint(1) NOT NULL,
    `CommentsVisible` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_BlogPosts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_BlogPosts_BlogCategories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `BlogCategories` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_BlogPosts_Users_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;


CREATE TABLE `LogArchives` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TableName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `EntityId` int NOT NULL,
    `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `OldValues` longtext CHARACTER SET utf8mb4 NULL,
    `NewValues` longtext CHARACTER SET utf8mb4 NULL,
    `Changes` varchar(500) CHARACTER SET utf8mb4 NULL,
    `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
    `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
    `LoggedAt` datetime(6) NOT NULL,
    `ArchivedAt` datetime(6) NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_LogArchives` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_LogArchives_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;


CREATE TABLE `Logs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TableName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `EntityId` int NOT NULL,
    `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `OldValues` TEXT CHARACTER SET utf8mb4 NULL,
    `NewValues` TEXT CHARACTER SET utf8mb4 NULL,
    `Changes` TEXT CHARACTER SET utf8mb4 NULL,
    `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
    `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
    `LoggedAt` datetime(6) NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Logs` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Logs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;


CREATE TABLE `Tickets` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Priority` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `TicketNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Category` varchar(100) CHARACTER SET utf8mb4 NULL,
    `SubCategory` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ResolutionNotes` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DueDate` datetime(6) NULL,
    `ResolvedAt` datetime(6) NULL,
    `ClosedAt` datetime(6) NULL,
    `AssignedAt` datetime(6) NULL,
    `ReopenedAt` datetime(6) NULL,
    `ResponseTimeHours` double NULL,
    `ResolutionTimeHours` double NULL,
    `CustomerId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `AssignedToId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `AssignedById` varchar(255) CHARACTER SET utf8mb4 NULL,
    `LastModifiedByUserId` longtext CHARACTER SET utf8mb4 NULL,
    `LastModifiedById` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Tickets` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Tickets_Users_AssignedById` FOREIGN KEY (`AssignedById`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Tickets_Users_AssignedToId` FOREIGN KEY (`AssignedToId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Tickets_Users_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Tickets_Users_LastModifiedById` FOREIGN KEY (`LastModifiedById`) REFERENCES `Users` (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `UserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserClaims_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `UserLogins` (
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_UserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_UserLogins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `UserRoles` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `UserTokens` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_UserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `Comments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AuthorName` longtext CHARACTER SET utf8mb4 NULL,
    `AuthorEmail` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `BlogPostId` int NOT NULL,
    `ParentCommentId` int NULL,
    `IsApproved` tinyint(1) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `ModeratorNotes` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
    `UserAgent` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Comments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Comments_BlogPosts_BlogPostId` FOREIGN KEY (`BlogPostId`) REFERENCES `BlogPosts` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Comments_Comments_ParentCommentId` FOREIGN KEY (`ParentCommentId`) REFERENCES `Comments` (`Id`),
    CONSTRAINT `FK_Comments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `TicketComments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Comment` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
    `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OldStatus` longtext CHARACTER SET utf8mb4 NULL,
    `NewStatus` longtext CHARACTER SET utf8mb4 NULL,
    `IsInternal` tinyint(1) NOT NULL,
    `IsSystemGenerated` tinyint(1) NOT NULL,
    `TicketId` int NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TicketComments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TicketComments_Tickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `Tickets` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketComments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `TicketHistory` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Action` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `OldValue` varchar(100) CHARACTER SET utf8mb4 NULL,
    `NewValue` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `TicketId` int NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TicketHistory` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TicketHistory_Tickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `Tickets` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketHistory_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `TicketNotifications` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Message` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsRead` tinyint(1) NOT NULL,
    `ReadAt` datetime(6) NULL,
    `TicketId` int NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TicketNotifications` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TicketNotifications_Tickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `Tickets` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketNotifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `CommentAttachments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CommentId` int NOT NULL,
    `FileName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `ContentType` varchar(100) CHARACTER SET utf8mb4 NULL,
    `FileSize` bigint NOT NULL,
    `UploadedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_CommentAttachments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CommentAttachments_Comments_CommentId` FOREIGN KEY (`CommentId`) REFERENCES `Comments` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `TicketAttachments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FileName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `FileSize` bigint NOT NULL,
    `FileType` varchar(50) CHARACTER SET utf8mb4 NULL,
    `ContentType` varchar(100) CHARACTER SET utf8mb4 NULL,
    `TicketId` int NULL,
    `CommentId` int NULL,
    `UploadedById` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `ModifiedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TicketAttachments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TicketAttachments_TicketComments_CommentId` FOREIGN KEY (`CommentId`) REFERENCES `TicketComments` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketAttachments_Tickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `Tickets` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketAttachments_Users_UploadedById` FOREIGN KEY (`UploadedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE INDEX `IX_BlogCategories_DisplayOrder` ON `BlogCategories` (`DisplayOrder`);


CREATE UNIQUE INDEX `IX_BlogCategories_Slug` ON `BlogCategories` (`Slug`);


CREATE INDEX `IX_BlogPosts_AuthorId` ON `BlogPosts` (`AuthorId`);


CREATE INDEX `IX_BlogPosts_CategoryId` ON `BlogPosts` (`CategoryId`);


CREATE INDEX `IX_BlogPosts_IsPublished` ON `BlogPosts` (`IsPublished`);


CREATE INDEX `IX_BlogPosts_PublishedDate` ON `BlogPosts` (`PublishedDate`);


CREATE UNIQUE INDEX `IX_BlogPosts_Slug` ON `BlogPosts` (`Slug`);


CREATE INDEX `IX_CommentAttachments_CommentId` ON `CommentAttachments` (`CommentId`);


CREATE INDEX `IX_Comments_BlogPostId` ON `Comments` (`BlogPostId`);


CREATE INDEX `IX_Comments_ParentCommentId` ON `Comments` (`ParentCommentId`);


CREATE INDEX `IX_Comments_UserId` ON `Comments` (`UserId`);


CREATE INDEX `IX_CustomerServiceRoleMappings_AspNetRoleName` ON `CustomerServiceRoleMappings` (`AspNetRoleName`);


CREATE UNIQUE INDEX `IX_CustomerServiceRoleMappings_CustomerServiceRole` ON `CustomerServiceRoleMappings` (`CustomerServiceRole`);


CREATE INDEX `IX_LogArchives_ArchivedAt` ON `LogArchives` (`ArchivedAt`);


CREATE INDEX `IX_LogArchives_EntityId` ON `LogArchives` (`EntityId`);


CREATE INDEX `IX_LogArchives_LoggedAt` ON `LogArchives` (`LoggedAt`);


CREATE INDEX `IX_LogArchives_TableName` ON `LogArchives` (`TableName`);


CREATE INDEX `IX_LogArchives_TableName_EntityId` ON `LogArchives` (`TableName`, `EntityId`);


CREATE INDEX `IX_LogArchives_UserId` ON `LogArchives` (`UserId`);


CREATE INDEX `IX_Logs_EntityId` ON `Logs` (`EntityId`);


CREATE INDEX `IX_Logs_LoggedAt` ON `Logs` (`LoggedAt`);


CREATE INDEX `IX_Logs_TableName` ON `Logs` (`TableName`);


CREATE INDEX `IX_Logs_TableName_EntityId` ON `Logs` (`TableName`, `EntityId`);


CREATE INDEX `IX_Logs_UserId` ON `Logs` (`UserId`);


CREATE INDEX `IX_Menus_Order` ON `Menus` (`Order`);


CREATE INDEX `IX_Menus_ParentId` ON `Menus` (`ParentId`);


CREATE INDEX `IX_RoleClaims_RoleId` ON `RoleClaims` (`RoleId`);


CREATE INDEX `IX_RoleMenus_MenuId` ON `RoleMenus` (`MenuId`);


CREATE UNIQUE INDEX `IX_RoleMenus_RoleId_MenuId` ON `RoleMenus` (`RoleId`, `MenuId`);


CREATE UNIQUE INDEX `RoleNameIndex` ON `Roles` (`NormalizedName`);


CREATE INDEX `IX_SiteSettings_Category` ON `SiteSettings` (`Category`);


CREATE UNIQUE INDEX `IX_SiteSettings_Key` ON `SiteSettings` (`Key`);


CREATE INDEX `IX_SiteSettings_Order` ON `SiteSettings` (`Order`);


CREATE INDEX `IX_TicketAttachments_CommentId` ON `TicketAttachments` (`CommentId`);


CREATE INDEX `IX_TicketAttachments_TicketId` ON `TicketAttachments` (`TicketId`);


CREATE INDEX `IX_TicketAttachments_UploadedById` ON `TicketAttachments` (`UploadedById`);


CREATE INDEX `IX_TicketComments_CreatedAt` ON `TicketComments` (`CreatedAt`);


CREATE INDEX `IX_TicketComments_TicketId` ON `TicketComments` (`TicketId`);


CREATE INDEX `IX_TicketComments_UserId` ON `TicketComments` (`UserId`);


CREATE INDEX `IX_TicketHistory_CreatedAt` ON `TicketHistory` (`CreatedAt`);


CREATE INDEX `IX_TicketHistory_TicketId` ON `TicketHistory` (`TicketId`);


CREATE INDEX `IX_TicketHistory_UserId` ON `TicketHistory` (`UserId`);


CREATE INDEX `IX_TicketNotifications_CreatedAt` ON `TicketNotifications` (`CreatedAt`);


CREATE INDEX `IX_TicketNotifications_TicketId` ON `TicketNotifications` (`TicketId`);


CREATE INDEX `IX_TicketNotifications_UserId_IsRead` ON `TicketNotifications` (`UserId`, `IsRead`);


CREATE INDEX `IX_Tickets_AssignedById` ON `Tickets` (`AssignedById`);


CREATE INDEX `IX_Tickets_AssignedToId` ON `Tickets` (`AssignedToId`);


CREATE INDEX `IX_Tickets_CreatedAt` ON `Tickets` (`CreatedAt`);


CREATE INDEX `IX_Tickets_CustomerId` ON `Tickets` (`CustomerId`);


CREATE INDEX `IX_Tickets_LastModifiedById` ON `Tickets` (`LastModifiedById`);


CREATE INDEX `IX_Tickets_Priority` ON `Tickets` (`Priority`);


CREATE INDEX `IX_Tickets_Status` ON `Tickets` (`Status`);


CREATE UNIQUE INDEX `IX_Tickets_TicketNumber` ON `Tickets` (`TicketNumber`);


CREATE INDEX `IX_UserClaims_UserId` ON `UserClaims` (`UserId`);


CREATE INDEX `IX_UserLogins_UserId` ON `UserLogins` (`UserId`);


CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);


CREATE INDEX `EmailIndex` ON `Users` (`NormalizedEmail`);


CREATE UNIQUE INDEX `UserNameIndex` ON `Users` (`NormalizedUserName`);


