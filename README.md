# Run Mike Run

> Prototipo de juego 3D fan-made hecho en Unity 6, inspirado en Monsters, Inc.

Proyecto educativo y no oficial, sin afiliación con Disney ni Pixar.

El juego nació en Unity 4.6 y fue adaptado a Unity 6 con una reconstrucción del terreno, mejora de UI y ajustes para compatibilidad moderna. La versión actual está enfocada en gameplay de un jugador y en la base del proyecto original.

## Estado del proyecto

- Juego 3D en tercera persona
- Escenas principales: menú y nivel de juego
- Motor: Unity 6 usando [Built-in Render Pipeline](https://docs.unity3d.com/Manual/builtin-render-pipeline.html)
- Modo actual: un jugador
- Multijugador: no implementado en esta versión

---

## Requisitos

- Unity Hub
- Unity 6.x (se recomienda una versión estable LTS; el proyecto fue probado en Unity 6.x estable)
- Para un proyecto de producción, conviene evitar versiones alpha o preview.
- macOS o Windows con soporte de Unity 6

---

## Cómo abrir el proyecto para desarrollo

Este documento está pensado para desarrolladores que van a abrir, compilar o modificar el proyecto localmente.

1. Instalar Unity Hub.
2. Instalar una versión estable de Unity 6.x.
3. Abrir la carpeta del proyecto desde la raíz que incluye `Assets`, `ProjectSettings` y `Packages`.
4. Desde Unity, abrir la escena principal de desarrollo y presionar Play desde ahí.
5. Si hace falta, revisar `File > Build Profiles` para confirmar que la escena principal está incluida antes de compilar.

> Este proyecto no está orientado a que alguien lo abra solo para jugar; su propósito es servir como base de desarrollo y mantenimiento del juego.

---

## Cómo jugar

### Controles

| Tecla / Entrada | Acción |
| --- | --- |
| `W`, `A`, `S`, `D` | Moverse |
| `Shift` | Correr |
| `Espacio` | Saltar |
| Mouse | Mirar / rotar cámara |
| `Esc` | Pausar o volver al menú |

### Objetivo

- Recolectar el tubo de gritos.
- Llegar a la puerta sin estar contaminado.
- Si el tiempo termina sin ganador, todos pierden.

### Objetos del mapa

| Objeto | Efecto |
| --- | --- |
| Tubo de gritos | Requisito para ganar |
| Puerta | Objetivo final |
| Zapatos | Aumentan velocidad |
| Medias / zoquetes | Contaminan |
| Casco | Protege del contagio |
| Ducha | Descontamina |

---

## Estructura del proyecto

```text
RunMikeRun/
  Assets/
    Art/
    Editor/
    Resources/
      Environment/
      Models/
      Terrain/
      Textures/
      UI/
    Scenes/
    Scripts/
      Player/
      UI/
      World/
  Packages/
  ProjectSettings/
  README.md
```

### Scripts principales

| Script | Área | Función |
| --- | --- | --- |
| `MenuPrincipal` | UI | Menú principal y nombre del jugador |
| `GameGUI` | UI | HUD del juego |
| `GestionaMultiJugador` | World | Estado del juego, tiempo y sesión |
| `SpawnJugador` | World | Spawneo del personaje principal |
| `SpawnObjetos` | World | Objetos del mapa |
| `ThirdPersonController` | Player | Movimiento, salto y animación |
| `EtiquetaJugador` | Player | Nombre visible sobre el personaje |
| `AmbienteTerreno` | World | Terreno, altura y elementos del ambiente |

---

## Desarrollo

### Built-in Render Pipeline

El proyecto usa el render pipeline clásico de Unity, no URP ni HDRP. Esto es compatible con la base del proyecto, pero conviene respetarlo si se hace trabajo de visual o materiales.

### Bake y reconstrucción del entorno

En la carpeta de editor hay herramientas para preparar el proyecto desde datos legacy:

- `BakeTerreno`
- `BakeArbolesYRocas`
- `ProjectBootstrap`

Estas utilidades ayudan a regenerar o preparar escenas, materiales y elementos del mundo para trabajar en Unity 6.

### Historial legacy

El proyecto original tenía contenido legacy fuera de `Assets/`. Ese contenido no se usa en la versión actual, pero quedó en el historial de git para referencia.

Si en algún momento se necesita recuperar ese material histórico, se puede hacer desde git con el commit previo al corte de legacy.

---

## Build

Para compilar:

1. Abrir `File > Build Profiles`.
2. Asegurarse de incluir `MainMenu` y `Mike_Juego`.
3. Ejecutar el build.

---

## Créditos

Los modelos y recursos del proyecto son parte de la adaptación fan-made del juego original. Algunos assets se tomaron de fuentes externas y se modificaron para encajar en este proyecto.

---

## Roadmap / trabajo pendiente

- finalizar pulido visual del personaje y etiqueta
- revisar detalles de combinación de materiales y FBX legacy
- consolidar la escena de juego y el menú final
- revisar soporte de build para Unity 6 estable

---

## Board de trabajo

- [Tablero de tareas pendientes](https://github.com/users/naza/projects/1)