USE DemoDB

INSERT INTO LOOKUPRole (RoleName, RoleDescription, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES	('Admin','Can Edit, Update, Delete', 1, 1),
					('Member','Read only', 1, 1)

--*********************************************************
-- add admin and member
--*********************************************************
INSERT INTO SYSUser (LoginName, PasswordEncryptedText, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES ('Admin','Admin', 1, 1)

INSERT INTO SYSUser (LoginName, PasswordEncryptedText, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES ('member','member', 1, 1)

select * from SYSUser

INSERT INTO SYSUserProfile (SYSUserID, FirstName, LastName, Email, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES (1, 'Šike', 'Bajramović', 'Šike@nikad.com', 1, 1)

INSERT INTO SYSUserProfile (SYSUserID, FirstName, LastName, Email, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES (2, 'Šaban', 'Miftari', 'Šaban@šabanija.com', 1, 1);

--*********************************************************
-- add roles
--*********************************************************
INSERT INTO
	SYSUserRole (SYSUserID, LOOKUPRoleID, IsActive, RowCreatedSYSUserID, RowModifiedSYSUserID)
	VALUES (1,1,1,1,1), (2,2,1,1,1);

INSERT INTO [dbo].[USER Preferences] ([SYSUserID])
	VALUES (1);

SELECT *FROM [dbo].[USER Preferences]
