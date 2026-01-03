# Explore

A 3D exploration game set in a compact, handcrafted star system. Players walk on round planets with dynamic gravity, fly ships between celestial bodies, and use stargates to travel between locations.

**Inspiration:** *Outer Wilds* (systemic physics, exploration) + *Stargate* (gate travel, mystery)

## Quick Links

| Document | Purpose |
|----------|---------|
| [design_doc.md](design_doc.md) | Vision, architecture, milestones |
| [CHANGELOG.md](CHANGELOG.md) | Current progress, session log |
| [specs/](specs/) | Per-system specifications |

## Tech Stack

- **Engine:** Unity 6000.3.2f1
- **Render Pipeline:** URP (Universal Render Pipeline)
- **Input:** Unity Input System (new)
- **IDE:** VS Code + GitHub Copilot

## Project Structure

```
explore/
├── design_doc.md          # Main design document
├── CHANGELOG.md           # Progress tracking
├── specs/                 # System specifications
│   ├── gravity-system.spec.md
│   ├── player-system.spec.md
│   └── ship-system.spec.md
└── explorer-game/         # Unity project
    └── Assets/_Project/   # Game-specific assets
```

## Current Milestone

**Milestone 0: Tooling + URP Baseline**

See [CHANGELOG.md](CHANGELOG.md) for detailed progress.

## Getting Started

1. Open `explorer-game/` folder in Unity 6000.3.2f1
2. Wait for package import to complete
3. Open scene in `Assets/_Project/Scenes/`

## For AI Assistance

When starting a Copilot session:
1. Share `design_doc.md` for architecture context
2. Share relevant `specs/*.spec.md` for implementation details
3. State current milestone and specific goal
4. Reference [CHANGELOG.md](CHANGELOG.md) for current state