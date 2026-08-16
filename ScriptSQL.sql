
-- COMO USAR
--   1. Crie o banco:      
CREATE DATABASE "Gerenciamento_reserva";

--   2. Script:  psql -U postgres -d Gerenciamento_reserva -f criar_banco.sql

-- SENHAS DOS USUARIOS PARA EXEMPLO
--   teste.integracao@sgr.local    Teste@123      (Administrador)
--   felipe.santana@hospede.com   Hospede@123
--   rodrigo.cabral@hospede.com   Hospede@123
--   artur.almeida@hospede.com    Hospede@123

DROP TABLE IF EXISTS notificacoes CASCADE;
DROP TABLE IF EXISTS reservas CASCADE;
DROP TABLE IF EXISTS quartos CASCADE;
DROP TABLE IF EXISTS hoteis CASCADE;
DROP TABLE IF EXISTS usuarios CASCADE;

-- Role De Usuarios: 'User' ou 'Admin'
CREATE TABLE usuarios (
    id            serial PRIMARY KEY,
    nome          varchar(150) NOT NULL,
    sobrenome     varchar(150) NOT NULL DEFAULT '',
    email         varchar(150) NOT NULL UNIQUE,
    senha_hash    varchar(255) NOT NULL,
    role          varchar(20) NOT NULL DEFAULT 'User',
    criado_em     timestamp NOT NULL DEFAULT NOW(),
    atualizado_em timestamp,
    excluido_em   timestamp -- Nao exclui de verdade, e um delete logico
);

CREATE TABLE hoteis (
    id            serial PRIMARY KEY,
    nome          varchar(150) NOT NULL,
    localizacao   varchar(255) NOT NULL,
    descricao     text,
    criado_em     timestamp NOT NULL DEFAULT NOW(),
    atualizado_em timestamp,
    excluido_em   timestamp
);

-- tipos:   0 = Standard, 1 = Luxo, 2 = SuiteMaster
-- status:  0 = Disponivel, 1 = Reservado
CREATE TABLE quartos (
    id              serial PRIMARY KEY,
    hotel_id        integer NOT NULL,
    numero          integer NOT NULL,
    tipo            integer NOT NULL,
    preco_por_noite numeric(10,2) NOT NULL,
    status          integer NOT NULL DEFAULT 0,
    criado_em       timestamp NOT NULL DEFAULT NOW(),
    atualizado_em   timestamp,
    excluido_em     timestamp,
    CONSTRAINT fk_quarto_hotel FOREIGN KEY (hotel_id)
        REFERENCES hoteis(id) ON DELETE RESTRICT,
    CONSTRAINT uq_quarto_hotel_numero UNIQUE (hotel_id, numero)
);

-- status: 0 = Pendente, 1 = Confirmada, 2 = Cancelada, 3 = Finalizada
-- horario de entrada as 14h, e de saida as 12h
CREATE TABLE reservas (
    id            bigserial PRIMARY KEY,
    data_checkin  timestamp NOT NULL,
    data_checkout timestamp NOT NULL,
    status        integer NOT NULL DEFAULT 1,
    usuario_id    bigint NOT NULL,
    quarto_id     bigint NOT NULL,
    criado_em     timestamp NOT NULL DEFAULT NOW(),
    atualizado_em timestamp,
    excluido_em   timestamp,
    CONSTRAINT fk_reserva_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT,
    CONSTRAINT fk_reserva_quarto FOREIGN KEY (quarto_id)
        REFERENCES quartos(id) ON DELETE RESTRICT
);

-- Eventos de reserva consumidos da fila do RabbitMQ
-- Os dados do hospede e do hotel sao gravados como texto, e nao por chave
-- estrangeira, porque a notificacao e um retrato do momento em que ocorreu
CREATE TABLE notificacoes (
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

-- Indices parciais: as listagens sempre filtram por registro ativo
CREATE INDEX ix_usuarios_ativos  ON usuarios (id) WHERE excluido_em IS NULL;
CREATE INDEX ix_hoteis_ativos    ON hoteis (id)   WHERE excluido_em IS NULL;
CREATE INDEX ix_quartos_ativos   ON quartos (id)  WHERE excluido_em IS NULL;
CREATE INDEX ix_reservas_ativas  ON reservas (id) WHERE excluido_em IS NULL;
CREATE INDEX ix_notificacoes_recentes ON notificacoes (processado_em DESC);

-- Como escrito no ReadMe, Uso do BCrypt para gerar um Hash da Senha
INSERT INTO usuarios (nome, sobrenome, email, senha_hash, role) VALUES
    ('Teste',   'Integracao', 'teste.integracao@sgr.local',  '$2a$11$PoVnSIBXoHMKg7vtlJ0dzOjgJKL0X/3r1NC9rTL8ypw6CwXhf4mm.', 'Admin'),
    ('Larissa', 'Andrade',    'larissa.andrade@hospede.com', '$2a$11$/sC8CfqT5Eg1s77KAZxbkeR3WUWooKY6u8LOq2IoC8TzYH3iYium2', 'User'),
    ('Rafael',  'Monteiro',   'rafael.monteiro@hospede.com', '$2a$11$lsdK9lUZuCJLuxyGOLj0F.N7bELP9odeoTg/G1Kypg541GYuy984m', 'User'),
    ('Beatriz', 'Campos',     'beatriz.campos@hospede.com',  '$2a$11$fyqKOkG2gY0J55iH/fAVzOnnVp4/9uK6LYt5M4eCKOHOe0E/dk02y', 'User');

INSERT INTO hoteis (nome, localizacao, descricao) VALUES
    ('Hotel Solar do Imperio',  'Petropolis, RJ', 'Charme colonial no coracao da cidade imperial.'),
    ('Pousada Quitandinha',     'Petropolis, RJ', 'Aos pes do Palacio Quitandinha.'),
    ('Hotel Vale das Videiras', 'Petropolis, RJ', 'Vista para a serra, clima de montanha.');

INSERT INTO quartos (hotel_id, numero, tipo, preco_por_noite, status) VALUES
    (1, 101, 0, 220.00, 1),
    (1, 102, 0, 220.00, 0),
    (1, 103, 1, 380.00, 0),
    (1, 201, 1, 380.00, 1),
    (1, 301, 2, 650.00, 0),
    (2,   1, 0, 180.00, 1),
    (2,   2, 0, 180.00, 0),
    (2,   3, 0, 180.00, 0),
    (2,   4, 1, 320.00, 1),
    (2,   5, 1, 320.00, 0),
    (3, 101, 0, 200.00, 0),
    (3, 102, 1, 350.00, 0),
    (3, 103, 1, 350.00, 0),
    (3, 201, 2, 700.00, 1),
    (3, 202, 2, 700.00, 0);

-- Cobre os quatro estados possiveis: confirmada, cancelada e finalizada
INSERT INTO reservas (data_checkin, data_checkout, status, usuario_id, quarto_id) VALUES
    ('2026-09-10 14:00:00', '2026-09-14 12:00:00', 1, 2,  1),
    ('2026-06-01 14:00:00', '2026-06-05 12:00:00', 1, 2,  9),
    ('2026-10-05 14:00:00', '2026-10-10 12:00:00', 1, 3, 14),
    ('2026-08-25 14:00:00', '2026-08-27 12:00:00', 2, 3,  3),
    ('2026-11-01 14:00:00', '2026-11-03 12:00:00', 1, 4,  6),
    ('2026-07-15 14:00:00', '2026-07-18 12:00:00', 3, 4, 12),
    ('2026-09-01 14:00:00', '2026-09-04 12:00:00', 1, 4,  4);


-- Select de usuarios

SELECT 'usuarios' AS tabela, count(*) AS registros FROM usuarios
UNION ALL SELECT 'hoteis',   count(*) FROM hoteis
UNION ALL SELECT 'quartos',  count(*) FROM quartos
UNION ALL SELECT 'reservas', count(*) FROM reservas;
