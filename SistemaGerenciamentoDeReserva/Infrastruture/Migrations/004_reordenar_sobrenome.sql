BEGIN;

TRUNCATE reservas;

DROP TABLE usuarios CASCADE;

CREATE TABLE usuarios (
    id serial PRIMARY KEY,
    nome character varying(150) NOT NULL,
    sobrenome character varying(150) NOT NULL DEFAULT ''::character varying,
    email character varying(150) NOT NULL UNIQUE,
    senha_hash character varying(255) NOT NULL,
    role character varying(20) NOT NULL DEFAULT 'User'::character varying,
    criado_em timestamp without time zone NOT NULL DEFAULT now(),
    atualizado_em timestamp without time zone,
    excluido_em timestamp without time zone
);

CREATE INDEX ix_usuarios_ativos ON usuarios (id) WHERE excluido_em IS NULL;

ALTER TABLE reservas
    ADD CONSTRAINT fk_reserva_usuario
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE RESTRICT;

COMMIT;
