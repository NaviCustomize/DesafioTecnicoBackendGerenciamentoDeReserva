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

CREATE INDEX IF NOT EXISTS ix_notificacoes_recentes ON notificacoes (processado_em DESC);
