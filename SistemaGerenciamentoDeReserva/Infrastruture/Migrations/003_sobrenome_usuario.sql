-- Sobrenome do usuário.
--
-- NOT NULL com DEFAULT '' para que as contas já existentes continuem válidas.
-- Vazio significa "cadastro anterior a este campo": a interface mostra só o
-- nome nesses casos, e a exigência do preenchimento vale para cadastros novos.

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS sobrenome VARCHAR(150) NOT NULL DEFAULT '';
