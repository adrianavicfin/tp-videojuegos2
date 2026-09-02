# 🌌 CosmosCritters

> **Proyecto Integrador** — Programación de Videojuegos II
> **Universidad Nacional de Hurlingham (UNAHUR)** — 2do Cuatrimestre 2026 (Comisión 1)
> **Profesor:** Reynaga, Ignacio Daniel

---

## 👥 Integrantes del Equipo

* **Bruque López, Damián**
* **Estrada, Iván**
* **Finoquetto, Adriana Victoria**

---

## 🎮 Game Design Document (GDD)

### Nombre del videojuego

Cosmos Critters ([link al repo](https://github.com/adrianavicfin/tp-videojuegos2))

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

### 📊 Diagrama UML Inicial (GDD)

![Diagrama UML Inicial](diagrama_uml_inicial.png)

---

### 📊 Diagrama UML de Implementación (Hito 1)

```mermaid
classDiagram
    %% Core & Inversion of Control
    class GameManager {
        -GameManager s_instance$
        +Instance$ GameManager
        -Awake() void
        -RegisterDependencies(IoCContainer container) void
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

    %% Application / Rules
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

    class ICountdownTimer {
        <<interface>>
        +float RemainingTime
        +float TotalDuration
        +event Action~float~ OnTick
        +event Action OnFinished
        +Start(float duration) void
        +Pause() void
        +Resume() void
        +Stop() void
    }

    class CountdownTimer {
        -float _remainingTime
        -float _totalDuration
        -bool _isRunning
        +Start(float duration) void
        +Tick(float deltaTime) void
        +Stop() void
    }

    class ICharacterAction {
        <<interface>>
        +string ActionName
        +CanExecute(Character user) bool
        +Execute(Character user, Character target) void
    }

    class ActionMove {
        +Vector2 Direction
        +float Distance
        +CanExecute(Character user) bool
        +Execute(Character user, Character target) void
    }

    class ActionShoot {
        +float Angle
        +float Power
        +int Damage
        +CanExecute(Character user) bool
        +Execute(Character user, Character target) void
    }

    class ActionAbility {
        +string AbilityName
        +int HealAmount
        +CanExecute(Character user) bool
        +Execute(Character user, Character target) void
    }

    %% Data
    class HeroDataSO {
        -string _heroName
        -int _maxHealth
        -float _moveSpeed
        -HeroRole _role
        +string HeroName
        +int MaxHealth
        +float MoveSpeed
        +HeroRole Role
    }

    %% Presentation / Entities
    class Character {
        <<abstract>>
        #int _currentHealth
        #int _maxHealth
        #string _characterName
        +int CurrentHealth
        +bool IsDead
        +TakeDamage(int amount) void
        +Heal(int amount) void
        +StartTurn()* void
        +EndTurn()* void
    }

    class Hero {
        -HeroDataSO _heroData
        +int SlotIndex
        +Initialize(HeroDataSO data, int slotIndex) void
        +ExecuteAction(ICharacterAction action, Character target) void
        +StartTurn() void
        +EndTurn() void
    }

    class Enemy {
        <<abstract>>
        +StartTurn() void
        +EndTurn() void
        +ExecuteAITurn()* void
    }

    class Boss {
        -int _currentPhase
        -int _totalPhases
        +int CurrentPhase
        +ExecuteAITurn() void
        +TakeDamage(int amount) void
    }

    class Minion {
        -float _attackDamage
        +ExecuteAITurn() void
    }

    %% Relationships
    GameManager ..> IoCContainer : registers services
    TurnManager ..> ICountdownTimer : depends on (DIP)
    CountdownTimer ..|> ICountdownTimer : implements
    TurnManager o-- Character : manages turn queue

    Character <|-- Hero : inherits
    Character <|-- Enemy : inherits
    Enemy <|-- Boss : inherits
    Enemy <|-- Minion : inherits

    Hero o-- HeroDataSO : configured by
    Hero ..> ICharacterAction : executes commands

    ActionMove ..|> ICharacterAction : implements
    ActionShoot ..|> ICharacterAction : implements
    ActionAbility ..|> ICharacterAction : implements
```

---

## 📁 Estructura del Código

```
Assets/src/
├── Core/                  # GameManager (Singleton), IoCContainer (Inversión de Control)
├── Data/                  # ScriptableObjects (HeroDataSO), Enums (HeroRole, TurnPhase)
├── Application/           # TurnManager, Timers, Comandos de Acción (Move, Shoot, Ability)
└── Presentation/          # Entidades en escena (Character, Hero, Enemy, Boss, Minion) y UI
```

---

## 🚀 Requisitos e Instalación

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/adrianavicfin/tp-videojuegos2.git
   ```
2. Abrir **Unity Hub** y añadir el proyecto seleccionando la carpeta `CosmosCritters/`.
3. Abrir con la versión oficial de la materia: **Unity 2022.3.62f3**.
