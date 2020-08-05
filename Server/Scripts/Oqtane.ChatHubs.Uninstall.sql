/*  
Remove ChatHub Tables
*/

IF COL_LENGTH('dbo.User', 'UserType') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[User] DROP COLUMN [UserType]
END

GO

DROP TABLE [dbo].[ChatHubMessage]
GO

DROP TABLE [dbo].[ChatHubConnection]
GO

DROP TABLE [dbo].[ChatHubPhoto]
GO

DROP TABLE [dbo].[ChatHubSetting]
GO

DROP TABLE [dbo].[ChatHubIgnore]
GO

DROP TABLE [dbo].[ChatHubRoomChatHubUser]
GO

DROP TABLE [dbo].[ChatHubRoom]
GO