# Drag2Note-Lite Code Map
> 自动生成的架构蓝图 | Auto-generated Architecture Blueprint

本文档提供源码结构的宏观概览，帮助开发者理解架构并定位核心组件。
This document provides a high-level overview of the source code structure to help developers understand the architecture and locate key components.

## 🌳 Project Tree / 项目树状图

```ascii
Drag2Note/
├── App.xaml                    # 应用程序入口与资源定义 (Application Entry & Resources)
├── Drag2Note.csproj            # 项目配置文件 (Project Configuration)
├── Models/                     # 数据模型 (Data Models)
│   ├── MetadataItem.cs         # 笔记/待办元数据定义 (Note/Todo Metadata Definition)
│   ├── AppSettings.cs          # 应用设置结构 (Application Settings Structure)
│   └── MarkdownBlock.cs        # Markdown 解析块 (Markdown Parsing Blocks)
├── ViewModels/                 # MVVM 视图模型 (View Models)
│   ├── MainViewModel.cs        # 主界面核心逻辑 (Core Logic for Dashboard)
│   ├── FloatingViewModel.cs    # 悬浮窗逻辑 (Floating Window Logic)
│   └── SettingsViewModel.cs    # 设置逻辑 (Settings Logic)
├── Views/                      # UI 视图 (UI Views)
│   ├── MainWindow.xaml         # 主仪表盘窗口 (Main Dashboard Window)
│   ├── FloatingWindow.xaml     # 桌面悬浮球 (Desktop Floating Ball)
│   ├── SettingsWindow.xaml     # 设置对话框 (Settings Dialog)
│   └── Components/             # 可复用组件 (Reusable Components)
│       └── NoteCard.xaml       # 笔记卡片组件 (Note Card Component)
└── Services/                   # 业务服务层 (Business Services)
    ├── Data/                   # 数据持久化 (Data Persistence)
    │   ├── MetadataService.cs  # JSON 元数据读写 (JSON Metadata I/O)
    │   └── StorageService.cs   # 文件系统操作 (File System Operations)
    ├── Logic/                  # 业务逻辑 (Business Logic)
    │   └── DragDropService.cs  # 拖拽处理逻辑 (Drag & Drop Logic)
    ├── HotkeyService.cs        # 全局热键服务 (Global Hotkey Service)
    ├── TrayService.cs          # 系统托盘管理 (System Tray Management)
    └── WindowManager.cs        # 窗口生命周期管理 (Window Lifecycle Management)
```

## 🧩 Module Details / 模块详情

### 核心层 (Core Layer)

| File | Responsibility (职责) | Key Features (关键特性) |
| :--- | :--- | :--- |
| `App.xaml.cs` | **程序入口点**。负责单例检查 (Mutex)、全局异常捕获以及应用启动流程。 | `OnStartup`, `SingleInstance`, `GlobalExceptionHandler` |
| `MainWindow.xaml.cs` | **主窗口交互**。处理窗口移动、关闭、以及基于状态的 UI 动效（如胶囊切换）。 | `WindowDrag`, `StateAnimations` |
| `MainViewModel.cs` | **大脑**。管理所有笔记数据 (`FilteredItems`)、搜索逻辑、拖拽排序接口 (`Drop`) 以及 UI 状态。 | `FilteredItems`, `Search`, `GongDragDrop Implementation` |

### 视图层 (View Layer)

| File | Responsibility (职责) | Key Features (关键特性) |
| :--- | :--- | :--- |
| `NoteCard.xaml` | **核心展示组件**。渲染单个笔记/待办事项，包含 Markdown 预览、内联标签编辑和独立的标签拖拽逻辑。 | `MarkdownPreview`, `TagDragDrop (Native)`, `InlineEditor` |
| `FloatingWindow.xaml` | **悬浮球**。接收文件拖入事件，提供极简的桌面入口。 | `DropHandler`, `TransparencyEffects` |
| `SettingsWindow.xaml` | **设置界面**。提供外观、快捷键和行为的配置选项。 | `ThemeSelector`, `HotkeyConfig` |

### 服务层 (Service Layer)

| File | Responsibility (职责) | Key Features (关键特性) |
| :--- | :--- | :--- |
| `MetadataService.cs` | **数据持久化**。负责 `metadata.json` 的原子性读写，确保数据完整性。 | `LoadAsync`, `SaveAsync`, `AtomicWrite` |
| `StorageService.cs` | **文件管理**。处理文件的复制、删除重命名，以及快捷方式 (`.lnk`) 的解析与创建。 | `CopyFile`, `CreateShortcut`, `HashFileName` |
| `HotkeyService.cs` | **全局热键**。基于 `NHotkey` 库注册和响应 `Ctrl+Alt+Q` 等全局快捷键。 | `Register`, `ToggleWindow` |
| `DragDropService.cs` | **拖拽逻辑**。处理外部文件拖入后的类型识别（图片 vs 文本 vs 文件）。 | `HandleDrop`, `ExtractContent` |

## ⚙️ Key Mechanisms / 关键机制

### 混合拖拽系统 (Hybrid Drag & Drop)
Drag2Note-Lite 采用独特的混合拖拽架构来解决 UI 嵌套冲突：
1.  **卡片排序 (Outer Layer)**: 使用 `GongSolutions.WPF.DragDrop` 库。
    *   **触发**: 在 `NoteCard` 的空白区域拖动。
    *   **逻辑**: `MainViewModel` 实现 `IDropTarget` 接口，处理 `ObservableCollection` 的重新排序。
2.  **标签排序 (Inner Layer)**: 使用 WPF 原生 `DragDrop`。
    *   **触发**: 在 `TagItem` (Border) 上按下鼠标 (`PreviewMouseLeftButtonDown`)。
    *   **冲突解决**: `CardListDragSource` 检测到拖动源为 Tag 时，会主动拦截 Gong 的拖拽事件，将控制权移交给原生逻辑。

### 数据存储架构 (Data Architecture)
*   **Metadata**: `UserData/metadata.json` 存储索引、标签和状态。
*   **Content**: 实际内容存储为 Markdown 文件 (`.md`)，图片和附件存储在 `UserAssets` 目录。
*   **Privacy**: 所有数据仅存储在本地，不上传云端。

> Last Updated / 最后更新: 2026-02-06
