-- Auditoria em todas as entidades: quando o registro nasceu, quando mudou
-- pela última vez e quando saiu de operação.
--
-- criado_em tem DEFAULT NOW() para que as linhas que já existem recebam um
-- valor e a coluna possa ser NOT NULL sem quebrar nada.
-- atualizado_em fica nulo até a primeira alteração — nulo significa "nunca
-- foi alterado", que é informação, não ausência de dado.

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS criado_em     TIMESTAMP NOT NULL DEFAULT NOW();
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS atualizado_em TIMESTAMP NULL;

ALTER TABLE hoteis   ADD COLUMN IF NOT EXISTS criado_em     TIMESTAMP NOT NULL DEFAULT NOW();
ALTER TABLE hoteis   ADD COLUMN IF NOT EXISTS atualizado_em TIMESTAMP NULL;

ALTER TABLE quartos  ADD COLUMN IF NOT EXISTS criado_em     TIMESTAMP NOT NULL DEFAULT NOW();
ALTER TABLE quartos  ADD COLUMN IF NOT EXISTS atualizado_em TIMESTAMP NULL;

-- Reservas ficaram de fora da migração anterior; recebem o trio completo.
ALTER TABLE reservas ADD COLUMN IF NOT EXISTS criado_em     TIMESTAMP NOT NULL DEFAULT NOW();
ALTER TABLE reservas ADD COLUMN IF NOT EXISTS atualizado_em TIMESTAMP NULL;
ALTER TABLE reservas ADD COLUMN IF NOT EXISTS excluido_em   TIMESTAMP NULL;

CREATE INDEX IF NOT EXISTS ix_reservas_ativas ON reservas (id) WHERE excluido_em IS NULL;
