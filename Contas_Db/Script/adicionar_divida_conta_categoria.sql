USE [test_fin]
GO

-- Vincula cada dívida a uma conta (responsável pelo pagamento) e a uma categoria,
-- usadas para gerar automaticamente as parcelas ao inserir a dívida.

ALTER TABLE [dbo].[tb_divida] ADD [id_conta] [int] NULL
GO
ALTER TABLE [dbo].[tb_divida] ADD [id_categoria] [int] NULL
GO

-- Se já existirem dívidas cadastradas, atualize id_conta/id_categoria antes de rodar os ALTERs abaixo,
-- senão a conversão para NOT NULL vai falhar.

ALTER TABLE [dbo].[tb_divida] ALTER COLUMN [id_conta] [int] NOT NULL
GO
ALTER TABLE [dbo].[tb_divida] ALTER COLUMN [id_categoria] [int] NOT NULL
GO

ALTER TABLE [dbo].[tb_divida]  WITH CHECK ADD  CONSTRAINT [FK_tb_divida_tb_conta] FOREIGN KEY([id_conta])
REFERENCES [dbo].[tb_conta] ([id_conta])
GO
ALTER TABLE [dbo].[tb_divida] CHECK CONSTRAINT [FK_tb_divida_tb_conta]
GO
ALTER TABLE [dbo].[tb_divida]  WITH CHECK ADD  CONSTRAINT [FK_tb_divida_tb_categoria] FOREIGN KEY([id_categoria])
REFERENCES [dbo].[tb_categoria] ([id_categoria])
GO
ALTER TABLE [dbo].[tb_divida] CHECK CONSTRAINT [FK_tb_divida_tb_categoria]
GO

CREATE NONCLUSTERED INDEX [IX_tb_divida_id_conta] ON [dbo].[tb_divida] ([id_conta] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_tb_divida_id_categoria] ON [dbo].[tb_divida] ([id_categoria] ASC)
GO
