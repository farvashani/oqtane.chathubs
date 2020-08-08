/*  
Create ChatHub Tables
*/

CREATE TABLE [dbo].[ChatHubRoom](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModuleId] [int] NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[ImageUrl] [nvarchar](256) NOT NULL,
	[Type] [nvarchar](256) NOT NULL,
	[Status] [nvarchar](256) NOT NULL,
	[OneVsOneId] [nvarchar](256) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubRoom] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubRoomChatHubUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubRoomId] [int] NOT NULL,
	[ChatHubUserId] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubRoomChatHubUser] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubMessage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubRoomId] [int] NOT NULL,
	[ChatHubUserId] [int] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Type] [nvarchar](256) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubMessage] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubConnection](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubUserId] [int] NOT NULL,
	[ConnectionId] [nvarchar](256) NOT NULL,
	[IpAddress] [nvarchar](256) NOT NULL,
	[UserAgent] [nvarchar](512) NOT NULL,
	[Status] [nvarchar](256) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubConnection] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubPhoto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubMessageId] [int] NOT NULL,
	[Source] [nvarchar](256) NOT NULL,
	[Thumb] [nvarchar](256) NOT NULL,
	[Caption] [nvarchar](512) NOT NULL,
	[Size] [bigint] NOT NULL,
	[Width] [int] NOT NULL,
	[Height] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubPhoto] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubUserId] [int] NOT NULL,
	[UsernameColor] [nvarchar](256) NOT NULL,
	[MessageColor] [nvarchar](256) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubSetting] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

CREATE TABLE [dbo].[ChatHubIgnore](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChatHubUserId] [int] NOT NULL,
	[ChatHubIgnoredUserId] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ChatHubIgnore] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)
GO

/*
Alter Table Add Columns
*/

IF COL_LENGTH('dbo.User', 'UserType') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[User] DROP COLUMN [UserType]
END

ALTER TABLE [dbo].[User] ADD [UserType] [nvarchar](256) NULL
GO

/*
Create foreign key relationships
*/

ALTER TABLE [dbo].[ChatHubRoom]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubRoom_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubRoomChatHubUser]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubRoomChatHubUser_ChatHubRoom] FOREIGN KEY([ChatHubRoomId])
REFERENCES [dbo].[ChatHubRoom] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubRoomChatHubUser]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubRoomChatHubUser_ChatHubUser] FOREIGN KEY([ChatHubUserId])
REFERENCES [dbo].[User] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubMessage]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubMessage_ChatHubRoom] FOREIGN KEY([ChatHubRoomId])
REFERENCES [dbo].[ChatHubRoom] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubMessage]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubMessage_ChatHubUser] FOREIGN KEY([ChatHubUserId])
REFERENCES [dbo].[User] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubConnection]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubConnection_ChatHubUser] FOREIGN KEY([ChatHubUserId])
REFERENCES [dbo].[User] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubPhoto]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubPhoto_ChatHubMessage] FOREIGN KEY([ChatHubMessageId])
REFERENCES [dbo].[ChatHubMessage] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubSetting]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubSetting_ChatHubUser] FOREIGN KEY([ChatHubUserId])
REFERENCES [dbo].[User] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ChatHubIgnore]  WITH CHECK ADD  CONSTRAINT [FK_ChatHubIgnore_ChatHubUser] FOREIGN KEY([ChatHubUserId])
REFERENCES [dbo].[User] ([UserId])
ON DELETE CASCADE
GO
