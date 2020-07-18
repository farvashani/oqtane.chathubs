/*  
Create Blog table
*/

CREATE TABLE [dbo].[StreamHub](
	[StreamHubId] [int] IDENTITY(1,1) NOT NULL,
	[ModuleId] [int] NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_StreamHub] PRIMARY KEY CLUSTERED 
  (
	[StreamHubId] ASC
  )
)
GO

/*  

Create foreign key relationships

*/
ALTER TABLE [dbo].[StreamHub]  WITH CHECK ADD  CONSTRAINT [FK_StreamHub_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
ON DELETE CASCADE
GO