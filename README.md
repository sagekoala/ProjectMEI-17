# Guardián del Cementerio — SGI-MIRI VR Project

Juego de Realidad Virtual para **Meta Quest 3** hecho con **Unity + OpenXR**, para la práctica de SGI (MIRI, Q1).

- **Autores:** [COMPLETAR: nombres del grupo]
- **Vídeo de demostración:** [COMPLETAR: enlace o nombre del fichero entregado]

## 1. Objetivo de la aplicación

Eres un mago que protege un cementerio colonial en la noche de Halloween. Unos duendes quieren destruir las calabazas mágicas que mantienen encendidas las luces protectoras del cementerio. Para salvarlo tienes que:

- **Llevar las calabazas** con telequinesis hasta la **tumba iluminada**. Cada entrega da puntos y energía.
- **Eliminar a los duendes** con tus hechizos antes de que lleguen a las calabazas. Al morir, explotan.
- **Protegerte del fantasma**: flota hacia ti por encima del cementerio. Se destierra apuntándole con la luz del **farol espectral**, que se coge con telequinesis.
- **Mantener la energía** del cementerio por encima del 10% hasta que acabe el tiempo de la ronda (7 minutos). Las luces protectoras se apagan a medida que baja la energía. Si llega a 0, pierdes.

Antes de cada ronda, un **tutorial interactivo** enseña a jugar.

## 2. Decisiones de diseño

Todas las metáforas de interacción se eligieron para que encajen con la fantasía de ser un mago:

| Tarea | Metáfora | Por qué |
|---|---|---|
| Manipulación con propósito | **Farol espectral**: se coge con telequinesis y se sostiene como una linterna; manteniendo **Y** emite una luz verde con niebla que se apunta **girando la muñeca**; su **tamaño** cambia el radio del haz | Cada transformación tiene un uso en el juego: traslación para llevar calabazas, rotación para apuntar el farol, escalado para decidir entre valor y peso (calabazas) o la anchura del haz (farol) |
| Selección y manipulación | **Telequinesis**: un rayo mágico desde la mano izquierda selecciona a distancia; el objeto se queda a la distancia donde se cogió, se aleja o acerca estirando el brazo (Go-Go), rota con la muñeca y se escala con A / B | Selección a distancia por rayo, natural para "mover cosas con la mente". El tamaño tiene efecto en el juego: más valor y más resistencia contra los duendes, pero más peso |
| Navegación propia | **Vuelo mágico**: mantener el grip derecho y volar hacia donde apunta la mano, con la velocidad según la presión | Es el desplazamiento propio de un mago. Se controla con una sola mano, así que se puede volar y lanzar hechizos a la vez |
| Navegación estándar | Caminar con el stick y teletransporte (plantilla XRI) | Opciones de confort conocidas, configurables en el grimorio |
| Acción | **Hechizos** con el gatillo derecho | La mano derecha "ataca" y la izquierda "manipula": cada mano tiene un papel claro |
| Control | **Grimorio**: libro de hechizos que flota sobre la mano izquierda | Un mago consulta su grimorio. Va en la muñeca y no pegado a la cara, que en VR es incómodo |

Otras decisiones:
- **Interfaces en el mundo.** El grimorio, el marcador y el tutorial están colocados en el mundo o en la mano, nunca fijos a la vista. El tutorial flota delante y se recoloca solo si giras la cabeza.
- **Feedback multisensorial.** Cada acción importante se nota con la vista, el oído y el tacto (sección 4.6).
- **Ambiente de Halloween.** Noche con niebla, tipografía de terror (Creepster) y viento de fondo.

## 3. Cómo abrir y ejecutar el proyecto

1. Abrir la carpeta del proyecto con **Unity 6000.5.0f1**. Los paquetes (XR Interaction Toolkit 3.5.1, OpenXR, Meta OpenXR, Input System, URP) se resuelven desde `Packages/manifest.json`.
2. La escena del juego es `Assets/Scenes/Colonial_Graveyard.unity`, que es la escena inicial de la build.
3. Para las gafas: plataforma **Android**, con las Meta Quest 3 conectadas, **Build And Run**.

## 4. Manual de uso

### 4.1 Controles

| Mando | Control | Acción |
|---|---|---|
| Izquierdo | **Gatillo** (mantener) | Telequinesis: levitar el objeto al que apuntas (hasta 8 m) |
| Izquierdo | **Estirar / recoger el brazo** (mientras levitas) | Alejar / acercar el objeto (técnica Go-Go: cuanto más lejos está, más rápido se mueve) |
| Izquierdo | **Y** (mantener, mientras levitas) | Rotar el objeto girando la muñeca, en cualquier eje. Al soltar Y se queda como está |
| Derecho | **A / B** (mientras levitas) | Agrandar / encoger el objeto (entre 0,5× y 2× de su tamaño original). Mientras levitas, A no salta |
| Derecho | **Gatillo** | Lanzar un hechizo (se recarga en 0,35 s) |
| Derecho | **Grip** (mantener) | Volar hacia donde apunta la mano; más presión, más velocidad |
| Izquierdo | **Stick** | Caminar (o teletransporte, según el grimorio) |
| Derecho | **Stick** adelante / lados | Teletransporte (plantilla XRI) / girar |
| Izquierdo | **X** | Abrir / cerrar el grimorio |

Las interfaces (tutorial y grimorio) se usan apuntando con el **rayo de la mano derecha + gatillo**, o tocándolas con el dedo.

### 4.2 Tutorial

Aparece al empezar y cada vez que se reinicia la partida. Mientras dura, el tiempo no corre y no salen duendes, pero hay calabazas para practicar (no puntúan). Los pasos son: objetivo, telequinesis, farol espectral, hechizos, vuelo y grimorio. Cada paso **se completa al hacer la acción** ("¡Bien hecho!"), y se puede saltar un paso o el tutorial entero. El último paso, **EMPEZAR RONDA**, lanza una cuenta atrás 3-2-1 y la ronda empieza.

### 4.3 Reglas y puntuación

| Evento | Efecto |
|---|---|
| Entregar una calabaza en la tumba iluminada | +10 puntos y +15% de energía a tamaño normal; se multiplica por su tamaño (0,5× → +5 puntos, 2× → +20 puntos y +30%) |
| Tamaño de la calabaza | Las grandes valen más pero pesan más (siguen la mano más despacio); las pequeñas, al revés |
| Calabaza levitada en alto | Fuera del alcance de los duendes: esperan debajo |
| Desterrar al fantasma (1,5 s de luz del farol) | +10 puntos; vuelve a aparecer a los 8 s en el lado opuesto del cementerio |
| El fantasma te toca | −1 vida (hay 10); reaparece en el lado opuesto |
| Eliminar un duende (6 de vida) | +5 puntos |
| Un duende destruye una calabaza | −10% de energía |
| Energía al empezar | 70% |
| Victoria | Acabar los 7 minutos con más del 10% de energía |
| Derrota | Energía al 0%, o perder las 10 vidas contra el fantasma |

El fantasma aparece a los 20 s de empezar la ronda, en la esquina del cementerio más lejana al jugador, flota a 0,8 m/s hacia la cabeza y, mientras le da la luz, se ralentiza, parpadea y encoge. El **farol espectral** está cerca del punto de inicio. Al cogerlo con telequinesis se queda delante de la mano y apunta hacia donde apunta la mano, como una linterna; **manteniendo Y** emite una luz verde con niebla, y se apunta girando la muñeca (si se coge y no se enciende, aparece el recordatorio "Mantén Y para encender la luz"). El haz mide siempre 6 m; su anchura depende del tamaño del farol: 15° a tamaño normal, 30° a 2× (más fácil acertar) y 7,5° a 0,5×. Como solo hay una mano de telequinesis, hay que elegir entre llevar una calabaza o protegerse con el farol.

Al principio hay un duende; a partir del primer minuto, dos a la vez. Cada duende eliminado reaparece a los 8 s. Siempre hay hasta 3 calabazas en el cementerio.

#### Hechizos desbloqueables

Cada ronda empieza solo con el Rayo. Entregar calabazas desbloquea hechizos más potentes. Al desbloquear uno **se equipa solo** y aparece un mensaje delante del jugador, con campanilla y vibración; en el grimorio (pestaña Hechizo) se puede volver a cualquier otro hechizo desbloqueado.

| Hechizo | Color | Efecto | Se desbloquea |
|---|---|---|---|
| Rayo | Cian | Daño 1 (6 impactos por duende) | Desde el principio |
| Rayo arcano | Morado | Daño 1, vuela 1,6× más rápido (más fácil acertar de lejos) | 1 calabaza entregada |
| Explosivo | Verde | Daño 1,5 a todos los duendes en 1,5 m | 3 calabazas entregadas |
| Bola de fuego | Naranja | Daño 2, bola más grande (3 impactos por duende) | 5 calabazas entregadas |

### 4.4 El grimorio (control)

Se abre con **X** y flota sobre la mano izquierda, mirando hacia ti. Mientras está abierto se pausan los hechizos, la telequinesis y el vuelo, para que los clics no los activen.

| Pestaña | Opciones |
|---|---|
| Hechizo | Lista de los 4 hechizos, una línea cada uno con su color: el elegido lleva borde blanco y [ELEGIDO]; los bloqueados aparecen en gris con las calabazas que faltan (el siguiente, en dorado) |
| Movimiento | Stick izquierdo: caminar o teletransporte · velocidad al caminar · giro suave o por pasos · velocidad o ángulo de giro · velocidad de vuelo |
| Partida | Puntos, energía, tiempo y resultado · **Pausar / Reanudar** · **Reiniciar** |
| Ayuda | Objetivo, puntuación y controles (los valores se leen del juego) |

### 4.5 Marcador

Encima de la mano izquierda se ven siempre los **puntos**, la **energía** y el **tiempo**. Se pone en rojo con poca energía, parpadea en rojo al perder energía y en verde al ganarla, y muestra el resultado al acabar la ronda.

### 4.6 Feedback

| Acción | Visual | Sonido | Vibración |
|---|---|---|---|
| Apuntar con telequinesis | Rayo cian y objeto teñido de cian | — | Toque en el mando izquierdo |
| Coger / sostener / soltar | Rayo y objeto dorados | Barrido ascendente / descendente | Pulso al coger, latido suave al sostener |
| Lanzar un hechizo | Bola con estela de cometa y luz de su color | Tono ascendente | Pulso en el mando derecho |
| Hechizo aún recargando | La bolita de la mano derecha está creciendo | — | Toque muy suave |
| Impacto | Destello del color del hechizo (el Explosivo muestra el tamaño de la explosión) | Golpe (explosión en el Explosivo) | — |
| Nuevo hechizo desbloqueado | Mensaje de su color delante del jugador | Campanilla | Pulso en el mando derecho |
| Aparece el fantasma | Fantasma translúcido con luz pálida | Silbido 3D | Toque en ambos mandos |
| El farol ilumina al fantasma | Cono de luz visible; el fantasma parpadea, brilla y encoge | — | — |
| Fantasma desterrado | Destello y texto "+10 pts" | Campanilla | — |
| El fantasma te toca | Mensaje "¡El fantasma te atrapó! Vidas: N" | Zumbido | Fuerte en ambos mandos |
| Golpear a un duende | Parpadeo rojo y barra de vida (verde → rojo) | Golpe seco | — |
| Eliminar a un duende | Explosión: destello, luz naranja y trozos | Estallido | — |
| Aparece un duende | — | Gruñido 3D: se oye de dónde viene | — |
| Un duende destruye una calabaza | Destello rojo; marcador en rojo | Zumbido 3D | Ambos mandos |
| Entregar una calabaza | Destello dorado; marcador en verde | Campanilla | Ambos mandos |
| Volar | — | — | Vibración que crece con la velocidad |
| Grimorio y tutorial | Botones que se iluminan; opción activa resaltada | Campanilla y cuenta atrás en el tutorial | Toque al apuntar y al pulsar |
| Ambiente | Niebla nocturna | Viento de fondo | — |

Los sonidos se generan por código (`MagicFeedback`), porque el proyecto no tenía efectos de sonido adecuados.

## 5. Estructura

Scripts en `Assets/Scripts/`:

| Script | Responsabilidad |
|---|---|
| `GraveyardGameManager` | Reglas de la ronda: energía, puntos, tiempo, inicio, victoria y derrota |
| `GraveyardTutorial` | Tutorial interactivo antes de cada ronda y cuenta atrás |
| `GoblinEnemy`, `GoblinSpawner` | Duendes: buscan la calabaza más cercana, reciben daño y reaparecen |
| `GhostSystem`, `GhostEnemy`, `GhostLantern` | Fantasma (aparición, persecución, vidas) y farol espectral (haz según tamaño y rotación) |
| `GoblinHealthBar`, `GoblinExplosion`, `FadingLight` | Barra de vida, parpadeo al recibir daño y explosión al morir |
| `PumpkinItem`, `PumpkinSpawner`, `PumpkinDeliveryZone` | Calabazas, su aparición y la tumba de entrega |
| `GraveyardLightingController` | Luces protectoras que se apagan con la energía |
| `WizardLevitation`, `WizardLevitationFeedback` | Telequinesis (selección, traslación, rotación y escalado) y su feedback |
| `WizardSpellCaster`, `MagicProjectile` | Hechizos, estela, indicador de recarga e impactos |
| `WizardFlight` | Vuelo mágico (proveedor de locomoción de XRI) |
| `WizardLocomotionSettings`, `WizardTurnSettings` | Opciones de movimiento y giro del rig XRI |
| `WizardControlMenu`, `MenuButtonHover` | Grimorio de muñeca con pestañas |
| `GraveyardWristHud` | Marcador de la muñeca |
| `GraveyardFeedback`, `MagicFeedback` | Feedback de eventos de la partida, sonidos sintetizados y vibración |
| `HalloweenFontTheme` | Aplica la tipografía Creepster a los textos |
| `WizardHandIK`, `WizardBodyFollow` | IK de las manos del mago y orientación del cuerpo con la cabeza |

## 6. Disclosure notice (uso de herramientas y recursos externos)

**Recursos de terceros:**

- **Unity VR Template** (`Assets/VRTemplateAssets`) y samples del **XR Interaction Toolkit 3.5.1** (Starter Assets, Hands Interaction Demo): rig XR, locomoción base (caminar, teletransporte, giro) e interactores.
- **Colonial City LittlePack – Church Graveyard Environment**, de *3DShapes* (Unity Asset Store): el cementerio.
- **WizardPBR** (Unity Asset Store) [COMPLETAR: autor]: modelo del mago.
- **LOWPO: Low-Poly Goblin Pack**, de *Standout 7* (Unity Asset Store): los duendes.
- **Spooky pumpkins** [COMPLETAR: autor y origen]: las calabazas.
- **Creepster**, de *Font Diner* (Google Fonts), con licencia SIL Open Font License 1.1, incluida en `Assets/Fonts/Creepster/OFL.txt`.
- [COMPLETAR: tutoriales, vídeos o documentación seguidos]

**Uso de IA generativa:**

- Se ha usado **Claude (Anthropic)**, a través de Claude Code, como asistente de programación. En esta rama ha implementado:
  - el vuelo mágico (`WizardFlight`);
  - el arreglo del cambio de giro (`WizardTurnSettings`) y la orientación del cuerpo con la cabeza (`WizardBodyFollow`);
  - la estela, la luz y el indicador de recarga de los hechizos;
  - el grimorio de muñeca (`WizardControlMenu`, `WizardLocomotionSettings`, `MenuButtonHover`) y el marcador de muñeca (`GraveyardWristHud`), que sustituyen al panel anterior fijado a la cámara;
  - la explosión de los duendes (`GoblinExplosion`, `FadingLight`) y su barra de vida (`GoblinHealthBar`);
  - la niebla y el ajuste de la iluminación, y la tipografía de Halloween (`HalloweenFontTheme`);
  - el tutorial y el inicio de ronda (`GraveyardTutorial`; estado de espera en `GraveyardGameManager`, `GoblinEnemy` y `GoblinSpawner`);
  - el feedback sonoro y háptico (`MagicFeedback`, `GraveyardFeedback` y los cambios en `WizardLevitationFeedback`, `WizardSpellCaster`, `MagicProjectile`, `GoblinEnemy` y `PumpkinDeliveryZone`);
  - la rotación de la telequinesis con la muñeca, el empuje / atracción Go-Go, su alcance de 8 m, el bloqueo del salto mientras se levita y el arreglo de los límites de escalado (`WizardLevitation`);
  - el efecto del tamaño de las calabazas (puntos, energía y peso), las calabazas fuera del alcance de los duendes y los textos flotantes (`PumpkinItem`, `PumpkinDeliveryZone`, `GoblinEnemy`, `FloatingText`);
  - los hechizos desbloqueables y su pestaña del grimorio (`WizardSpellCaster`, `MagicProjectile`, `WizardControlMenu`);
  - el fantasma, el farol espectral y las vidas (`GhostSystem`, `GhostEnemy`, `GhostLantern`, cambios en `GraveyardGameManager`, `GraveyardFeedback`, `GraveyardWristHud`, `GraveyardEndScreen` y el tutorial);
  - los hechizos brillantes, la pantalla de fin de partida y los límites del cementerio (`GlowSphere`, `GraveyardEndScreen`, `PlayAreaBounds`);
  - la escena de inicio de la build y este documento.

  El detalle está en el historial de commits de la rama. [COMPLETAR: confirmar que el grupo ha revisado y probado estos cambios.]
- [COMPLETAR: uso de IA, si lo hubo, en el resto del código (mago en primera persona, telequinesis, hechizos, calabazas, duendes, reglas de la partida, iluminación)]

## 7. Limitaciones conocidas

- **El APK se abre como una ventana plana en las Quest**: la app no está arrancando en modo VR. Está pendiente de revisar la configuración de OpenXR para Android; la primera prueba es desactivar "Android XR Support" y dejar solo "Meta Quest Support".
- **Panel antiguo:** el panel de control anterior y su conmutador siguen en la escena, desactivados.
