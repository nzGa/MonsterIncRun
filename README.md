# Run Mike Run

Juego 3D basado en *Monsters, Inc.* Proyecto educativo (fan), sin afiliación con Disney ni Pixar.

Empezó en **Unity 4.6** (2014) con terreno, red y multijugador. Los modelos y animaciones salieron de **3ds Max 2014**. Hoy corre en **Unity 6** (Built-in Render Pipeline), en modo un jugador: el Terrain de Unity 4 no carga, así que el paisaje se reconstruye con heightmap y posiciones de árboles/rocas; menús y HUD pasaron de OnGUI a Canvas. El multijugador todavía no está.

---

## Para jugadores

Cómo instalar Unity, abrir el proyecto y jugar.

### Instalar Unity 6

1. Instalá [Unity Hub](https://unity.com/download) o, en Mac, `brew install --cask unity-hub`.
2. Instalá un editor **Unity 6**. Este proyecto se abrió bien con **Unity 6.6 (6000.6.0f1)** Apple Silicon. Unity 6.3 LTS (6000.3.24f1) también sirve si Hub la lista.
3. La primera vez que abre Hub: salteá **Configuración inicial** (*Omitir configuración*) o andá a **Proyectos**. El botón de agregar no está en el asistente de bienvenida.
4. Add project desde disco: la carpeta exacta (tiene que tener `Assets` y `ProjectSettings`).
5. Si macOS pide la Keychain de `github.com`, es la contraseña de login de la Mac, no la de GitHub. *Permitir* / *Permitir siempre*.
6. Si Hub se cuelga instalando un editor: volvé a abrirlo, elegí un Unity 6 que ya esté instalado (por ejemplo 6000.6.0f1) y no arranques otra instalación.
7. La primera apertura tarda: esperá a que compile shaders e importe assets.

### Abrir y darle Play

1. No le des Play a la escena Untitled por defecto. Abrí `Assets/Scenes/MainMenu` y ahí dale Play.
2. Lista de build: **File → Build Profiles** (o Build Settings). Agregá `MainMenu` (índice 0) y `Mike_Juego` (índice 1). Si no, **Iniciar** falla porque la escena no está en el build profile.
3. Después de **Iniciar**: `WASD` caminar, `Shift` correr, `Espacio` saltar, mouse cámara, `Esc` menú.

Si un FBX no escala bien, el juego usa una cápsula o primitivas para no bloquear el Play.

### Cómo se juega

Hasta **5 jugadores** en el diseño original. Las reglas son simples:

- Gana el **primero** que obtiene un tubo de gritos y llega a la **puerta**.
- La puerta aparece en una **posición aleatoria**.
- Si se acaba el tiempo y nadie llegó a la puerta, **pierden todos** los jugadores conectados (15 minutos por defecto).

Para cruzar la puerta hay que tener el tubo y **no estar contaminado**.

En esta versión hay **modo un jugador**: Mike spawnea local, los ítems y la puerta aparecen al azar, y las medias te contaminan a vos (en el original contaminaban a los oponentes). El multiplayer se puede volver a agregar después.

### Controles


| Tecla           | Acción                     |
| --------------- | -------------------------- |
| `W` `A` `S` `D` | Caminar                    |
| `Shift`         | Correr                     |
| `Espacio`       | Saltar                     |
| Mouse           | Cámara                     |
| `Esc`           | Menú (desconectar / salir) |


### Objetos en el mapa

Aparecen al azar cuando arranca la partida.


| Objeto                        | Qué hace                                                                                                          |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Tubo de gritos**            | Hace falta para ganar. Solo uno por jugador.                                                                      |
| **Puerta**                    | Ganas si tenés tubo y no estás contaminado. Posición aleatoria.                                                   |
| **Zapatos**                   | Aumentan la velocidad.                                                                                            |
| **Medias humanas (zoquetes)** | Contaminan a todos los oponentes. Un contaminado no puede cruzar la puerta hasta descontaminarse en la **ducha**. |
| **Casco**                     | Evita que las medias te contaminen.                                                                               |
| **Ducha**                     | Te descontamina.                                                                                                  |


### Créditos de modelos

Algunos modelos 3D fueron hechos a mano y otros están tomados de bibliotecas y modificados:


| Modelo        | Fuente                                        |
| ------------- | --------------------------------------------- |
| Fábrica       | [3dmodelfree.com](http://www.3dmodelfree.com) |
| Cartel        | [TurboSquid](https://www.turbosquid.com)      |
| Puerta        | [crazy3dfree.com](http://www.crazy3dfree.com) |
| Caja sorpresa | [TurboSquid](https://www.turbosquid.com)      |

---

## Para desarrolladores / modificar el proyecto

Estructura, menús de bake, scripts y dónde quedó el respaldo de Unity 4.

### Estructura

```
MonsterIncRun/
  Assets/                      Lo que abre Unity 6
    Scenes/                    MainMenu, Mike_Juego
    Scripts/
      UI/                      Canvas (menú, HUD, timer)
      Player/                  movimiento, cámara, ítems
      World/                   spawn, sesión, modelos, terreno
    Resources/
      UI/                      texturas del menú y HUD
      Models/                  FBX (fábrica, tubo, puerta, caja, ducha, Mike)
      Environment/             árboles, palmeras, rocas
      Terrain/                 heightmap y datos de árboles
      Textures/                texturas de piso, césped, etc.
      Skyboxes/
    Art/                       texturas / sky de respaldo (el editor y algunos scripts las usan)
    Editor/                    bootstrap, bake de terreno/árboles, import de Mike
  Packages/                    Unity 6
  ProjectSettings/
```

Unity solo mira `Assets/`. No hay carpeta `legacy/` en el árbol activo: el juego Unity 6 no la carga.

### Scripts principales


| Script                  | Carpeta | Rol                          |
| ----------------------- | ------- | ---------------------------- |
| `MenuPrincipal`         | UI      | Canvas: Iniciar / Salir      |
| `GameGUI`               | UI      | HUD Canvas                   |
| `GestionaMultiJugador`  | World   | Timer, pausa, cursor         |
| `SpawnJugador`          | World   | Spawn local de Mike          |
| `SpawnObjetos`          | World   | Puerta, ducha, tubos y cajas |
| `ObtieneObjeto`         | Player  | Ítems y victoria             |
| `ThirdPersonController` | Player  | Movimiento y animaciones     |
| `AmbienteTerreno`       | World   | Terrain, árboles y rocas     |


### Bake de terreno y árboles

Con la escena de juego lista (o desde el menú, aunque no esté abierta):

1. **Monster Inc Run → Bake Terrain Into Scene** — deja el Terrain visible en el Editor (datos en `Assets/Resources/Terrain`).
2. **Monster Inc Run → Bake Trees And Rocks Into Scene** — instancia árboles y rocas en `Mike_Juego` para poder moverlos y guardarlos.

Guardá la escena después. No hace falta tener Play activo.

También existe **Monster Inc Run → Bootstrap Project** (escenas de build, materiales, reimport de Mike Legacy).

### Respaldo Unity 4 (historial de git)

La carpeta `legacy/` (Standard Assets, Terrain binario, prefabs y scripts `.js` de Unity 4) **no se importaba** (estaba fuera de `Assets/`) y se sacó del árbol activo para aligerar el repo.

Sigue en el historial de git (último commit que aún la tenía: `79fa273`):

```bash
git show 79fa273:legacy/
# o recuperar la carpeta entera:
git checkout 79fa273 -- legacy/
```

No borra el historial: `git log -- legacy/` sigue mostrando esos commits.

### Build

**File → Build Profiles**: `MainMenu` (0) y `Mike_Juego` (1), después Build.

### Qué falta

- [ ] Ajuste fino de texturas
- [ ] Multijugador (Netcode o Mirror)
- [ ] Build de player macOS/Windows desde **File → Build Profiles**
