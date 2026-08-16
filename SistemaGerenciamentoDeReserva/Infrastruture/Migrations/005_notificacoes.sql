-- Registro das notificações consumidas da fila.
--
-- Antes disso o consumidor apenas escrevia no log: a mensageria funcionava mas
-- não deixava rastro consultável. Com a tabela, cada evento processado vira
-- linha visível na área administrativa.
--
-- Os dados do hóspede e do quarto são gravados como texto, e não por FK, de
-- propósito: a notificação é um retrato do momento em que ocorreu. Se o hóspede
-- trocar de nome ou o quarto for excluído depois, o histórico continua contando
-- o que era verdade quando a mensagem foi publicada.

CREATE TABLE IF NOT EXISTS notificacoes (
    id            serial PRIMARY KEY,
    reserva_id    bigint NOT NULL,
    usuario_id    bigint NOT NULL,
    quarto_id     bigint NOT NULL,
    tipo_evento   varchar(30) NOT NULL,
    hospede       varchar(300) NOT NULL DEFAULT '',
    hospede_email varchar(150) NOT NULL DEFAULT '',
    hotel         varchar(150) NOT NULL DEFAULT '',
    quarto_numero integer NOT NULL DEFAULT 0,
    data_checkin  timestamp NOT NULL,
    data_checkout timestamp NOT NULL,
    ocorrido_em   timestamp NOT NULL,
    processado_em timestamp NOT NULL DEFAULT NOW()
);

-- A listagem sempre traz as mais recentes primeiro
CREATE INDEX IF NOT EXISTS ix_notificacoes_recentes ON notificacoes (processado_em DESC);
