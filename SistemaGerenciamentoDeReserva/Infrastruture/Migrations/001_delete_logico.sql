
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;
ALTER TABLE hoteis   ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;
ALTER TABLE quartos  ADD COLUMN IF NOT EXISTS excluido_em TIMESTAMP NULL;

CREATE INDEX IF NOT EXISTS ix_usuarios_ativos ON usuarios (id) WHERE excluido_em IS NULL;
CREATE INDEX IF NOT EXISTS ix_hoteis_ativos   ON hoteis (id)   WHERE excluido_em IS NULL;
CREATE INDEX IF NOT EXISTS ix_quartos_ativos  ON quartos (id)  WHERE excluido_em IS NULL;
