# 🌌 CosmosCritters

> **Proyecto Integrador** — Programación de Videojuegos II
>
> **Universidad Nacional de Hurlingham (UNAHUR)** — 2do Cuatrimestre 2026 (Comisión 1)
>
> **Profesor:** Reynaga, Ignacio Daniel

---

## 👥 Integrantes del Equipo

* **Bruque López, Damián**
* **Estrada, Iván**
* **Finoquetto, Adriana Victoria**

---

## 🎮 Game Design Document (GDD)

### Nombre del videojuego

Cosmos Critters

### Pila tecnológica a utilizar

* **Motor de videojuegos:** Unity LTS `2022.3.62f3`
* **Pipeline Gráfico:** Universal Render Pipeline 2D (**URP 2D**)
* **Lenguaje de programación:** C# (.NET Standard)
* **Justificación:** Elegimos trabajar con este motor ya que será utilizado por el docente para enseñar el contenido de la materia, lo que nos permitirá realizar un mejor seguimiento con respecto a nuestro trabajo.

---

### Descripción general

Videojuego táctico en 2d por turnos, diseñado para 2 a 4 jugadores en modalidad local (HotSeat). Inspirado en worms, cada jugador toma el control de un personaje en un escenario con desniveles y plataformas, cooperando para eliminar a un jefe mediante el cálculo de trayectorias de proyectiles, uso estratégico de armas/roles y aprovechamiento del entorno antes de que el jefe o los eventos del espacio los eliminen.

En Cosmos Critters, los jugadores toman el control de una escuadra alienígena asimétrica con estadísticas y habilidades únicas en un entorno cósmico dinámico. Deben coordinar sus disparos aprovechando la gravedad de múltiples planetoides, la destrucción de coberturas según su resistencia y sobrevivir a eventos cósmicos periódicos (como lluvias de meteoritos) para derrotar a la IA enemiga.

---

### Gameplay

Los jugadores estarían en modo cooperativo, tomando acciones por turno en un tiempo limitado de forma secuencial, contra un enemigo en común. Con posibilidad de acciones de movimiento, ataques de proyectiles o acciones únicas de cada personaje.

La interacción de cada escenario sería similar entre sí, con efectos que alteren las acciones de movimiento o proyectiles, según cómo afecte la física en el momento, modificando la física de los personajes y el resultado de sus acciones según los elementos en pantalla.

Ocurrirán eventos que alteren el escenario en el espacio donde se puede mover y con efectos que podrían modificar las físicas de su cercanía o del escenario completo.

---

### Estilo visual

El estilo visual sería un diseño visual sci-fi 2d, el juego se desarrolla en el espacio con un estilo artístico cartoon animado, un estilo caricaturesco.
La perspectiva del juego se basaría en una cámara centrada en el personaje que tiene el turno en el momento, o en el jefe. Con un formato side-scroll. Con la interfaz alternando entre un personaje y otro para mostrar las acciones posibles.

Ambientado en zonas del espacio, alternando entre escenarios de cuerpos celestiales como meteoritos, planetas o agujeros negros, hasta escenarios internos de naves o estructuras artificiales.

#### Inspiraciones (jugabilidad y diseño visual)

* Worms
* Angry Birds
* Alien hominid
* Spore
* Risk of rain 2
* Astroneer

---

### Posibles ideas a implementar según el desarrollo

* Progresión de niveles
* Valor de resistencia según el objeto del mapa a romper

---

### Matriz MDA

#### Estética:

* Tensión por el reloj (countdown)
* Cooperación/compañerismo en un único dispositivo
* Suspenso e incertidumbre por la aleatoriedad de los eventos cósmicos, con los meteoritos por ejemplo.
* Inmersión cósmica/alienígena a través de la narrativa fantasiosa.

#### Dinámicas:

* Acciones afectadas por las físicas del mapa
* Manipulación estratégica del entorno.
* Coordinación/estrategias a partir del rol de cada personaje y uso de habilidades (curar a un compañero, poner un escudo para lluvia de meteoritos, etc.)
* Gestión de tiempo de acción en el turno
* Momentos de humor/caos compartidos o por humillación

#### Mecánicas:

* Turnos con temporizador regresivo (para moverse y para ejecutar el disparo)
* Gravedad de los planetas
* Efecto de empuje
* //Sistema de Físicas
* Personajes diferenciados por habilidades
* Entorno destructible y resistencia de materiales
* Eventos de entorno (ejemplo, lluvia de meteoritos)
* Boss con IA.
* Sistema de vida
* Sistema de armas
* Proyectiles con físicas y cálculo de trayectorias
* Desplazamiento
* Control de cámara
* Límites del mapa (zona de muerte instantánea)

---

### Gráfico de Relaciones de la Matriz MDA

```mermaid
flowchart LR
    subgraph A["ESTÉTICA"]
        A1["Tensión por el reloj (countdown)"]
        A2["Cooperación/compañerismo en un único dispositivo"]
        A3["Suspenso e incertidumbre por la aleatoriedad de los eventos cósmicos, con los meteoritos por ejemplo."]
        A4["Inmersión cósmica/alienígena a través de la narrativa fantasiosa."]
    end

    subgraph D["DINÁMICAS"]
        D1["Acciones afectadas por las físicas del mapa"]
        D2["Manipulación estratégica del entorno."]
        D3["Coordinación/estrategias a partir del rol de cada personaje y uso de habilidades (curar a un compañero, poner un escudo para lluvia de meteoritos, etc.)"]
        D4["Gestión de tiempo de acción en el turno"]
        D5["Momentos de humor/caos compartidos o por humillación"]
    end

    subgraph M["MECÁNICAS"]
        M1["Turnos con temporizador regresivo (para moverse y para ejecutar el disparo)"]
        M2["Gravedad de los planetas"]
        M3["Efecto de empuje"]
        M4["//Sistema de Físicas"]
        M5["Personajes diferenciados por habilidades"]
        M6["Entorno destructible y resistencia de materiales"]
        M7["Eventos de entorno (ejemplo, lluvia de meteoritos)"]
        M8["Boss con IA."]
        M9["Sistema de vida"]
        M10["Sistema de armas"]
        M11["Proyectiles con físicas y cálculo de trayectorias"]
        M12["Desplazamiento"]
        M13["Control de cámara"]
        M14["Límites del mapa (zona de muerte instantánea)"]
    end

    %% Relaciones Estética -> Dinámicas
    A1 --> D4
    A2 --> D2
    A2 --> D3
    A3 --> D5
    A4 --> D1
    A4 --> D2

    %% Relaciones Dinámicas -> Mecánicas
    D1 --> M2
    D1 --> M4
    D1 --> M11
    D1 --> M12
    D1 --> M13

    D2 --> M2
    D2 --> M6

    D3 --> M5
    D3 --> M9
    D3 --> M10

    D4 --> M1

    D5 --> M3
    D5 --> M6
    D5 --> M7
    D5 --> M8
    D5 --> M14
```

---

## 🏛️ Diagramas UML

### 📊 1. Diagrama UML Inicial (GDD)

![Diagrama UML Inicial](diagrama_uml_inicial.png)

---

### 📊 2. Diagrama UML de Arquitectura e Implementación (Hito 1 & Hito 2)

Diagrama completo que refleja la totalidad del código en `Assets/src/` organizado en `/Data`, `/Rules`, `/Visual` y `/Core`:

```mermaid
classDiagram
    %% Core & Inversion of Control
    class GameManager {
        -GameManager s_instance$
        +Instance$ GameManager
        +MatchSettings CurrentMatchSettings
        -Awake() void
        -RegisterDependencies(IoCContainer container) void
        +SetMatchSettings(MatchSettings settings) void
    }

    class IoCContainer {
        -IoCContainer s_instance$
        +Instance$ IoCContainer
        +AddSingleton(Type service, Type impl) IoCContainer
        +AddScoped(Type service, Type impl) IoCContainer
        +AddTransient(Type service, Type impl) IoCContainer
        +Resolve~T~() T
        +Inject(object target) void
    }

    %% Data Layer (Pure C# & ScriptableObjects)
    class CharacterStats {
        +string CharacterName
        +int MaxHealth
        +int CurrentHealth
        +float MoveSpeed
        +float JumpForce
        +bool IsDead
        +event Action~int,int~ OnHealthChanged
        +event Action OnDied
        +ApplyDamage(int amount) void
        +Heal(int amount) void
    }

    class MatchSettings {
        +List~HeroDataSO~ SelectedHeroes
        +int SelectedMapIndex
        +float TurnDuration
        +SetSelectedHeroes(List~HeroDataSO~ heroes) void
        +SetMapIndex(int mapIndex) void
        +SetTurnDuration(float duration) void
    }

    class HeroDataSO {
        -string _heroName
        -int _maxHealth
        -float _moveSpeed
        -HeroRole _role
        +string HeroName
        +int MaxHealth
        +HeroRole Role
    }

    class WeaponDataSO {
        -string _weaponName
        -int _baseDamage
        -float _explosionRadius
        -float _knockbackForce
        -GameObject _projectilePrefab
        +int BaseDamage
        +float ExplosionRadius
        +float KnockbackForce
        +GameObject ProjectilePrefab
    }

    class TerrainMaterialSO {
        -string _materialName
        -int _damageResistance
        +string MaterialName
        +int DamageResistance
    }

    %% Rules Layer (Logic, Presenters & Commands)
    class TurnManager {
        -Queue~Character~ _turnQueue
        -ICountdownTimer _turnTimer
        -Character _activeCharacter
        -TurnPhase _currentPhase
        +Construct(ICountdownTimer turnTimer) void
        +AdvanceToNextTurn() void
        +EndCurrentTurn() void
        +NotifyActionExecuting() void
        +NotifyActionResolved() void
    }

    class PlayerHUDPresenter {
        -IPlayerHUDView _view
        -CharacterStats _model
        -ICountdownTimer _turnTimer
        +SetTurnActive(bool isActive) void
        +Dispose() void
    }

    class MainMenuController {
        -List~HeroDataSO~ _availableHeroes
        -List~HeroDataSO~ _selectedHeroes
        -int _selectedMapIndex
        -float _turnDuration
        +ToggleSelectHero(HeroDataSO hero) void
        +SelectMap(int mapIndex) void
        +SetTurnDuration(float duration) void
        +StartMatch() void
    }

    class ICountdownTimer {
        <<interface>>
        +float RemainingTime
        +event Action~float~ OnTick
        +event Action OnFinished
        +Start(float duration) void
        +Stop() void
    }

    class ICharacterAction {
        <<interface>>
        +string ActionName
        +CanExecute(Character user) bool
        +Execute(Character user, Character target) void
    }

    class IDamageable {
        <<interface>>
        +int CurrentHealth
        +int MaxHealth
        +bool IsDead
        +TakeDamage(int amount) void
    }

    class IGravityAffected {
        <<interface>>
        +Rigidbody2D Rigidbody
        +Transform Transform
        +ApplyGravitationalPull(Vector2 force) void
        +AlignWithSurface(Vector2 upDirection) void
    }

    class IPlayerHUDView {
        <<interface>>
        +SetCharacterName(string name) void
        +UpdateHealth(int current, int max) void
        +SetTurnActiveState(bool isActive) void
        +UpdateCountdown(float remainingSeconds) void
    }

    class Ability {
        <<abstract>>
        +string AbilityName
        +int CooldownTurns
        +int CurrentCooldown
        +bool IsReady
        +Trigger(Character user, Character target) void
        +TickCooldown() void
        #ExecuteEffect(Character user, Character target)* void
    }

    class HealAbility {
        +int HealAmount
        #ExecuteEffect(Character user, Character target) void
    }

    class ShieldAbility {
        +int ShieldPoints
        #ExecuteEffect(Character user, Character target) void
    }

    class TeleportAbility {
        +Vector2 TargetPosition
        #ExecuteEffect(Character user, Character target) void
    }

    class ActionShoot {
        +float Angle
        +float Power
        +int Damage
        +Execute(Character user, Character target) void
    }

    class ActionMove {
        +Vector2 Direction
        +float Distance
        +Execute(Character user, Character target) void
    }

    %% Visual Layer (Unity MonoBehaviours, Physics & UI)
    class Character {
        <<abstract>>
        +CharacterStats Stats
        +TakeDamage(int amount) void
        +Heal(int amount) void
        +ApplyGravitationalPull(Vector2 force) void
        +AlignWithSurface(Vector2 upDirection) void
        +StartTurn()* void
        +EndTurn()* void
    }

    class Hero {
        +HeroDataSO HeroData
        +Ability SecondaryAbility
        +ExecuteAction(ICharacterAction action, Character target) void
        +ExecuteSecondaryAbility(Character target) void
    }

    class Enemy {
        <<abstract>>
        +ExecuteAITurn()* void
    }

    class Boss {
        -int _currentPhase
        +ExecuteAITurn() void
    }

    class Minion {
        +ExecuteAITurn() void
    }

    class GravityBody {
        -float _gravityRadius
        -float _gravityForce
        -ApplyRadialGravity() void
    }

    class Projectile {
        -int _damage
        -float _explosionRadius
        -float _knockbackForce
        +Launch(Vector2 direction, float power, Character owner) void
        +Explode() void
    }

    class DestructibleCover {
        -TerrainMaterialSO _materialData
        -int _currentIntegrity
        +TakeDamage(int amount) void
    }

    class KillZoneView {
        -OnTriggerExit2D(Collider2D other) void
    }

    class PlayerHUDView {
        +SetCharacterName(string name) void
        +UpdateHealth(int current, int max) void
        +SetTurnActiveState(bool isActive) void
        +UpdateCountdown(float remainingSeconds) void
    }

    %% Relationships
    GameManager ..> IoCContainer : registers
    GameManager o-- MatchSettings : persists
    MainMenuController ..> GameManager : passes MatchSettings
    TurnManager ..> GameManager : reads MatchSettings
    TurnManager ..> ICountdownTimer : depends on (DIP)
    TurnManager o-- Character : manages queue

    PlayerHUDPresenter ..> CharacterStats : observes (Model)
    PlayerHUDPresenter ..> IPlayerHUDView : updates (View)
    PlayerHUDView ..|> IPlayerHUDView : implements

    Character ..|> IDamageable : implements
    Character ..|> IGravityAffected : implements
    Character o-- CharacterStats : delegates stats
    Character <|-- Hero : inherits
    Character <|-- Enemy : inherits
    Enemy <|-- Boss : inherits
    Enemy <|-- Minion : inherits

    Hero o-- HeroDataSO : configured by
    Hero o-- Ability : possesses
    Hero ..> ICharacterAction : executes

    Ability <|-- HealAbility : inherits
    Ability <|-- ShieldAbility : inherits
    Ability <|-- TeleportAbility : inherits

    ActionShoot ..|> ICharacterAction : implements
    ActionMove ..|> ICharacterAction : implements

    Projectile ..|> IGravityAffected : implements
    DestructibleCover ..|> IDamageable : implements
    DestructibleCover o-- TerrainMaterialSO : configured by

    GravityBody ..> IGravityAffected : attracts
    Projectile ..> IDamageable : damages
    Projectile ..> Hero : knockback (Friendly Fire)
    KillZoneView ..> Character : instant kill
    KillZoneView ..> Projectile : destroys
```

---

## 📁 Estructura del Código (Hito 2)

El proyecto está estrictamente organizado según las pautas de la cátedra en:

```
Assets/src/
├── Data/                  # Datos, modelos puros y ScriptableObjects sin lógica de motor
│   ├── Enums/             # HeroRole, TurnPhase
│   ├── Models/            # CharacterStats, MatchSettings
│   └── ScriptableObjects/ # HeroDataSO, WeaponDataSO, TerrainMaterialSO
│
├── Rules/                 # Lógica de partida, controladores, acciones y presentadores
│   ├── Abilities/         # Ability, HealAbility, ShieldAbility, TeleportAbility
│   ├── Actions/           # ActionMove, ActionShoot, ActionAbility
│   ├── Controllers/       # MainMenuController
│   ├── Interfaces/        # ITimer, ICountdownTimer, ICharacterAction, IDamageable, IGravityAffected, IPlayerHUDView
│   ├── Presenters/        # PlayerHUDPresenter
│   └── TurnManager.cs, CountdownTimer.cs, StopwatchTimer.cs
│
├── Visual/                # Componentes MonoBehaviours dependientes de Unity (Entidades, Físicas, UI)
│   ├── Entities/          # Character, Hero, Enemy, Boss, Minion
│   ├── Environment/       # DestructibleCover
│   ├── Physics/           # GravityBody, Projectile, KillZoneView
│   └── UI/                # PlayerHUDView
│
└── Core/                  # IoCContainer (Inversión de Control) y GameManager
```

---

## 🚀 Requisitos e Instalación

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/adrianavicfin/tp-videojuegos2.git
   ```
2. Abrir **Unity Hub** y añadir el proyecto seleccionando la carpeta `CosmosCritters/`.
3. Abrir con la versión oficial de la materia: **Unity 2022.3.62f3**.
