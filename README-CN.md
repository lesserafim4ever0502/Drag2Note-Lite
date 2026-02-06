# Drag2Note-Lite

> **一款“拖拽优先”的轻量级 Windows 桌面记事应用**
>
> 🌐 **[English](README.md)**

Drag2Note-Lite 是一款极简却强大的生产力工具，旨在以思维的速度捕获信息。通过将文本、图片或文件拖入桌面悬浮球，你可以瞬间将稍纵即逝的想法转化为结构化、可搜索的笔记。

![Hero Image](Screenshot/windows.png)

## 🌟 为什么选择 Drag2Note-Lite？

在臃肿且依赖云服务的笔记应用泛滥的今天，Drag2Note-Lite 坚持以下原则：
1.  **极致速度**: 零延迟启动，通过全局热键瞬间呼出。
2.  **完全隐私**: 100% 本地化。你的数据只存在于你的硬盘上，绝不上传云端。
3.  **原生美学**: 精心打磨的 WPF 界面，拥有原生应用的流畅与高级感。
4.  **超低占用**: 极致优化，运行时仅约 **50MB** 内存占用。

---

## ✨ 核心特性

### 🖱️ 高级拖拽工作流 (Hybrid Drag Engine)
> **将任何内容拖到悬浮球即可瞬间捕获。**

![Floating Interaction](Screenshot/Floating-decision.png)

我们构建了一个**混合拖拽引擎**，完美融合了两种拖拽体验：
- **卡片排序**: 由 `GongSolutions.WPF.DragDrop` 驱动，提供丝滑的列表重排动画和半透明幽灵占位符。
- **标签管理**: 自定义原生实现，允许你在可拖拽的卡片内部独立拖拽标签，互不冲突。
- **文件捕获**: 将任何文件拖入悬浮球即可创建笔记。图片自动生成预览，文本文件自动解析内容。

### 🏷️ 智能与流畅的标签系统
![Editor & Tagging](Screenshot/card-edit.png)

- **自动日期**: 新笔记自动添加 `YYYY-MM-DD` 格式的创建日期标签。
- **拖拽排序**: 像管理卡片一样自由调整标签顺序。
- **内联编辑**: 双击任意标签即可重命名。

### 🎨 自适应 UI / UX
![Dark Mode](Screenshot/windows-dark.png)

- **真·深色模式**: 界面（包括悬停状态、按钮样式、文字对比度）会根据系统主题动态切换。
- **精致交互**: 
  - **胶囊切换**: "笔记/待办" 和 "编辑/预览" 模式切换拥有平滑的过渡动画。
  - **圆角控件**: 自定义的 `ControlTemplate` 确保每一个按钮（如圆角返回键）都有细腻的触感反馈。

### 🛠️ 开发者友好的架构
- **清晰 MVVM**: 基于 `CommunityToolkit.Mvvm` 构建，逻辑与视图分离。
- **本地 JSON**: 数据存储为易读的 `metadata.json` 和标准 Markdown 文件。
- **自行构建**: 无私有代码。只需 .NET 8.0 SDK 即可自行编译。

---

## 🚀 快速开始

### 安装
1.  前往 [Releases](https://github.com/lesserafim4ever0502/Drag2Note-Lite/releases) 页面。
2.  下载 `Drag2Note-Lite_Setup.exe` (或便携版 `.zip`)。
3.  运行应用程序。

### 基础用法
- **全局热键**: 按下 `Ctrl + Alt + Q` 快速显示/隐藏主面板。
- **捕获**: 将文件拖到屏幕右下角的悬浮球上。
- **编辑**: 点击卡片进入 "编辑器模式"。
- **预览**: 点击 "眼睛" 图标切换 Markdown 渲染预览。

---

## 🛠 技术栈

本项目基于以下强大的开源技术构建：

| 分类 | 技术 | 用途 |
| :--- | :--- | :--- |
| **核心** | .NET 8.0 (C# 12) | 高性能运行时。 |
| **UI 框架** | WPF (Windows Presentation Foundation) | 硬件加速的桌面 UI。 |
| **架构** | MVVM (CommunityToolkit) | 清晰的代码结构与数据绑定。 |
| **拖拽** | GongSolutions.WPF.DragDrop | 处理复杂的拖拽交互。 |
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
3.  提交更改 (`git commit -m 'Add amazing feature'`)。
4.  推送到分支。
5.  发起 Pull Request。

---

## 📄 许可证

本项目基于 MIT 许可证分发。详见 `LICENSE` 文件。

## 🙏 致谢

- **GongSolutions** 提供出色的拖拽库。
- **Microsoft** 提供现代化的 .NET 生态。
- **Icons8** 提供视觉资源。
