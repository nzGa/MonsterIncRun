# Run Mike Run

Juego 3D multijugador basado en *Monsters, Inc.*

Desarrollado desde cero con fines educativos, sin experiencia previa en las herramientas utilizadas.

Hecho con:

- **Unity 4.6** (2014): terreno, lógica de red y el juego original.
- **3ds Max 2014**: modelos 3D y animaciones (Mike, fábrica, objetos).
- **Unity 6** (Built-in Render Pipeline): migración. El Terrain de Unity 4 no carga en Unity 6; el paisaje se reconstruye con el heightmap y las posiciones de árboles/rocas. El multijugador todavía no está (un jugador). Menús y HUD pasaron de OnGUI a Canvas.

## Cómo abrirlo en Unity 6

1. Instalá [Unity Hub](https://unity.com/download) o, en Mac, `brew install --cask unity-hub`.
2. Instalá un editor **Unity 6**. Este proyecto se abrió bien con **Unity 6.6 (6000.6.0f1)** Apple Silicon. Unity 6.3 LTS (6000.3.24f1) también sirve si Hub la lista.
3. La primera vez que abre Hub: salteá **Configuración inicial** (*Omitir configuración*) o andá a **Proyectos**. El botón de agregar no está en el asistente de bienvenida.
4. Add project desde disco: la carpeta exacta (tiene que tener `Assets` y `ProjectSettings`).
5. Si macOS pide la Keychain de `github.com`, es la contraseña de login de la Mac, no la de GitHub. *Permitir* / *Permitir siempre*.
6. Si Hub se cuelga instalando un editor: volvé a abrirlo, elegí un Unity 6 que ya esté instalado (por ejemplo 6000.6.0f1) y no arranques otra instalación.
7. La primera apertura tarda: esperá a que compile shaders e importe assets.
8. No le des Play a la escena Untitled por defecto. Abrí `Assets/Scenes/MainMenu` y ahí dale Play.
9. Lista de build: **File → Build Profiles** (o Build Settings). Agregá `MainMenu` (índice 0) y `Mike_Juego` (índice 1). Si no, **Iniciar** falla porque la escena no está en el build profile.
10. Después de **Iniciar**: `WASD` caminar, `Shift` correr, `Espacio` saltar, mouse cámara, `Esc` menú.

Si un FBX no escala bien, el juego usa una cápsula o primitivas para no bloquear el Play.

## Mover árboles y rocas

No existen en la escena hasta que los horneás. Con **Play apagado**:

1. Abrí `Assets/Scenes/Mike_Juego`.
2. Menú **Monster Inc Run → Bake Trees And Rocks Into Scene**.
3. Quedan bajo `Entorno/ArbolesYRocas`. Seleccioná uno y **W** para mover.
4. **Ctrl+S** (Mac: **Cmd+S**) guarda la escena. Esas posiciones quedan para siempre.

En Play el juego usa esos objetos y no duplica. Si vaciás el padre, vuelve a spawnear desde `originalTrees.bytes`.

## Cómo se juega

Hasta **5 jugadores**. Las reglas son simples:

- Gana el **primero** que obtiene un tubo de gritos y llega a la **puerta**.
- La puerta aparece en una **posición aleatoria**.
- Si se acaba el tiempo y nadie llegó a la puerta, **pierden todos** los jugadores conectados (15 minutos por defecto).

Para cruzar la puerta hay que tener el tubo y **no estar contaminado**.

En esta versión hay **modo un jugador**: Mike spawnea local, los ítems y la puerta aparecen al azar, y las medias te contaminan a vos (en el original contaminaban a los oponentes). El multiplayer se puede volver a agregar después.

### Controles (cliente)


| Tecla           | Acción                     |
| --------------- | -------------------------- |
| `W` `A` `S` `D` | Caminar                    |
| `Shift`         | Correr                     |
| `Espacio`       | Saltar                     |
| Mouse           | Cámara                     |
| `Esc`           | Menú (desconectar / salir) |


### Objetos en el mapa

Aparecen al azar cuando arranca el servidor.


| Objeto                        | Qué hace                                                                                                          |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Tubo de gritos**            | Hace falta para ganar. Solo uno por jugador.                                                                      |
| **Puerta**                    | Ganas si tenés tubo y no estás contaminado. Posición aleatoria.                                                   |
| **Zapatos**                   | Aumentan la velocidad.                                                                                            |
| **Medias humanas (zoquetes)** | Contaminan a todos los oponentes. Un contaminado no puede cruzar la puerta hasta descontaminarse en la **ducha**. |
| **Casco**                     | Evita que las medias te contaminen.                                                                               |
| **Ducha**                     | Te descontamina.                                                                                                  |


## Créditos de modelos

Algunos modelos 3D los hice yo (como **Mike**). Otros están tomados de bibliotecas y modificados.

La malla de Mike viene con la boca abierta: el FBX no tiene blendshapes, morpher ni hueso de mandíbula. Una sonrisa cerrada hay que modelarla en 3ds Max (u otra malla / morph); no se puede improvisar moviendo vértices.


| Modelo        | Fuente                                        |
| ------------- | --------------------------------------------- |
| Fábrica       | [3dmodelfree.com](http://www.3dmodelfree.com) |
| Cartel        | [TurboSquid](https://www.turbosquid.com)      |
| Puerta        | [crazy3dfree.com](http://www.crazy3dfree.com) |
| Caja sorpresa | [TurboSquid](https://www.turbosquid.com)      |


Proyecto educativo / fan. No está afiliado a Disney ni Pixar.

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


| Script                  | Carpeta | Rol                          |
| ----------------------- | ------- | ---------------------------- |
| `MenuPrincipal`         | UI      | Canvas: Iniciar / Salir      |
| `GameGUI`               | UI      | HUD Canvas                   |
| `GestionaMultiJugador`  | World   | Timer, pausa, cursor         |
| `SpawnJugador`          | World   | Spawn local de Mike          |
| `SpawnObjetos`          | World   | Puerta, ducha, tubos y cajas |
| `ObtieneObjeto`         | Player  | Ítems y victoria             |
| `ThirdPersonController` | Player  | Movimiento y animaciones     |



## Qué falta

- [ ] Terrain original de Unity 4 (colinas/césped como Terrain): no es portable; por ahora planos texturizados
- [ ] Warnings de import de FBX que quedan (`MaterialLocation.External` en algunos props, split de mesh 65k) — polish
- [ ] Ajuste fino de colores de fábrica/Mike si todavía no coinciden con el original
- [ ] Multijugador (Netcode o Mirror)
- [ ] Build de player macOS/Windows desde **File → Build Profiles**
- [ ] Cambios locales de bootstrap/arte aún no pusheados
