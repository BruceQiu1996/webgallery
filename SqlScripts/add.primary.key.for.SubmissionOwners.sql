USE [MsComWebDb]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SubmissionOwners_BACKUP](
	[SubmissionOwnerID] [int] NOT NULL,
	[SubmitterID] [int] NOT NULL,
	[SubmissionID] [int] NOT NULL
)

INSERT INTO [dbo].[SubmissionOwners_BACKUP]	([SubmissionOwnerID], [SubmitterID], [SubmissionID])
SELECT [SubmissionOwnerID], [SubmitterID], [SubmissionID]
FROM [dbo].[SubmissionOwners]
GO

DROP TABLE [SubmissionOwners]

CREATE TABLE [dbo].[SubmissionOwners](
	[SubmissionOwnerID] [int] IDENTITY(1,1) NOT NULL,
	[SubmitterID] [int] NOT NULL,
	[SubmissionID] [int] NOT NULL,
	CONSTRAINT [PK_SubmissionOwners] PRIMARY KEY CLUSTERED 
	(
		[SubmissionOwnerID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

INSERT INTO [dbo].[SubmissionOwners] ([SubmitterID], [SubmissionID])
SELECT distinct [SubmitterID], [SubmissionID]
FROM [dbo].[SubmissionOwners_BACKUP]


GO