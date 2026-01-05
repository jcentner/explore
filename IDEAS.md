# Future Feature Ideas

> **Note:** This is a parking lot for speculative ideas beyond the current roadmap.  
> These are NOT commitments—just captured thoughts for potential future exploration.  
> See `design_doc.md` for the active roadmap and `CHANGELOG.md` for current progress.

---

## Exploration & Discovery

<!-- Ideas related to finding things, uncovering secrets, environmental storytelling -->

- **Stargate-style dialing system** — Symbol-based address system corresponding to actual star coordinates within the galaxy. Players learn/discover symbols to dial new destinations.
  - Partial addresses narrow to galactic regions (first symbols = quadrant, next = cluster, etc.)
  - **Wrong/incomplete dials** have consequences: unstable wormholes, one-way trips, unexpected destinations
  - Chevron-style feedback with escalating tension (will it connect?)
  - *Note: Avoid being an exact Stargate copy — find unique twist on the mechanic*

---

## Movement & Traversal

<!-- Ideas for new ways to move around, vehicles, gadgets -->

- 

---

## Environmental Systems

<!-- Weather, hazards, day/night, dynamic world elements -->

- **Hazardous atmospheres** — Require suit upgrades or ship modifications to survive
- **Solar flares** — Disable electronics, require shelter or shielding
- **Tidal heating** — Moons near gas giants have volcanic activity
- **Asteroid fields** — Navigation puzzles and/or mining opportunities
- **Binary/complex systems** — Figure-8 orbits, complex gravity interactions (builds on spherical gravity work)

---

## Crafting & Construction

<!-- Building, crafting, modular design systems -->

- **Freeform component construction** — Ship parts designed by connecting sub-components (e.g., laser weapon = emitter + capacitor + power conduit). Capacitors built from raw materials gathered in-world. Deep crafting tree from resources → parts → systems.
  - **Quality tiers** based on materials AND assembly skill — same blueprint, different results
  - **Reverse-engineering** salvaged alien tech (partial success = partial understanding)
  - **Heat management** as universal constraint — powerful systems generate heat, need dissipation
  - **Experimental combinations** with emergent effects (some good, some explosive)
  - **Component degradation** — Parts wear, can be repaired, eventually need replacement
- **Modular ship design** — Design hull panels, assemble into structures, add doors/hatches, mount engines, place generators inside. Full creative freedom for ship layout.
  - **Center of mass** affects handling
  - **Discrete hull integrity** — Damage is localized to panels, repairs are targeted
  - **Pressurization zones** — Breach one section, seal the bulkhead, survive
  - **Energy production/consumption** as major constraint — budget power across systems
  - **Heat dissipation** as major constraint — high-powered builds need cooling solutions
- **Crafting shortcuts** — Buy complete components from NPCs, purchase sub-parts (hinges, seals), or acquire blueprints. Balances depth vs. accessibility.
- *Implementation TBD — needs research into voxel/modular building systems, physics constraints, UI for assembly.*

---

## Lore & Narrative

<!-- Story hooks, mysteries, world-building concepts -->

- **Big LLM story generation (API)** — Use cloud LLM calls for sophisticated story arcs, alien race creation. Pre-built "code prototypes" as a toolbox (stealth tech templates, language rule sets, symbol/graphic libraries) that the LLM can select and combine.
  - **Precursor mystery** — The ancient civilization that built the gates. Why are they gone? What did they know?
  - **Alien civilizations** with internally consistent logic: biology → culture → technology → politics
  - **Living factions** whose conflicts evolve based on player actions and time
  - Toolbox includes: government templates, tech paradigms, aesthetic motifs, conflict archetypes, forbidden knowledge types

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
  - **Star classification** affects what you find (red dwarfs = resource-rich but dim, blue giants = short-lived systems with ancient ruins)
  - **Galactic geography** matters: core (dense, dangerous), rim (sparse, unexplored), nebulae (sensor interference, hidden systems)
  - Rare **unconnected systems** reachable only by ship — long journeys with potential high reward, but may have been ignored for a reason (high risk, or just nothing of value)
- **Stargate network overlay** — Many (not all) habitable planets connected by portal network. Map displays discovered connections, encouraging exploration to find new routes.
  - **Gate network topology** creates natural hubs and backwaters — politics emerge from geography
  - **Dead gates** that need repair/power source before activation
- **Small local LLM for flavor content** — ~5GB quantized model (e.g., Qwen 3, story-tuned variant) generates names, titles, flavor text, lore snippets for the procedural universe.
  - Feed the model **cultural rule sets per civilization** — naming conventions internally consistent (harsh consonants for warriors, flowing syllables for aquatic species)
  - Generated history that **references other generated content** — "The Siege of Verath" mentioned in multiple places
  - Procedural myths/legends that **hint at real game mechanics** or secrets
  - Consistent **language fragments** — players recognize patterns, decode alien script over time
- **Background content streaming** — Story generation runs async: first populates connected planets reachable from player's starting location, then expands outward. Prevents long initial load times.
- **Small LLM for NPC dialogue** — Separate model fine-tuned for roleplay/dialogue handles character interactions. Allows dynamic conversations without scripted trees.
  - **Persistent memory** — NPCs remember you, your reputation, what you've told them
  - **Information asymmetry** as gameplay — NPCs know things about other systems, will trade knowledge
  - **Procedural quests** generated from world state ("I heard there's activity at the old gate on Keth-4...")
  - **Organic negotiation** — Haggling/trading feels natural, not menu-driven
  - NPCs with **secrets** revealed only under certain conditions

---

## Random Sparks

<!-- Unfiltered ideas that don't fit elsewhere yet -->

- 

---

*Last reviewed: 2026-01-05*
