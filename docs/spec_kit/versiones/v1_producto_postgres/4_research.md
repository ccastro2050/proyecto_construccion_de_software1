# Investigación y decisiones — Versión 1: producto + PostgreSQL (C#/ASP.NET Core)

> **Versión 1** · **Lectura opcional** (el porqué de las decisiones del plan,
> con las alternativas que se evaluaron y descartaron). Complementa a
> [3_plan.md](3_plan.md); el orden de trabajo está en [8_tasks.md](8_tasks.md).

---

## D1 — ADO.NET crudo: sin Entity Framework (ni Dapper)

**Alternativas descartadas:** Entity Framework Core (el ORM de .NET) y
Dapper (micro-ORM).
**Decisión:** `NpgsqlConnection` + `NpgsqlCommand` con SQL parametrizado a mano.
**Por qué:** el objetivo es aprender **SQL y arquitectura**, no un ORM. EF
esconde exactamente lo que el curso quiere mostrar (el SQL, el mapeo, las
transacciones); Dapper es razonable pero igual tapa el ciclo
conexión→comando→lector que un estudiante debe ver una vez en la vida.
**Precio asumido:** más líneas por método del repositorio — cada una es
lección.

## D2 — Capas completas desde el día 1 (y no un MVP en un solo archivo)

**Alternativa descartada:** v1 = todo en `Program.cs` con minimal APIs y
refactorizar a capas después.
**Decisión:** controller → servicio → repositorio con interfaces desde v1.
**Por qué:** el valor de la v1 es el **esqueleto** sobre el que crecen las
demás versiones sin reescribir. El criterio de aceptación 6 (probar el
servicio con un repositorio falso, sin PostgreSQL) **solo es posible** si el
servicio depende de una `interface` — la prueba objetiva de que las capas
quedaron bien cortadas.

## D3 — Sin fábrica ni selección de motor: el ensamblador es la DI de Program.cs

**Alternativa descartada:** escribir de una vez la fábrica multi-motor.
**Decisión:** dos registros `AddScoped` que instancian la única combinación
existente (YAGNI con dirección).
**Por qué:** una fábrica con un solo producto es código muerto. La interfaz
`IRepositorioProducto` SÍ se escribe hoy — es la puerta por la que entrará
el segundo motor — pero el mecanismo de selección llega cuando exista algo
que seleccionar (v3). El examen del principio abierto/cerrado será ese: en
v3, solo el ensamblador cambia.

## D4 — La BD completa desde la v1 (la API solo toca `producto`)

**Alternativa descartada:** una BD mínima que crece con cada versión.
**Decisión:** `db/bdfacturas_postgres.sql` crea `bdfacturas` COMPLETA (12 tablas,
triggers, SPs); la regla es que el código de v1 solo puede nombrar
`producto`.
**Por qué:** los estudiantes ya vieron bases de datos — la BD es
**infraestructura dada**; lo que se construye por versiones es la API. Evita
migraciones entre versiones y deja los triggers y SPs de facturación
esperando a la v2. Costo asumido: 11 tablas a la vista que aún no se usan —
por eso la regla se declara explícita en la spec.

## D5 — La validación vive en las PETICIONES (una por verbo)

**Alternativas descartadas:** validar con ifs dentro del controlador, una
clase validadora aparte, o no validar y dejar que la BD rechace.
**Decisión:** tres clases de PETICIÓN (`ProductoCrear`, `ProductoReemplazo`,
`ProductoActualizar`) que DECLARAN sus reglas con anotaciones; ASP.NET
valida y responde 422 con la lista de errores (formato personalizado en
`Program.cs`).
**Por qué:** es la manera idiomática del framework — la petición declara, el
framework hace cumplir — y materializa la semántica de cada verbo: el mismo
body `{"stock": 7}` falla en PUT (le faltan campos) y pasa en PATCH. Bono
didáctico: **el tipo es regla** — `stock` es `int?`, así que un `7.5` o un
`"texto"` caen en 422 sin escribir ni un if.
**Nota de nombre:** estas clases NO son modelos — modelo = clase entidad
(`Modelos/`, en v1 `Producto`). Por eso viven en su propia carpeta
`Peticiones/`: describen lo que LLEGA en cada verbo, no lo que ES.

## D6 — PostgreSQL como primer motor

**Alternativas descartadas:** empezar con SQL Server (el motor "natural"
del ecosistema .NET) o con MariaDB.
**Decisión:** v1 arranca con PostgreSQL 16 en contenedor (alpine, ~50 MB).
**Por qué:** es el motor libre de referencia de la industria, liviano
(arranca en segundos, sin requisitos de RAM) y AMIGO de Docker: ejecuta
solo los scripts montados en `/docker-entrypoint-initdb.d/`, así que el
compose de la v1 queda en DOS servicios sin contenedor inicializador. Y
deja una lección pendiente a propósito: cuando llegue el segundo motor
(SQL Server), que NO tiene ese mecanismo, se entenderá el valor del
patrón inicializador por contraste. Npgsql (el proveedor ADO.NET) es de
primera clase en .NET — el ecosistema no obliga a casarse con su motor.

## D7 — dotnet watch dentro del contenedor (imagen SDK, no runtime)

**Alternativa descartada:** imagen multi-stage con publish (más pequeña,
estilo producción).
**Decisión:** la imagen del SDK corriendo `dotnet watch`, con el código
montado como volumen y `bin/`+`obj/` en volúmenes anónimos.
**Por qué:** el ciclo del curso es guardar → recompila solo → refrescar.
Una imagen de producción optimizada no enseña nada en v1 y rompe ese ciclo.
El matiz de los volúmenes anónimos importa: los compilados de Linux (los
del contenedor) no deben mezclarse con los de Windows (los del IDE del
estudiante).

## D8 — Docker compose desde la v1 (dos servicios)

**Alternativa descartada:** `docker run` a mano y la API por fuera.
**Decisión:** `docker-compose.yml` con `postgres` + `api-facturas` desde
v1 — `docker compose up -d --build` deja todo funcionando.
**Por qué:** el Artículo 4 de la constitución ("un solo comando") es
permanente — y la constitución gana. El compose de v1 **crece por
versiones** (más adelante los otros motores, la API genérica y el front
Flask con Jinja2): la infraestructura también se construye por
incrementos.
