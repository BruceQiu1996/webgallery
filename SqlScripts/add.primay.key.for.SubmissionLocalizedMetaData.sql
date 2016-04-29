USE [MsComWebDb]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SubmissionLocalizedMetaData_BACKUP](
	[SubmissionID] [int] NOT NULL,
	[Language] [varchar](50) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1500) NOT NULL,
	[BriefDescription] [nvarchar](400) NOT NULL
)

INSERT INTO [dbo].[SubmissionLocalizedMetaData_BACKUP] ([SubmissionID], [Language], [Name], [Description], [BriefDescription])
SELECT [SubmissionID], [Language], [Name], [Description], [BriefDescription]
FROM [dbo].[SubmissionLocalizedMetaData]


DROP TABLE [SubmissionLocalizedMetaData]

CREATE TABLE [dbo].[SubmissionLocalizedMetaData](
	[MetadataID] [int] IDENTITY(1,1) NOT NULL,
	[SubmissionID] [int] NOT NULL,
	[Language] [varchar](50) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1500) NOT NULL,
	[BriefDescription] [nvarchar](400) NOT NULL,
	CONSTRAINT [PK_SubmissionLocalizedMetaData] PRIMARY KEY CLUSTERED 
	(
		[MetadataID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

INSERT INTO [dbo].[SubmissionLocalizedMetaData] ([SubmissionID], [Language], [Name], [Description], [BriefDescription])
SELECT [SubmissionID], [Language], [Name], [Description], [BriefDescription]
FROM [dbo].[SubmissionMetadata_BACKUP]


GO