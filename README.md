# Drag2Note-Lite

[English](#english) | [简体中文](#chinese)

<a name="english"></a>

## English

Drag2Note-Lite is a lightweight, efficient Windows desktop note-taking application designed for quick information capture via drag-and-drop. It extracts content from files and images into searchable, tagged notes.

### 🌟 Features / 功能特性
- **Advanced Drag & Drop / 高级拖拽**: 
  - **Native & Library Hybrid**: Combines `GongSolutions.WPF.DragDrop` for smooth card reordering with custom logic for tag management.
  - **Ghost Adorners**: Semitransparent benchmarks for a premium dragging experience.
  - **混合拖拽引擎**: 结合了 `GongSolutions` 的成熟排序算法与自定义的 `CardListDragSource`，完美解决嵌套组件（Tag）的拖拽冲突。
- **Smart Tagging / 智能标签**: 
  - Automatically adds creation date tags.
  - Supports custom tag creation, dragging, and auto-wrapping layout.
  - 自动日期标签 + 自定义标签，支持独立拖拽排序。
- **Portable & Minimalist / 便携极简**: 
  - Data stored locally in `UserData`. No cloud, no login.
  - 纯本地存储，无云端依赖。
- **Adaptive UI / 自适应界面**: 
  - **True Dark Mode**: Styles (including hover states) adapt dynamically to system theme.
  - **Fluid Animations**: Custom `ControlTemplate` for interactive elements (e.g., rounded Back button with hover effects).
  - 完美适配深色模式，拥有流畅的 CSS 级动效与圆角交互体验。
- **Global Hotkeys**: Quick access via `Ctrl+Alt+Q`.

### 🚀 Getting Started / 快速开始
1. Download the latest release or build from source.
2. Run `Drag2Note-Lite.exe`.
3. Use global hotkey or drag files to the floating window.

---

## 🛠 Tech Stack / 技术栈
- **Core Framework**: .NET 8.0, WPF
- **Language**: C# 12, XAML
- **MVVM**: CommunityToolkit.Mvvm
- **Data Persistence**: JSON (Metadata) + Markdown (Content)
- **Key Libraries**:
  - `GongSolutions.WPF.DragDrop`: For advanced drag-and-drop interactions.
  - `NHotkey.Wpf`: For global keyboard shortcuts.
- **Design System**: 
  - Custom XAML Resource Dictionary for theming.
  - Responsive layouts with `UniformGrid` and `WrapPanel`.

## 📄 License / 许可证
MIT License

## 🙏 Credits / 致谢
- **GongSolutions.WPF.DragDrop** for the amazing drag library.
- Icons by Icons8 & Iconfont.
