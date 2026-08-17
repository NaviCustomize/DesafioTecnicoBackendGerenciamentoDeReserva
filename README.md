# Sistema de Gerenciamento de Reservas - Backend & Frontend

Plataforma de reserva de quartos de hotel, com autenticação por papel, verificação de conflito de datas e notificações assíncronas. O ecossistema une um frontend em React a uma API em .NET 8 com suporte a mensageria.

- Backend: [DesafioTecnicoBackendGerenciamentoDeReserva](https://github.com/NaviCustomize/DesafioTecnicoBackendGerenciamentoDeReserva)
- Frontend: [DesafioTecnicoFrontendGerenciamentoDeReserva](https://github.com/NaviCustomize/DesafioTecnicoFrontendGerenciamentoDeReserva)

## Arquitetura

O backend foi desenvolvido seguindo os princípios de Domain-Driven Design (DDD), estruturado em camadas para separar as responsabilidades entre Domínio, Aplicação, Infraestrutura e Apresentação. O acesso a dados usa Dapper, com SQL escrito à mão nos repositórios.

No frontend, a comunicação com a API fica concentrada na camada de serviços: as telas não conhecem endereço nem método HTTP, o que permite trocar a implementação sem mexer na interface.

## Funcionalidades Técnicas

* **Controle de Acesso:** autenticação por JWT com o papel dentro do token. Endpoints administrativos exigem papel de Admin, e há verificação de propriedade para que um usuário não leia nem altere dados de outro.
* **Mensageria:** integração com RabbitMQ para processamento assíncrono dos eventos de reserva, com exchange do tipo topic, confirmação manual, dead-letter queue e um worker de lembrete de check-in.
* **Persistência:** repositórios implementados com Dapper para Usuários, Hotéis, Quartos, Reservas e Notificações, todos com exclusão lógica e colunas de auditoria.
* **Qualidade:** 109 testes, unitários nos serviços de domínio com Moq e de integração nos endpoints com WebApplicationFactory. Uso de DTOs para transferência de dados e middleware global de tratamento de erro.
* **Frontend:** interface em React 19 com TypeScript, usando Vite, Axios, React Router com rotas protegidas por papel, React-Bootstrap e styled-components.
* **Acessibilidade:** VLibras, alto contraste, ajuste de fonte, navegação por teclado e lint de acessibilidade no oxlint.

## Requisitos de Dados (SQL)

**Atenção:** é obrigatório rodar o script SQL de criação de tabelas e inserção de dados (`ScriptSQL.sql`, incluso no repositório do backend) antes de iniciar a aplicação, para garantir a integridade das chaves estrangeiras e do catálogo de hotéis.

## Como rodar

### Backend

Precisa de .NET 8 e PostgreSQL. O RabbitMQ é opcional: sem ele a API sobe normalmente, apenas registra um aviso no log e não publica os eventos.

```bash
psql -U postgres -f ScriptSQL.sql
```

As credenciais ficam em User Secrets, fora do controle de versão:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=Gerenciamento_reserva;Username=postgres;Password=SUA_SENHA"
```

```bash
dotnet user-secrets set "JwtSettings:Secret" "uma-chave-com-no-minimo-32-caracteres"
```

Faltam ainda `JwtSettings:Issuer`, `JwtSettings:Audience` e `JwtSettings:ExpireMinutes`. Para subir o RabbitMQ, `docker compose up -d`. Depois:

```bash
dotnet run --launch-profile http
```

A API fica em http://localhost:5075 e o Swagger em http://localhost:5075/swagger.

### Frontend

Precisa de Node 18 ou superior e da API rodando.

```bash
npm install
```

Copie o `.env.example` para `.env` e execute:

```bash
npm run dev
```

A aplicação sobe em http://localhost:5173. A porta é fixa porque o backend libera exatamente essa origem no CORS. Com `VITE_MOCK=true` a interface responde às chamadas localmente, com dados de exemplo, sem precisar do backend.


## Regras de Negócio

* **Exclusão lógica:** nenhum registro é apagado do banco. A coluna `excluido_em` recebe a data e o registro sai das consultas, o que mantém o histórico e evita erro de chave estrangeira ao remover um cadastro que já possui reservas. Usuário excluído também perde o acesso, porque a busca por e-mail no login filtra apenas os ativos.
* **Check-in e check-out:** são política do hotel, não escolha do hóspede, com entrada às 14h e saída às 12h. A API normaliza as datas recebidas, então o cliente envia apenas o dia. Como consequência, o mesmo quarto atende hóspedes diferentes no mesmo dia.
* **Conflito de datas:** a sobreposição de períodos é recusada na criação e na edição da reserva, o que permite reservar o mesmo quarto para períodos diferentes.
* **Status do quarto:** acompanha as reservas, ficando Reservado enquanto houver reserva em aberto e voltando para Disponível quando não houver. Ele não bloqueia uma nova reserva, quem recusa sobreposição é a verificação de conflito.
* **Papéis:** o usuário comum enxerga apenas as próprias reservas e o histórico. O administrador tem acesso ao painel de indicadores e à gestão de hotéis, quartos, reservas, notificações e usuários.
* **Conta:** troca de senha e encerramento de conta exigem a senha atual, mesmo com a sessão aberta. O administrador pode desativar e reativar contas.

## Autor

Kenji Neves Okubo Lima
