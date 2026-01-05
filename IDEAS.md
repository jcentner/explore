# Future Feature Ideas

> **Note:** This is a parking lot for speculative ideas beyond the current roadmap.  
> These are NOT commitments—just captured thoughts for potential future exploration.  
> See `design_doc.md` for the active roadmap and `CHANGELOG.md` for current progress.

---

## Exploration & Discovery

<!-- Ideas related to finding things, uncovering secrets, environmental storytelling -->

- **Stargate-style dialing system** — Symbol-based address system corresponding to actual star coordinates within the galaxy. Players learn/discover symbols to dial new destinations.

---

## Movement & Traversal

<!-- Ideas for new ways to move around, vehicles, gadgets -->

- 

---

## Environmental Systems

<!-- Weather, hazards, day/night, dynamic world elements -->

- 

---

## Crafting & Construction

<!-- Building, crafting, modular design systems -->

- **Freeform component construction** — Ship parts designed by connecting sub-components (e.g., laser weapon = emitter + capacitor + power conduit). Capacitors built from raw materials gathered in-world. Deep crafting tree from resources → parts → systems.
- **Modular ship design** — Design hull panels, assemble into structures, add doors/hatches, mount engines, place generators inside. Full creative freedom for ship layout.
- **Crafting shortcuts** — Buy complete components from NPCs, purchase sub-parts (hinges, seals), or acquire blueprints. Balances depth vs. accessibility.
- *Implementation TBD — needs research into voxel/modular building systems, physics constraints, UI for assembly.*

---

## Lore & Narrative

<!-- Story hooks, mysteries, world-building concepts -->

- **Big LLM story generation (API)** — Use cloud LLM calls for sophisticated story arcs, alien race creation. Pre-built "code prototypes" as a toolbox (stealth tech templates, language rule sets, symbol/graphic libraries) that the LLM can select and combine.

---

## Visual & Audio Polish

<!-- Art direction ideas, shader effects, soundscapes -->

- 

---

## Quality of Life

<!-- UI improvements, accessibility, player convenience features -->

- 

---

## Procedural Generation

<!-- Galaxy, planets, content generation systems -->

- **Procedural galaxy map** — Randomly generated star positions, each star has procedurally generated solar system (planets, moons, asteroids, comets) based on wide parameter set. Galaxy-scale exploration.
- **Stargate network overlay** — Many (not all) habitable planets connected by portal network. Map displays discovered connections, encouraging exploration to find new routes.
- **Small local LLM for flavor content** — ~5GB quantized model (e.g., Qwen 3, story-tuned variant) generates names, titles, flavor text, lore snippets for the procedural universe. 
- **Background content streaming** — Story generation runs async: first populates connected planets reachable from player's starting location, then expands outward. Prevents long initial load times.
- **Small LLM for NPC dialogue** — Separate model fine-tuned for roleplay/dialogue handles character interactions. Allows dynamic conversations without scripted trees.

---

## Random Sparks

<!-- Unfiltered ideas that don't fit elsewhere yet -->

- 

---

*Last reviewed: 2026-01-05*
