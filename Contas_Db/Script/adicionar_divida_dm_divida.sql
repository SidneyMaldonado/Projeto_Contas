USE [test_fin]
GO

-- Distingue dívida (a pagar) de receita (a receber) no mesmo cadastro:
-- dm_divida = 1 -> dívida, dm_divida = 0 -> receita.
-- O DEFAULT 1 preenche as linhas já existentes, que são todas dívidas.

ALTER TABLE [dbo].[tb_divida] ADD [dm_divida] [bit] NOT NULL
    CONSTRAINT [DF_tb_divida_dm_divida] DEFAULT ((1)) WITH VALUES
GO
