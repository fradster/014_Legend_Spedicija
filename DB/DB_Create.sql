use master
go

IF DB_ID('DemoDB') IS NOT NULL DROP DATABASE DemoDB;
go

create database DemoDB;
go

USE DemoDB
GO

CREATE TABLE [dbo].[LOOKUPRole](
	[LOOKUPRoleID] [int] IDENTITY(1,1) NOT NULL,--0: nobody; 1: admin; 2: user
	[RoleName] [varchar](25) DEFAULT '',
	[RoleDescription] [varchar](100) DEFAULT '',
	[RowCreatedSYSUserID] [int] NOT NULL,
	[RowCreatedDateTime] [datetime] DEFAULT GETDATE(),
	[RowModifiedSYSUserID] [int] NOT NULL,
	[RowModifiedDateTime] [datetime] DEFAULT GETDATE(),
	PRIMARY KEY (LOOKUPRoleID)
);
GO

CREATE TABLE [dbo].[SYSUser](
	[SYSUserID] [int] IDENTITY(1,1) NOT NULL,
	[LoginName] [Nvarchar](50) NOT NULL,
	[PasswordEncryptedText] [Nvarchar](50) NOT NULL,
	[RowCreatedSYSUserID] [int] NOT NULL,
	[RowCreatedDateTime] [datetime] DEFAULT GETDATE(),
	[RowModifiedSYSUserID] [int] NOT NULL,
	[RowModifiedDateTime] [datetime] DEFAULT GETDATE(),
	PRIMARY KEY (SYSUserID)
);
GO

ALTER TABLE [dbo].[SYSUser] ADD
	Salt binary(32);
GO

--ALTER TABLE [dbo].[SYSUser]
	--DROP COLUMN PasswordTemprary


CREATE TABLE [dbo].[SYSUserProfile](
	[SYSUserProfileID] [int] IDENTITY(1,1) NOT NULL,
	[SYSUserID] [int] NOT NULL,
	[FirstName] [Nvarchar](30) NOT NULL,
	[LastName] [Nvarchar](30) NOT NULL,
	[RowCreatedSYSUserID] [int] NOT NULL,
	[RowCreatedDateTime] [datetime] DEFAULT GETDATE(),
	[RowModifiedSYSUserID] [int] NOT NULL,
	[RowModifiedDateTime] [datetime] DEFAULT GETDATE(),
	[email] NVARCHAR (288) NOT NULL,
	PRIMARY KEY (SYSUserProfileID)
);
GO

CREATE TABLE [dbo].[USER Preferences] (
	[PreferencesId]				INT IDENTITY(1,1) NOT NULL,
	[SYSUserID]						INT NOT NULL,
	[NoOfUsersDisplayed]	INT DEFAULT 10,
	PRIMARY KEY (PreferencesId)
);
GO

CREATE TABLE [dbo].[SYSUserRole](
	[SYSUserRoleID] [int] IDENTITY(1,1) NOT NULL,
	[SYSUserID] [int] NOT NULL,
	[LOOKUPRoleID] [int] NOT NULL,--0: nobody; 1: admin; 2: user
	[IsActive] [bit] DEFAULT (1),
	[RowCreatedSYSUserID] [int] NOT NULL,
	[RowCreatedDateTime] [datetime] DEFAULT GETDATE(),
	[RowModifiedSYSUserID] [int] NOT NULL,
	[RowModifiedDateTime] [datetime] DEFAULT GETDATE(),
	PRIMARY KEY (SYSUserRoleID)
);
GO

--*****************************************************************
--** messaging
CREATE TABLE [dbo].[Message](
	[MessageID] [int] IDENTITY(1,1) NOT NULL,
	[SYSUserID] [int] NULL,
	[MessageText] [nTExt] NULL,
	[DatePosted] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

--******************************************************************
--** smtp parameters
CREATE TABLE [Admin SMTP parameters] (
	[Id]					int						PRIMARY KEY IDENTITY(1,1),
	[UserEmail]		NVARCHAR(60)	NOT NULL,
	[password]		NVARCHAR(30)	NOT NULL,
	[host]				NVARCHAR(30)	NOT NULL,
	[RowCreatedSYSUserID] [int] NOT NULL,
	[RowCreatedDateTime] [datetime] DEFAULT GETDATE(),
	[RowModifiedSYSUserID] [int] NOT NULL,
	[RowModifiedDateTime] [datetime] DEFAULT GETDATE()
)
GO

--DROP TABLE [Admin SMTP parameters]

SELECT * FROM [dbo].[SYSUser]