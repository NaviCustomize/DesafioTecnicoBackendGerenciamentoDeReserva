-- Delete lógico em usuários, hotéis e quartos.
--
-- Motivo: a exclusão física esbarra nas chaves estrangeiras com RESTRICT
-- (um quarto com reservas, um hotel com quartos) e o histórico do sistema
-- não pode ser destruído só porque o cadastro saiu de operação.
--
-- Coluna nula = registro ativo. Migração aditiva: nada é apagado e a
-- reversão é apenas remover as colunas.

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;
ALTER TABLE hoteis   ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;
ALTER TABLE quartos  ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;

-- As listagens sempre filtram por ativo; o índice parcial evita varrer os excluídos.
CREATE INDEX IF NOT EXISTS ix_usuarios_ativos ON usuarios (id) WHERE excluido_em IS NULL;
CREATE INDEX IF NOT EXISTS ix_hoteis_ativos   ON hoteis (id)   WHERE excluido_em IS NULL;
CREATE INDEX IF NOT EXISTS ix_quartos_ativos  ON quartos (id)  WHERE excluido_em IS NULL;
