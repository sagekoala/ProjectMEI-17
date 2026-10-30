# Guardián del Cementerio

Juego de Realidad Virtual para **Meta Quest 3**, hecho con **Unity + OpenXR**. Práctica de SGI (MIRI, Q1).

- **Autores:** Brandon Dukes y Adrià Galdón
- **Vídeo:** [COMPLETAR: enlace al vídeo]

## 1. Objetivo

Eres un mago que protege un cementerio en la noche de Halloween. Las calabazas mágicas mantienen encendidas las luces del cementerio, y unos duendes quieren destruirlas.

Para ganar tienes que:

- Llevar las calabazas con telequinesis hasta la tumba iluminada. Cada calabaza da puntos y energía.
- Matar a los duendes con hechizos antes de que lleguen a las calabazas.
- Protegerte del fantasma con la linterna anti-fantasmas.
- Tener más del 10% de energía cuando acaba la ronda (5 minutos: 4 de juego normal y 1 de oleada final).

Pierdes si la energía llega a 0% o si el fantasma te toca 10 veces.

Antes de la ronda hay un tutorial. Al final sale una pantalla con el resultado.

## 2. Cómo ejecutar el proyecto

1. Abrir el proyecto con **Unity 6000.5.0f1** (no actualizar la versión).
2. La escena del juego es `Assets/Scenes/Colonial_Graveyard.unity`.
3. Para las gafas: en *Build Profiles* elegir **Meta Quest**, conectar las Quest 3 por cable y pulsar **Build And Run**. La app se llama "Guardian del Cementerio".

Importante: en *Project Settings → XR Plug-in Management → OpenXR*, en la pestaña de Android solo tiene que estar activado **Meta Quest Support**. Si se activa "Android XR Support", el juego se abre como una ventana plana y no en VR.

## 3. Controles

**Mano izquierda**
- **Gatillo (mantener):** telequinesis. Coge el objeto al que apuntas, hasta 8 m.
- **Mover el brazo (con un objeto cogido):** estirar el brazo aleja el objeto, recoger el brazo lo acerca (técnica Go-Go).
- **Y (mantener, con un objeto cogido):** el objeto gira igual que tu muñeca.
- **Y (con la linterna):** encender la luz.
- **Stick:** caminar o teletransporte (se elige en el grimorio).
- **X:** abrir o cerrar el grimorio.

**Mano derecha**
- **Gatillo:** lanzar un hechizo.
- **Grip (mantener):** volar hacia donde apunta la mano. Más presión, más velocidad.
- **A / B (con un objeto cogido):** hacer el objeto más grande o más pequeño (de 0,5× a 2×).
- **Stick:** girar y teletransporte.

Los menús se usan con el rayo de la mano derecha y el gatillo.

## 4. Cómo se juega

### Calabazas
- Hay hasta 3 calabazas en el cementerio.
- Entregar una calabaza normal da +10 puntos y +15% de energía.
- El tamaño importa: una calabaza grande vale más (2× da +20 puntos y +30%), pero pesa más y sigue la mano más despacio. Una pequeña vale menos.
- Si levantas una calabaza muy alto, los duendes no llegan y esperan debajo.

### Duendes
- Buscan la calabaza más cercana. Si se la comen, pierdes 10% de energía.
- Tienen 6 de vida. Matar a uno da +5 puntos.
- En el último minuto (a partir del minuto 4) llega la **oleada final**: suena un cuerno y llegan duendes arqueros, más rápidos pero con solo 2 de vida.

### Hechizos
Al principio solo tienes el Rayo. Al entregar calabazas se desbloquean hechizos nuevos, y el nuevo se pone solo. En el grimorio puedes volver a cualquier hechizo desbloqueado.

- **Rayo** (cian): daño 1. Desde el principio.
- **Rayo arcano** (morado): daño 1, más rápido. Con 1 calabaza.
- **Explosivo** (verde): daño 1,5 a todos los duendes cerca del impacto. Con 3 calabazas.
- **Bola de fuego** (naranja): daño 2. Con 5 calabazas.

### Fantasma y linterna
- A los 20 s de empezar sale un fantasma por el lado más lejano del cementerio. Vuela hacia ti. Mientras está, se oye un **lamento** que avisa de que viene: suena desde donde está el fantasma y se vuelve más agudo cuanto más cerca está.
- Si te toca, pierdes una vida (tienes 10) y vuelve a salir por el otro lado.
- Cerca del inicio está la **linterna anti-fantasmas**, con un cartel encima.
- Cógela con telequinesis: se queda en tu mano y apunta hacia delante.
- Mantén **Y** para encender la luz verde y apunta al fantasma girando la muñeca.
- Con 2 s de luz el fantasma muere (+10 puntos). Su barra de vida baja mientras le da la luz.
- El tamaño de la linterna cambia el ancho de la luz: más grande, luz más ancha y más fácil de acertar.
- Solo tienes una mano para la telequinesis, así que tienes que elegir: llevar una calabaza o tener la linterna.

### Límites
No se puede salir del cementerio. Hay paredes invisibles y un techo, también volando.

## 5. Grimorio (menú)

Se abre con **X** y sale encima de la mano izquierda. Mientras está abierto, los hechizos, la telequinesis y el vuelo se pausan.

- **Hechizo:** lista de hechizos con su color. El elegido pone [ELEGIDO]. Los bloqueados dicen cuántas calabazas faltan.
- **Movimiento:** caminar o teletransporte, velocidad al caminar, giro suave o por pasos, velocidad de giro y velocidad de vuelo.
- **Partida:** puntos, energía, vidas y tiempo. Pausar, reanudar y reiniciar.
- **Ayuda:** resumen de las reglas y los controles.

En la muñeca izquierda también hay un marcador con puntos, energía, vidas y tiempo.

## 6. Feedback

Casi todas las acciones tienen respuesta visual, sonido y vibración:

- **Telequinesis:** rayo cian al apuntar y dorado al coger; vibración en la mano izquierda.
- **Hechizos:** bola brillante con estela; sonido al lanzar; vibración en la mano derecha. La bolita de la mano derecha muestra si el hechizo está listo.
- **Duendes:** parpadean en rojo al recibir daño, tienen barra de vida y explotan al morir. Se oye un gruñido cuando salen.
- **Calabazas:** destello dorado y texto "+puntos" al entregar; destello rojo si un duende se la come.
- **Fantasma:** silbido al salir y un lamento continuo mientras está (más agudo cuanto más cerca, más grave cuando le da la luz). Con la luz parpadea, encoge y la mano vibra. Mensaje y vibración fuerte si te toca.
- **Linterna:** cartel en el suelo, aviso "Mantén Y para encender la luz" en la mano, y luz verde que ilumina la escena.
- **Hechizo nuevo y oleada final:** mensaje delante del jugador, sonido y vibración.
- **Ambiente:** niebla, viento y letra de Halloween.

Los sonidos se generan por código (`MagicFeedback`).

## 7. Scripts principales

Todos están en `Assets/Scripts/`:

- `GraveyardGameManager`: reglas de la ronda (energía, puntos, tiempo, vidas, victoria y derrota).
- `GraveyardTutorial`, `GraveyardEndScreen`: tutorial y pantalla final.
- `WizardLevitation`, `WizardLevitationFeedback`: telequinesis (coger, mover, girar, escalar).
- `WizardSpellCaster`, `MagicProjectile`: hechizos y su daño.
- `WizardFlight`: vuelo.
- `WizardControlMenu`, `WizardLocomotionSettings`, `GraveyardWristHud`: grimorio, opciones de movimiento y marcador.
- `GoblinEnemy`, `GoblinSpawner`, `GoblinHealthBar`, `GoblinExplosion`: duendes y oleada final.
- `GhostSystem`, `GhostEnemy`, `GhostLantern`: fantasma y linterna.
- `PumpkinItem`, `PumpkinSpawner`, `PumpkinDeliveryZone`: calabazas y tumba de entrega.
- `PlayAreaBounds`: límites del cementerio.
- `GraveyardFeedback`, `MagicFeedback`, `FloatingText`, `GlowSphere`: feedback, sonidos, textos flotantes y efectos de luz.
- `HalloweenFontTheme`: letra de Halloween.

## 8. Recursos externos

### Recursos de terceros
- **Unity VR Template** y samples de **XR Interaction Toolkit 3.5.1**: rig XR, caminar, teletransporte y giro.
- **Colonial City LittlePack – Church Graveyard Environment**, de 3DShapes (Unity Asset Store, gratis): el cementerio. https://assetstore.unity.com/packages/3d/environments/urban/colonial-city-littlepack-church-graveyard-environment-163089
- **Breakable Spooky Pumpkins**, de 3d.rina (Unity Asset Store, gratis): las calabazas. https://assetstore.unity.com/packages/3d/props/food/breakable-spooky-pumpkins-12060
- **LOWPO: Low-Poly Goblin Pack Free Sample**, de Standout 7 (Unity Asset Store, gratis): los duendes. https://assetstore.unity.com/packages/3d/characters/creatures/lowpo-low-poly-goblin-pack-free-sample-332200
- **WizardPBR** (Unity Asset Store), de [COMPLETAR: autor y enlace]: el mago.
- **Creepster**, de Font Diner (Google Fonts), licencia SIL Open Font License 1.1 (`Assets/Fonts/Creepster/OFL.txt`).
- Apuntes de la asignatura y documentación de Unity y Meta.

## 9. Limitaciones

- La altura de los ojos es fija (1,36 m), no es la altura real del jugador. Se ajusta al empezar y al recentrar las gafas (mantener el botón Meta).
- El panel de control antiguo sigue en la escena, pero está desactivado.

## 10. Uso de IA (disclosure)

[COMPLETAR]

<!--
Borrador (borrar este comentario al completar):
- ChatGPT (OpenAI): como guía para configurar el proyecto de Unity y los assets (importar los paquetes del Asset Store, materiales, iluminación y ajustes de XR).
- Claude Code (Anthropic): para escribir y cambiar código durante el desarrollo: vuelo, grimorio de muñeca y marcador, tutorial, explosión y barra de vida de los duendes, feedback de sonido y vibración, letra de Halloween, iluminación, la oleada final de arqueros y el sonido de aviso del fantasma.
- Claude (Anthropic, chat): para revisar el proyecto con los requisitos de la práctica, resolver problemas de la build para Meta Quest (ajustes de OpenXR, nombre del paquete, espacio en disco del PC) y escribir código de estas partes: hechizos que brillan, pantalla final, límites del cementerio, telequinesis con Go-Go y giro con la muñeca, efecto del tamaño de las calabazas, hechizos desbloqueables, fantasma y linterna, y este documento.
- Las ideas del juego y las decisiones son nuestras. Todo el código hecho con IA lo hemos revisado y probado en el editor y en las Meta Quest 3.
-->
