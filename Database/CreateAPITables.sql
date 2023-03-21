USE [PurchasingTest]
GO

/****** Object:  Table [dbo].[Results]    Script Date: 10/25/2022 10:54:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[result_products](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[totalNumRecs] [nvarchar](50) NULL,
	[firstRecNum] [nvarchar](50) NULL,
	[lastRecNum] [nvarchar](50) NULL,
	[sortOptions] [nvarchar](50) NULL,
	[productName] [nvarchar](50) NULL,
	[searcheables] [nvarchar](50) NULL,
	[brandImageName] [nvarchar](100) NULL,
	[upcs] [nvarchar](100) NULL
) 
GO


CREATE TABLE [dbo].[result_benefitCopies](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_attributes](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_images](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_documents](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_services](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_relationships](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_binaryObjectDetails](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO

CREATE TABLE [dbo].[result_featuredParts](
	[ID] UNIQUEIDENTIFIER NOT NULL,
	[product_id] [nvarchar](50) NULL,
	[info] [nvarchar](max) NULL,
) 
GO



