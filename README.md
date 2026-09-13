# Run Mike Run

Juego 3D multijugador basado en *Monsters, Inc.*, hecho **desde cero con fines educativos**. Hasta 5 jugadores: el primero que consigue un tubo de gritos y llega a la puerta (en una posición aleatoria) gana.

Lo desarrollé **sin experiencia previa** en las herramientas, ni en modelado 3D ni en videojuegos:

- Terreno e interacciones de red: **Unity 4.6**
- Modelado 3D y animaciones: **Autodesk 3ds Max 2014**

El proyecto está migrado a **Unity 6** (Built-in Render Pipeline), con **modo un jugador** y UI en **Canvas** (uGUI).

## Cómo se juega

Hasta **5 jugadores**. Las reglas son simples:

- Gana el **primero** que obtiene un tubo de gritos y llega a la **puerta**.
- La puerta aparece en una **posición aleatoria**.
- Si se acaba el tiempo y nadie llegó a la puerta, **pierden todos** los jugadores conectados (15 minutos por defecto).

Para cruzar la puerta hay que tener el tubo y **no estar contaminado**.

En esta versión hay **modo un jugador**: Mike spawnea local, los ítems y la puerta aparecen al azar, y las medias te contaminan a vos (en el original contaminaban a los oponentes). El multiplayer se puede volver a agregar después.

### Controles (cliente)

| Tecla | Acción |
| --- | --- |
| `W` `A` `S` `D` | Caminar |
| `Shift` | Correr |
| `Espacio` | Saltar |
| Mouse | Cámara |
| `Esc` | Menú (desconectar / salir) |

### Objetos en el mapa

Aparecen al azar cuando arranca el servidor.

| Objeto | Qué hace |
| --- | --- |
| **Tubo de gritos** | Hace falta para ganar. Solo uno por jugador. |
| **Puerta** | Ganas si tenés tubo y no estás contaminado. Posición aleatoria. |
| **Zapatos** | Aumentan la velocidad. |
| **Medias humanas (zoquetes)** | Contaminan a todos los oponentes. Un contaminado no puede cruzar la puerta hasta descontaminarse en la **ducha**. |
| **Casco** | Evita que las medias te contaminen. |
| **Ducha** | Te descontamina. |

## Créditos de modelos

Algunos modelos 3D los hice yo (como **Mike**). Otros están tomados de bibliotecas y modificados:

| Modelo | Fuente |
| --- | --- |
| Fábrica | [3dmodelfree.com](http://www.3dmodelfree.com) |
| Cartel | [TurboSquid](https://www.turbosquid.com) |
| Puerta | [crazy3dfree.com](http://www.crazy3dfree.com) |
| Caja sorpresa | [TurboSquid](https://www.turbosquid.com) |

Proyecto educativo / fan. No está afiliado a Disney ni Pixar.

## Cómo abrirlo en Unity 6

En esta máquina no hace falta Unity 4.6.

1. Instalá [Unity Hub](https://unity.com/download) y un editor **Unity 6** (6000.0 LTS o más nuevo; Hub puede actualizar el proyecto).
2. Add project → esta carpeta (`MonsterIncRun`).
3. File → Build Settings: escenas `MainMenu` y `Mike_Juego`.
4. Abrí `Assets/Scenes/MainMenu` y dale Play.

Controles: `WASD` caminar, `Shift` correr, `Espacio` saltar, mouse cámara, `Esc` pausa.

Si un FBX no escala bien, el juego usa una cápsula o primitivas para no bloquear el Play.

El build Windows de 2014 (~69 MB) ya no está en el repo: era un `.exe` + datos compilados, no corre en Mac y no se reconstruye desde Unity 6.

## Estructura

```
MonsterIncRun/
  Assets/                      Lo que abre Unity 6
    Scenes/                    MainMenu, Mike_Juego
    Scripts/
      UI/                      Canvas (menú, HUD, timer)
      Player/                  movimiento, cámara, ítems
      World/                   spawn, sesión, modelos
    Resources/
      UI/                      texturas del menú y HUD
      Models/                  FBX (fábrica, tubo, puerta, caja, ducha, Mike)
    Editor/                    import de animaciones legacy de Mike
  Packages/                    Unity 6
  ProjectSettings/
  legacy/                      No se importa (fuera de Assets/)
    unity4-assets/             Standard Assets, Terrain, prefabs binarios, materiales viejos
    unity4-scripts/            UnityScript (.js) y APIs que ya no compilan
```

Unity solo mira `Assets/`. Por eso el Terrain, los prefabs de Unity 4 y los `.js` están en `legacy/`: el editor arranca más limpio y no intenta compilar código muerto.

| Script | Carpeta | Rol |
| --- | --- | --- |
| `MenuPrincipal` | UI | Canvas: Iniciar / Salir |
| `GameGUI` | UI | HUD Canvas |
| `GestionaMultiJugador` | World | Timer, pausa, cursor |
| `SpawnJugador` | World | Spawn local de Mike |
| `SpawnObjetos` | World | Puerta, ducha, tubos y cajas |
| `ObtieneObjeto` | Player | Ítems y victoria |
| `ThirdPersonController` | Player | Movimiento y animaciones |

## Qué falta

- [x] README
- [x] Proyecto Unity 6
- [x] Modo un jugador
- [x] UI con Canvas
- [x] Repo ordenado (`Assets/` vs `legacy/`)
- [ ] Ajuste de escala/colisión de los FBX en el editor
- [ ] Multijugador (Netcode o Mirror, gratis)
- [ ] Build de macOS (File → Build Settings)
