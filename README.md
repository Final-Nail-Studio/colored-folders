# Colored Folders

Colors folder backgrounds in the Unity Project window based on folder name. 

## Installation

1. Copy or clone this package into your project's `Packages/` directory so the path is `Packages/com.finalnailstudio.coloredfolders/`.
2. Alternatively, open **Window > Package Manager**, click **+**, choose **Add package from disk**, and select the `package.json` file.

Unity will detect the package automatically on the next domain reload.

## Usage

### Quick Start

**Alt + Right Click** any folder in the Project window to open the Colored Folders menu. Pick a color and the rule applies immediately to every folder with that name in your project.

### Context Menu (Alt + Right Click on a folder)

| Action | Description |
|--------|-------------|
| **Set Color** | Choose from 16 preset colors |
| **Recursive** | Toggle whether children inherit this color |
| **Remove Rule** | Delete the rule for this folder name |
| **Settings...** | Open the settings window |

### Settings Window

Open via **Tools > Colored Folders > Settings**.

- View all active rules with color swatches
- Adjust colors using Unity's color picker
- Toggle recursive inheritance per rule
- Add rules manually by typing a folder name
- Import/export presets as JSON
- Clear all rules

## Rule Behavior

### Name Matching
- Rules match by **folder name only**, not full path.
- Matching is **case-insensitive** (`scripts`, `Scripts`, and `SCRIPTS` all match the same rule).
- Every folder in the project sharing that name gets the same color.

### Recursive Inheritance
- **Recursive** is enabled by default on new rules.
- A recursive rule propagates its color to all subfolders that don't have their own direct rule.
- A **direct name match** always overrides an inherited recursive color.
- When inheriting, the **nearest recursive ancestor** wins over more distant ones.

#### Example

Given these rules:
- `Scripts` = Blue, recursive = true
- `Player` = Red, recursive = false

| Folder | Color | Reason |
|--------|-------|--------|
| `Assets/Scripts` | Blue | Direct match |
| `Assets/Scripts/Player` | Red | Direct match overrides inherited |
| `Assets/Scripts/Player/Components` | Blue | Player is not recursive, inherits from Scripts |
| `Assets/Scripts/Enemy` | Blue | Inherits from Scripts |
| `Assets/Art/Scripts` | Blue | Direct match (same name) |

### Renaming

Renaming a folder automatically updates its color based on its new name. No manual intervention needed.

## Presets

### Export
1. Open **Tools > Colored Folders > Settings**
2. Click **Export JSON**
3. Choose a save location

### Import
1. Open **Tools > Colored Folders > Settings**
2. Click **Import JSON**
3. Select a `.json` preset file (replaces all current rules)

Sample presets are included in `Samples~/Example Presets/`.

## Settings Storage

Rules are stored in `ProjectSettings/ColoredFoldersSettings.json`. This file should be committed to version control so the team shares the same folder colors.

## Limitations

- **Assets folder only** — Packages and other root folders are not tinted.
- **Color tinting only** — No custom icons.
- **Editor only** — No runtime code or overhead. Not really a limitation :)
- **Folder name rules only** — Does not target a specific path, only a name shared by all matching folders.

## Compatibility

- Unity 2021.3 and later
- Dark and light editor themes
- Windows, macOS, Linux

## License

MIT — see [LICENSE.md](LICENSE.md)
