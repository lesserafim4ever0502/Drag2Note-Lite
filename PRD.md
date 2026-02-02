# Product Requirements Document (PRD): Drag2Note

**Project Name:** Drag2Note
**Version:** 1.0.0
**Platform:** Windows (Electron)
**Target Audience:** Users needing lightweight, high-efficiency fragment information collection and management.

---

## 1. 产品概述 (Executive Summary)
Drag2Note 是一款极简的 Windows 桌面笔记应用。核心理念通过“拖拽”完成信息的快速收集，并通过轻量级的主窗口进行分类查看。它结合了“浮窗稍后处理”与“卡片式管理”的逻辑，强调极速启动和系统级无缝集成。

## 2. 核心用户流程 (User Flow)
1.  **收集**: 用户通过快捷键唤起或鼠标划过屏幕边缘唤出“浮窗”，将文本、图片、文件、链接拖入。
2.  **处理**: 拖入后，用户选择生成 "Note" 或 "To-do"。
3.  **生成**: 应用自动将资源分类、排序，并生成 Markdown 文件。
4.  **管理**: 用户在主窗口查看卡片，通过左右方向键切换 Note/To-do 视图，拖拽卡片排序。

---

## 3. 功能需求详情 (Functional Requirements)

### 3.1 浮窗 (Floating Window - The "Drop Zone")
这是应用的核心入口。

* **默认状态**:
    * **隐藏/显示**: 可通过全局快捷键切换显示状态。
    * **置顶**: 始终位于屏幕最顶层，支持透明度调节。
* **拖拽交互 (Drag & Drop)**:
    * 窗口作为一个 Drop Zone，接受 Text, Image, URL, File。
    * 支持**连续拖拽**: 用户可以分多次拖入不同资源。
* **决策逻辑**:
    * 拖入资源后，UI 弹出三个选项按钮: `[Note]`, `[To-do]` 和 `[Insert]`。
    * **Insert (插入模式)**: 点击后，内容暂存，唤起主窗口进入“选择卡片”模式，用户点击目标卡片后将内容追加到该卡片末尾。
    * 如果用户未点击按钮，Drop Zone 保持激活，允许继续拖入资源。
* **资源处理逻辑 (Resource Processor)**:
    * 点击创建按钮后，按以下顺序重组所有拖入的资源：
        1.  **Text** (`\n` 分隔)
        2.  **Images/Files**: 不直接复制源文件，而是创建 **Windows 快捷方式 (.lnk)** 到 `/resources` 文件夹。
            *   **优势**: 极大节省磁盘空间，避免数据冗余。
            *   **兼容性**: 应用内部读取 `.lnk` 原路径进行渲染或调用系统打开。
        3.  **Links**: (url链接)
    * **Note 模式**: 生成标准 Markdown。
    * **To-do 模式**: 将所有资源整合，每一项资源前添加 Markdown 复选框语法 `- [ ]`。

### 3.2 主窗口 (Main Dashboard)
用于管理和查看内容的面板。

* **视图切换 (View Switching)**:
    * **快捷键**: 当主窗口激活时，按键盘 `Left Arrow` (←) 或 `Right Arrow` (→) 键。
    * **逻辑**: 在 "Note View" 和 "To-do View" 之间循环切换。
    * **默认**: 应用启动时，**优先显示 "To-do View"**。
* **卡片展示 (Card UI)**:
    * **数据源**: 读取本地生成的 `.md` 文件。
    * **标题**: 显示 `customTitle` (存储在 metadata 中)，**非**文件名。单击标题可进入编辑模式进行重命名。
    * **预览**: 仅读取 `.md` 文件的**首行文本**作为摘要显示。
    * **排序**: 支持自由拖拽卡片进行排序 (Drag-and-Sort)，顺序需持久化保存。
* **To-do 特有交互**:
    * 卡片上显示复选框。
    * 勾选复选框: 更新 metadata 状态为 `completed`，卡片视觉变暗 (Dimmed)。
    * 取消勾选: 恢复高亮。
* **边缘停靠 (Auto-Hide Docking)**:
    * 当主窗口被拖动到屏幕左/右边缘时，自动吸附并隐藏。
    * 鼠标悬停边缘时滑出，移开后自动收回 (延迟 500ms)。
* **打开文件**: 双击卡片或点击编辑按钮，在**应用内 Markdown 编辑器**中打开。支持实时预览 (Preview) 和源码编辑 (Edit) 模式切换。

### 3.3 设置窗口 (Settings)
分为三个 Tab 页面。

* **Tab 1: 基础 (General)**
    * 暗黑模式 (Dark Mode) 开关。
    * 开机自启 (Launch at Startup) 开关。
    * 检查更新 (Check for Updates) 按钮。
* **Tab 2: 主窗 (Main Window)**
    * 设置呼出主窗的全局快捷键。
    * 设置选择 "Note/To-do" 的快捷键 (可选)。
    * **To-do 排序**: 开关 "已完成自动置底" (Completed items to bottom), "未完成自动置顶"。
* **Tab 3: 浮窗 (Floating Window)**
    * 开启/关闭浮窗功能。
    * 浮窗透明度调节 (Slider: 0% - 100%)。
    * 设置呼出/隐藏浮窗的全局快捷键。

### 3.4 系统托盘 (System Tray)
* **图标交互**:
    * 左键单击 (Click): 切换 浮窗 显示/隐藏。
    * 左键双击 (Double Click): 切换 主窗口 显示/隐藏。
* **右键菜单 (Context Menu)**:
    * 显示/隐藏 主窗口 (动态文案)
    * 显示/隐藏 浮窗 (动态文案)
    * 应用设置 (打开设置窗口)
    * 退出应用 (Quit)

---

## 4. 数据架构 (Data & Architecture)

### 4.1 目录结构 (File Structure)
应用应采用“本地优先”策略，数据完全归属于用户。

```text
/UserData
  /notes           # 存放生成的 .md 文件 (文件名: Note-{timestamp}.md)
  /resources       # 存放拖入的图片/附件副本
  metadata.json    # 核心索引文件
  settings.json    # 用户配置文件
```

### 4.2 Metadata Schema (`metadata.json`)
为了支持“自定义标题”和“自由排序”，不能仅依赖文件系统，必须维护一个 JSON 索引。

```typescript
interface AppData {
  items: MetadataItem[];
}

interface MetadataItem {
  id: string;             // UUID
  type: 'note' | 'todo';
  filePath: string;       // 关联的本地 MD 文件路径
  customTitle: string;    // 用户在卡片上设置的标题 (独立于文件名)
  previewText: string;    // 缓存的首行文本，减少IO读取
  status: 'pending' | 'completed'; // 仅针对 To-do
  orderIndex: number;     // 用于卡片自由排序
  createdAt: number;
}
```

### 4.3 Settings Schema (`settings.json`)
存储用户偏好与全局快捷键配置。

```typescript
interface Settings {
  firstRun: boolean;
  general: {
    darkMode: boolean;
    autoLaunch: boolean;
  };
  mainWindow: {
    shortcut: string;           // 主窗口开关快捷键 (e.g. "CommandOrControl+Alt+K")
    sortCompletedBottom: boolean;
    docking: {
       enabled: boolean;
       hideDelay: number;
    }
  };
  floatingWindow: {
    enabled: boolean;
    opacity: number;
    shortcut: string;           // 浮窗开关快捷键
  };
}
```

---

## 5. 技术栈与非功能需求
* **框架**: Electron + React + TypeScript + Vite.
* **样式**: Tailwind CSS (追求轻量化 UI).
* **状态管理**: Zustand 或 React Context.
* **持久化**: `electron-store` (用于设置), `fs` (用于 metadata 和文件).
* **性能**:
    * 浮窗必须常驻内存，响应延迟 < 200ms。
    * 主窗口关闭时默认最小化到托盘，只有在托盘菜单点击“退出”时才销毁进程。

---

## 6. UI 参考
* **卡片布局**: 请严格参考附件 `./reference/Note_window.png` (Note View) 和 `./reference/To_do_window.png` (To-do View)。
* **设置菜单**: 请参考 `./reference/Settings_window.png`。
* **风格**: 米色背景 (Beige/Cream) 搭配手绘风格边框 (Hand-drawn style borders) 或 极简圆角边框 (Clean rounded borders)，具体视开发阶段 UI 库而定。