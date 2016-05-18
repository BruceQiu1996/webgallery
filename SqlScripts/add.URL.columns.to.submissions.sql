USE [mscomwebDB]
GO

ALTER TABLE 
	[dbo].[Submissions]
ADD 
	[LogoUrl] [nvarchar](255) NULL,
	[ScreenshotUrl1] [nvarchar](255) NULL,
	[ScreenshotUrl2] [nvarchar](255) NULL,
	[ScreenshotUrl3] [nvarchar](255) NULL,
	[ScreenshotUrl4] [nvarchar](255) NULL,
	[ScreenshotUrl5] [nvarchar](255) NULL,
	[ScreenshotUrl6] [nvarchar](255) NULL
GO


