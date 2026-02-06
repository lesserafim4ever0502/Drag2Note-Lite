# Drag2Note-Lite Code Map

This document provides a high-level overview of the source code structure to help developers understand the architecture and locate key components.

## 📂 Project Structure

### Root
- **`App.xaml` & `App.xaml.cs`**: Entry point. Handles startup logic, `Mutex` for single instance check, and global exception handling.
- **`Drag2Note.csproj`**: Project file. Defines dependencies (e.g., `GongSolutions.WPF.DragDrop`, `CommunityToolkit.Mvvm`).

### 📂 Views
Contains the UI definitions (XAML) and their code-behind files.
- **`MainWindow.xaml`**: The primary dashboard.
  - **Features**: Search bar, Tab switching (Notes/Todo), Card List, Editor/Preview capsule.
  - **Drag & Drop**: Configured with `dd:DragDrop.DropHandler` and custom `DragHandler` to support hybrid dragging (cards + tags).
- **`SettingsWindow.xaml`**: Configuration dialog for appearance and behavior settings.
- **`Components/NoteCard.xaml`**: The core UI component for displaying a single note/todo item.
  - **Features**: Interactive Tag bar (with its own drag logic), Title editing, Markdown preview snippet.
  - **Conflict Resolution**: Explicitly disables Gong drag on Tags to allow native `PreviewMouseLeftButtonDown` dragging for tags.
- **`Components/DragAdorner.cs`** *(Deprecated/Removed)*: Previous usage for native drag visuals, replaced by GongSolutions.

### 📂 ViewModels
Implements the MVVM pattern using `CommunityToolkit.Mvvm`.
- **`MainViewModel.cs`**: The brain of the application.
  - **State**: Manages `FilteredItems`, `CurrentTab`, `SearchText`, and `IsEditorActive`.
  - **Commands**: `MoveItem`, `ReorderItem`, `SaveRenameCommand`, etc.
  - **Interfaces**: Implements `IDropTarget` for GongSolutions to handle card reordering logic.
  - **Custom logic**: Creates `CardListDragSource` instance.
- **`CardListDragSource.cs`**: **Configured Custom Drag Handler**.
  - **Purpose**: Intercepts drag initiation.
  - **Logic**: Checks visual tree. If the user drags a **Tag**, it blocks Gong from starting, allowing the Tag's internal drag logic to take over. This enables nested drag-and-drop.
- **`SettingsViewModel.cs`**: Manages application settings state.

### 📂 Models
Data structures mirroring the JSON storage.
- **`MetadataItem.cs`**: Represents a single Note/Todo.
  - **Properties**: `Id`, `FilePath`, `Tags`, `OrderIndex`, `Status`.
- **`MarkdownBlock.cs`**: Used for the preview renderer logic.

### 📂 Services
Business logic and data access layer.
- **`Data/MetadataService.cs`**: Handles loading/saving `metadata.json`.
- **`Data/StorageService.cs`**: Manages file system operations for the `UserAssets` and `UserData` folders.
- **`Logic/MarkdownService.cs`**: Simple Markdown parsing for the preview mode.
- **`SettingsService.cs`**: Persists user preferences.

## 🔑 Key Mechanisms

### Hybrid Drag & Drop System
1. **Card Reordering**: Handled by **GongSolutions.WPF.DragDrop**.
   - **Trigger**: Dragging any part of the `NoteCard` *except* the tags.
   - **Implementation**: `MainViewModel` implements `DragOver` and `Drop` to update `OrderIndex`.
2. **Tag Reordering**: Handled by **Native WPF DragDrop**.
   - **Trigger**: `PreviewMouseLeftButtonDown` on Tag elements.
   - **Conflict Fix**: `CardListDragSource` detects clicks on "TagItem" or "TagsItemsControl" and cancels the Gong drag, yielding control to the native handler.

### Data Persistence
- **Metadata**: Stored in `UserData/metadata.json`. Code ensures atomic saves where possible.
- **Content**: Each note is a `.md` file in `UserData`.

### UI Theming
- Uses `DynamicResource` extensively for colors (Text, Backgrounds) to support automatic Dark/Light mode switching based on system settings.
- **Custom Templates**: Buttons (like the Back button) use `ControlTemplate` with Triggers for refined hover/press states.
