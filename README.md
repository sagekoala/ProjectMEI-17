# Wizard Graveyard — SGI-MIRI VR Project

Aplicación de Realidad Virtual para **Meta Quest 3** hecha con **Unity + OpenXR**, para la práctica de SGI (MIRI, Q1).

- **Autores:** [COMPLETAR: nombres del grupo]
- **Vídeo de demostración:** [COMPLETAR: enlace o nombre del fichero entregado]

## 1. Objetivo de la aplicación

El usuario es un mago en un cementerio colonial. Ve su propio cuerpo de mago en primera persona y sus manos siguen a los mandos. Con su magia puede:

- **Levitar** lápidas y objetos del cementerio para moverlos, rotarlos y escalarlos.
- **Lanzar hechizos** que impactan contra la escena.
- **Desplazarse** caminando, con el teletransporte de la plantilla o con un **orbe de teletransporte** propio.
- **Configurar** el hechizo y la locomoción desde un **grimorio** (menú 3D) que aparece sobre la mano izquierda.

Las metáforas de interacción están elegidas para encajar con la temática de mago:

- La selección y manipulación es **telequinesis**: apuntar con la mano, levitar el objeto y girarlo con la muñeca.
- La navegación propia **lanza un orbe mágico** y lleva al usuario a donde cae.
- El control del sistema se hace desde un **libro de hechizos** que el mago lleva en la mano.

## 2. Cómo abrir y ejecutar el proyecto

1. Abrir la carpeta del proyecto con **Unity 6000.5.0f1**. Los paquetes (XR Interaction Toolkit 3.5.1, OpenXR, Meta OpenXR, Input System, URP) se resuelven desde `Packages/manifest.json`.
2. Abrir la escena `Assets/Scenes/Colonial_Graveyard.unity`. Es la escena inicial de la build.
3. Para las gafas: plataforma **Android**, con las Meta Quest 3 conectadas, **Build And Run**.

## 3. Manual de uso

### Controles

| Mando | Control | Acción |
|---|---|---|
| Izquierdo | **Botón Menú** (≡) | Abrir / cerrar el grimorio (en el editor: **F2**) |
| Izquierdo | **Gatillo** (mantener) | Levitar el objeto al que se apunta con la mano izquierda |
| Izquierdo | Girar la **muñeca** mientras se levita | Rotar el objeto levitado |
| Derecho | **Grip** (mantener) mientras se levita + separar/juntar las manos | Escalar el objeto levitado (entre 0,25× y 4×) |
| Derecho | **Gatillo** | Lanzar un hechizo |
| Derecho | **Botón B** | Lanzar el orbe de teletransporte |
| Izquierdo | **Stick** | Caminar (por defecto) o teletransporte + giro (configurable en el grimorio) |
| Derecho | **Stick** adelante / lados | Teletransporte con arco (plantilla XRI) / girar |
| Derecho | **Botón A** | Saltar (salto simple de 0,4 m) |
| Cualquiera | **Grip** con la mano pegada a una pared + tirar | Escalar la pared |

### Selección y manipulación (levitación)

1. Apunta con la mano izquierda a un objeto levitable. Las lápidas pequeñas junto al punto de inicio y el cubo de prueba lo son. Un **rayo morado** sale de la mano y el objeto se **tiñe de morado**.
2. Mantén el **gatillo izquierdo**: el objeto flota delante de la mano y la sigue (**traslación**). El rayo y el objeto pasan a **dorado**.
3. **Rotación:** gira la muñeca izquierda y el objeto gira igual. Para girarlo más, suéltalo y vuelve a cogerlo.
4. **Escalado:** mantén el **grip derecho** y separa las manos para agrandarlo, o júntalas para encogerlo.
5. Suelta el gatillo y el objeto cae con física, conservando su nuevo tamaño.

### Navegación

- **Caminar:** stick izquierdo.
- **Teletransporte de la plantilla:** stick derecho hacia delante, apuntar y soltar.
- **Giro:** stick derecho a los lados, suave o por pasos según el grimorio.
- **Escalar paredes:** las paredes del cementerio bloquean el paso caminando o saltando. Para cruzarlas, acerca una mano a la pared (a menos de 15 cm), mantén el **grip** y tira hacia abajo y hacia ti, alternando manos como en un rocódromo.
- **Orbe de teletransporte (navegación propia):** pulsa **B** y se lanza un orbe dorado que vuela en arco. Si cae en el **suelo** del cementerio, apareces ahí con un destello dorado. Si choca con una pared, una lápida u otro objeto, el destello es **rojo** y no te mueves. Solo puede haber un orbe en el aire a la vez.

### Control: el grimorio

Se abre con el **botón Menú** del mando izquierdo y flota sobre esa mano mirando hacia ti. Se usa apuntando con el **rayo de la mano derecha + gatillo**, o tocándolo con el dedo. Mientras está abierto, los hechizos, la levitación y el orbe quedan en pausa para que los clics no los activen.

| Opción | Efecto |
|---|---|
| Color del hechizo | 4 colores. Cambia el orbe y su impacto; la opción activa lleva un borde blanco. |
| Stick izquierdo: Caminar / Teletransporte | Cambia la función del stick izquierdo |
| Velocidad al caminar − / + | De 0,5 a 6 m/s |
| Giro: Suave / Por pasos | Tipo de giro con el stick |
| Velocidad (o ángulo) de giro − / + | 30–180 °/s en giro suave; 15–90° en giro por pasos |
| Cerrar | Cierra el grimorio |

**Feedback:** los botones se iluminan al apuntarlos y se oscurecen al pulsarlos, y la opción activa se muestra en morado.

## 4. Estructura

Scripts propios en `Assets/Scripts/`:

| Script | Responsabilidad |
|---|---|
| `WizardHandIK` | IK de las manos del mago hacia los mandos |
| `WizardSpellCaster`, `MagicProjectile` | Hechizos y destello de impacto |
| `WizardLevitation` | Selección por rayo, traslación, rotación y escalado del objeto levitado |
| `WizardLevitationFeedback` | Rayo visible y tinte del objeto apuntado o levitado |
| `WizardTeleportOrb`, `TeleportOrbProjectile` | Navegación con el orbe de teletransporte |
| `WizardLocomotionSettings` | Acceso en tiempo de ejecución a las opciones de locomoción del rig XRI |
| `WizardControlMenu` | Grimorio (menú 3D world-space) |

## 5. Disclosure notice (uso de herramientas y recursos externos)

**Recursos de terceros:**

- **Unity VR Template** (`Assets/VRTemplateAssets`) y samples del **XR Interaction Toolkit 3.5.1** (Starter Assets, Hands Interaction Demo): rig XR, locomoción base (movimiento continuo, teletransporte, giro) e interactores.
- **Colonial City LittlePack – Church Graveyard Environment**, de *3DShapes* (Unity Asset Store): entorno del cementerio.
- **WizardPBR** (Unity Asset Store) [COMPLETAR: autor]: modelo y animaciones del mago.
- [COMPLETAR: tutoriales, vídeos o documentación que hayáis seguido]

**Uso de IA generativa:**

- Se ha usado **Claude (Anthropic)**, a través de Claude Code, como asistente de programación para:
  - analizar el proyecto respecto al enunciado;
  - implementar el grimorio (`WizardControlMenu`, `WizardLocomotionSettings` y el color de hechizo configurable);
  - implementar la rotación y el escalado del objeto levitado, el feedback de levitación (`WizardLevitationFeedback`) y la navegación con orbe (`WizardTeleportOrb`, `TeleportOrbProjectile`);
  - añadir las lápidas levitables a la escena, configurar la escena de la build y redactar el borrador de este documento;
  - escalar el entorno, ajustar el escalón y el salto del jugador y hacer escalables las paredes.

  Estos cambios están en las ramas `feat/wizard-control-menu`, `feat/levitation-manipulation` y `feat/integration` del repositorio. El código generado ha sido revisado y probado por el grupo: [COMPLETAR: confirmar tras probarlo en las gafas].
- [COMPLETAR: cualquier otro uso de IA, p. ej. en los scripts originales `WizardHandIK`, `WizardSpellCaster`, `MagicProjectile`, `WizardLevitation`]

## 6. Cambios sobre los assets y la plantilla

- El entorno (suelo, iglesia, tumbas, paredes y farolas) está escalado **×1,5** respecto al pack original, porque en las gafas el cementerio parecía una maqueta. El jugador mantiene su escala real.
- Del rig XR se ha bajado el escalón máximo del CharacterController (0,5 → 0,3 m) y el salto (1,25 m con doble salto → 0,4 m simple), para que las paredes sean una barrera.
- Al prefab `Graveyard_wall` del pack se le ha añadido un `ClimbInteractable` de XRI para poder escalar las paredes.

## 7. Limitaciones conocidas

- Las rotaciones grandes requieren soltar y volver a coger el objeto, por el rango de giro de la muñeca.
- El tinte de resaltado sustituye el color base del material mientras dura, en lugar de añadir un brillo.
