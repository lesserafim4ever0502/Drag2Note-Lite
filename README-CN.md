# Drag2Note-Lite

> **一款“拖拽优先”的轻量级 Windows 桌面记事应用**
>
> 🌐 **[English](README.md)**

Drag2Note-Lite 是一款极简且实用的生产力工具，旨在快速捕获信息。通过将文本、图片或文件拖入桌面悬浮球，你可以将想法转化为结构化、可搜索的笔记。

<p align="center">
  <img src="Screenshot/windows.png" alt="Hero Image" width="100%">
</p>

## 🌟 为什么选择 Drag2Note-Lite？

不同于复杂的云笔记应用，Drag2Note-Lite 关注以下原则：
1.  **快速启动**: 通过全局热键快速呼出。
2.  **隐私**: 100% 本地化。你的数据只存在于你的硬盘上。
3.  **原生体验**: 整洁的 WPF 界面，提供原生应用般的使用体验。
4.  **低资源占用**: 经过优化，运行时仅约 **50MB** 内存占用。

---

## ✨ 核心特性

### 🖱️ 拖拽工作流
> **将内容拖到悬浮球即可捕获。**

<p align="center">
  <img src="Screenshot/Floating-idle.png" alt="Floating Ball" width="40%" style="margin-right: 20px" />
  <img src="Screenshot/Floating-decision.png" alt="Floating Interaction" width="40%" />
</p>

我们结合了以下特性：
- **卡片排序**: 采用原生 WPF 拖放事件实现，确保交互的流畅性与轻量化。
- **标签管理**: 自定义原生实现，允许你在可拖拽的卡片内部独立拖拽标签，互不冲突。
- **文件捕获**: 将任何文件拖入悬浮球即可创建笔记。图片自动生成预览，文本文件自动解析内容。

### 🏷️ 标签系统 (主界面)
<p align="center">
  <img src="Screenshot/main-notes.png" alt="Notes List" width="45%" style="margin-right: 10px"/>
  <img src="Screenshot/main-todo.png" alt="Todo List" width="45%"/>
</p>

- **自动日期**: 新笔记自动添加 `YYYY-MM-DD` 格式的创建日期标签。
- **拖拽排序**: 像管理卡片一样自由调整标签顺序。
- **内联编辑**: 双击任意标签即可重命名。

### 🎨 UI / UX

<p align="center">
  <img src="Screenshot/windows-dark.png" alt="Dark Mode" width="100%">
</p>

- **深色模式**: 界面会根据系统主题动态切换。
- **界面交互**: 
  - **胶囊切换**: "笔记/待办" 和 "编辑/预览" 模式切换拥有平滑的过渡动画。
  - **圆角控件**: 自定义的 `ControlTemplate` 确保每一个按钮都有清晰的反馈。

### 📝 编辑器与预览
<p align="center">
  <img src="Screenshot/card-edit.png" alt="Editor & Tagging" width="48%"/>
  <img src="Screenshot/card-preview.png" alt="Markdown Preview" width="48%"/>
</p>

### ⚙️ 设置选项
<p align="center">
  <img src="Screenshot/settings-main.png" alt="Settings Main" width="32%"/>
  <img src="Screenshot/settings-general.png" alt="Settings General" width="32%"/>
  <img src="Screenshot/settings-floating.png" alt="Settings Floating" width="32%"/>
</p>

### 🛠️ 架构设计
- **清晰 MVVM**: 基于 `CommunityToolkit.Mvvm` 构建，逻辑与视图分离。
- **本地 JSON**: 数据存储为易读的 `metadata.json` 和标准 Markdown 文件。
- **自行构建**: 无私有代码。只需 .NET 8.0 SDK 即可自行编译。

---

## 🚀 快速开始

### 安装
1.  前往 [Releases](https://github.com/lesserafim4ever0502/Drag2Note-Lite/releases) 页面。
2.  下载 `Drag2Note-Lite_Setup.exe`。
3.  运行应用程序。

### 基础用法
- **全局热键**: 按下 `Ctrl + Alt + K` 快速显示/隐藏主面板。
- **捕获**: 将文件拖到屏幕右下角的悬浮球上。
- **编辑**: 点击卡片进入 "编辑器模式"。
- **预览**: 点击 "眼睛" 图标切换 Markdown 渲染预览。

---

## 🛠 技术栈

本项目基于以下开源技术构建：

| 分类 | 技术 | 用途 |
| :--- | :--- | :--- |
| **核心** | .NET 8.0 (C# 12) | 高性能运行时。 |
| **UI 框架** | WPF (Windows Presentation Foundation) | 硬件加速的桌面 UI。 |
| **架构** | MVVM (CommunityToolkit) | 清晰的代码结构与数据绑定。 |
| **拖拽** | Native WPF Events | 处理拖拽交互。 |
| **输入** | NHotkey.Wpf | 全局键盘钩子。 |
| **系统** | Hardcodet.NotifyIcon | 系统托盘集成。 |

---

## 📂 项目结构

关于代码库的详细解构，请参阅我们的 **[代码地图 (CODEMAP.md)](CODEMAP.md)**。

```
Drag2Note/
├── Views/              # XAML UI 定义
├── ViewModels/         # 应用逻辑
├── Models/             # 数据结构
├── Services/           # 文件 I/O 与业务逻辑
└── UserData/           # 你的笔记 (本地存储)
```

---

## 🤝 贡献代码

欢迎提交 Pull Request！
1.  Fork 本仓库。
2.  创建特性分支 (`git checkout -b feature/amazing-feature`)。
3.  提交更改 (`git commit -m 'Add new feature'`)。
4.  推送到分支。
5.  发起 Pull Request。

---

## 📄 许可证

本项目基于 MIT 许可证分发。详见 `LICENSE.md` 文件。

## 🙏 致谢

- **Microsoft** 提供 .NET 生态。
- **Icons8** 提供视觉资源。
