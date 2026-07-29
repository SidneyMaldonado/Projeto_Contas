USE [test_fin]
GO
/****** Objeto:  Table [dbo].[tb_investimento]    Data do Script: 29/07/2026 10:37:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_investimento](
	[id_investimento] [int] IDENTITY(1,1) NOT NULL,
	[id_carteira] [int] NOT NULL,
	[nm_investimento] [nvarchar](50) NOT NULL,
	[nr_quantidade] [numeric](10, 2) NOT NULL,
	[vl_cotacao] [numeric](10, 2) NOT NULL,
	[ds_observacao] [nvarchar](500) NULL,
	[dm_ativo] [bit] NOT NULL,
 CONSTRAINT [PK_tb_investimento] PRIMARY KEY CLUSTERED 
(
	[id_investimento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[tb_carteira]    Data do Script: 29/07/2026 10:37:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_carteira](
	[id_carteira] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[nm_carteira] [nvarchar](50) NOT NULL,
	[dm_ativo] [bit] NOT NULL,
 CONSTRAINT [PK_tb_carteira] PRIMARY KEY CLUSTERED 
(
	[id_carteira] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[tb_historico]    Data do Script: 29/07/2026 10:37:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_historico](
	[id_historico] [int] IDENTITY(1,1) NOT NULL,
	[dt_historico] [datetime] NOT NULL,
	[id_investimento] [int] NOT NULL,
	[nm_investimento] [nvarchar](50) NOT NULL,
	[nr_quantidade] [numeric](10, 2) NOT NULL,
	[vl_cotacao] [numeric](10, 2) NOT NULL,
	[ds_observacao] [nvarchar](500) NOT NULL,
	[dm_ativo] [bit] NOT NULL,
 CONSTRAINT [PK_tb_historico] PRIMARY KEY CLUSTERED 
(
	[id_historico] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[tb_operacao]    Data do Script: 29/07/2026 10:37:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_operacao](
	[id_operacao] [int] IDENTITY(1,1) NOT NULL,
	[id_investimento] [int] NOT NULL,
	[dm_compra] [bit] NOT NULL,
	[dt_operacao] [datetime] NOT NULL,
	[nr_quantidade] [int] NOT NULL,
	[vl_operacao] [numeric](10, 2) NOT NULL,
	[dm_ativo] [bit] NOT NULL,
 CONSTRAINT [PK_tb_operacao] PRIMARY KEY CLUSTERED
(
	[id_operacao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tb_investimento] ADD  CONSTRAINT [DF_tb_investimento_dm_ativo]  DEFAULT ((1)) FOR [dm_ativo]
GO
ALTER TABLE [dbo].[tb_carteira] ADD  CONSTRAINT [DF_tb_carteira_dm_ativo]  DEFAULT ((1)) FOR [dm_ativo]
GO
ALTER TABLE [dbo].[tb_historico] ADD  CONSTRAINT [DF_tb_historico_dt_historico]  DEFAULT (getdate()) FOR [dt_historico]
GO
ALTER TABLE [dbo].[tb_operacao] ADD  CONSTRAINT [DF_tb_operacao_dt_operacao]  DEFAULT (getdate()) FOR [dt_operacao]
GO
ALTER TABLE [dbo].[tb_operacao] ADD  CONSTRAINT [DF_tb_operacao_dm_ativo]  DEFAULT ((1)) FOR [dm_ativo]
GO
ALTER TABLE [dbo].[tb_investimento]  WITH CHECK ADD  CONSTRAINT [FK_tb_investimento_tb_carteira] FOREIGN KEY([id_carteira])
REFERENCES [dbo].[tb_carteira] ([id_carteira])
GO
ALTER TABLE [dbo].[tb_investimento] CHECK CONSTRAINT [FK_tb_investimento_tb_carteira]
GO
ALTER TABLE [dbo].[tb_carteira]  WITH CHECK ADD  CONSTRAINT [FK_tb_carteira_tb_usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[tb_usuario] ([id_usuario])
GO
ALTER TABLE [dbo].[tb_carteira] CHECK CONSTRAINT [FK_tb_carteira_tb_usuario]
GO
ALTER TABLE [dbo].[tb_historico]  WITH CHECK ADD  CONSTRAINT [FK_tb_historico_tb_investimento] FOREIGN KEY([id_investimento])
REFERENCES [dbo].[tb_investimento] ([id_investimento])
GO
ALTER TABLE [dbo].[tb_historico] CHECK CONSTRAINT [FK_tb_historico_tb_investimento]
GO
ALTER TABLE [dbo].[tb_operacao]  WITH CHECK ADD  CONSTRAINT [FK_tb_operacao_tb_investimento] FOREIGN KEY([id_investimento])
REFERENCES [dbo].[tb_investimento] ([id_investimento])
GO
ALTER TABLE [dbo].[tb_operacao] CHECK CONSTRAINT [FK_tb_operacao_tb_investimento]
GO
