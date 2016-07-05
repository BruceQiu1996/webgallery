USE [MsComWebDb]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Issues](
	[IssueID] [int] IDENTITY(1,1) NOT NULL,
	[IssueType] [int] NOT NULL,
	[AppId] [nvarchar](255) NULL,
	[IssueDescription] [nvarchar] (4000) NOT NULL,
	[ReporterEmail] [nvarchar](500) NOT NULL,
	[ReporterFirstName] [nvarchar](255) NOT NULL,
	[ReporterLastName] [nvarchar](255) NOT NULL,
	[DateReported] [datetime] NOT NULL,
	CONSTRAINT [PK_Issues] PRIMARY KEY CLUSTERED 
	(
		[IssueID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO