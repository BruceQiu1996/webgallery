USE [MsComWebDb]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[UnconfirmedSubmissionOwners_BACKUP](
	[UnconfirmedSubmissionOwnerID] [int] NOT NULL,
	[SubmissionID] [int] NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[RequestDate] [date] NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[IsSuperSubmitterRequest] [bit] NULL
)

INSERT INTO [dbo].[UnconfirmedSubmissionOwners_BACKUP]	([UnconfirmedSubmissionOwnerID], [SubmissionID], [RequestID], [RequestDate], [FirstName], [LastName], [IsSuperSubmitterRequest])
SELECT [UnconfirmedSubmissionOwnerID], [SubmissionID], [RequestID], [RequestDate], [FirstName], [LastName], [IsSuperSubmitterRequest]
FROM [dbo].[UnconfirmedSubmissionOwners]
GO

ALTER TABLE [dbo].[UnconfirmedSubmissionOwners] ADD PRIMARY KEY (UnconfirmedSubmissionOwnerID)

GO