# ImgLib

> 一款桌面水印工具，支持图片水印添加、批量导出和实时预览。

基于 **.NET 8 + Avalonia UI + SkiaSharp** 构建，支持 JPEG / PNG / RAW 等多种图片格式，提供丰富的视觉效果（虚化背景、圆角、阴影、多行文字水印）和高效的批量导出能力。

---

## 功能特性

### 图片管理
- 支持 **JPEG、PNG、GIF** 格式
- 支持主流 RAW 格式：**Nikon NEF** (后续计划添加多种RAW格式解析)
- 缩略图列表浏览，支持**分页加载**和**无限滚动**

### 水印效果
- **虚化背景**（Gaussian Blur），可调节模糊强度（预设 + 自定义）
- **圆角裁剪**，可调节圆角半径（预设 + 自定义）
- **图片阴影**，支持 X/Y 偏移和模糊强度（预设 + 自定义）
- **多行文字水印**，支持换行符分割
- 水印文字**自适应缩放**（根据图片尺寸自动调整字体大小）
- 水印文字阴影（偏移 + 模糊，预设 + 自定义）

### EXIF 模板
- 读取完整 EXIF 元数据（相机型号、光圈、快门、ISO、焦距等）
- 支持 **Nikon 专属 EXIF 字段**（快门次数、镜头类型、对焦模式、Picture Control 等）
- 水印文字支持 **EXIF 模板变量**（如 `{Model}`、`{FNumber}`、`{ISO}`），使用中文显示名称
- EXIF 信息树形查看面板

### 预览系统
- **实时预览**：参数变化后自动刷新预览图像
- **防抖机制**：可配置自动预览触发间隔
- **预览降采样**：支持按百分比或固定像素值降采样，提升预览性能
- **Pan & Zoom**：支持鼠标拖拽平移和滚轮缩放预览图像
- **RGB 直方图**：浮动直方图叠加层，**可自由拖拽定位**
- 直方图显示/隐藏可切换

### 导出
- **单张导出**：将当前预览图片导出为 JPEG
- **批量导出**：缩略图勾选对话框 → 选择输出目录 → 并行导出（CPU 核心数 - 1 并发）
- 导出进度追踪（完成/失败/进行中计数）
- 支持取消批量导出
- 可配置默认输出格式和 JPEG 质量

### 设置系统
- 系统设置对话框（输出格式、JPEG 质量、预览参数）
- JSON 文件持久化

---

## 技术架构

```
ImgLib.sln
├── ImgLib/                          # 核心类库
│   ├── ImageFile.cs                 # 图片文件模型（JPEG/RAW 工厂）
│   ├── ImageService.cs              # 图片生成引擎
│   ├── ImageGenerateOption.cs       # 生成参数配置
│   ├── ExifInfo.cs                  # EXIF 元数据读取（MetadataExtractor）
│   ├── NikonExifInfo.cs             # Nikon 专属 EXIF 扩展
│   └── WatermarkPipeline/           # 水印渲染管线（Command + Visitor 模式）
│       ├── Commands/                # 渲染命令（虚化、阴影、文字水印等）
│       ├── SkiaWatermarkRenderer.cs # SkiaSharp 渲染器
│       ├── WatermarkPipelineRunner.cs # 管线执行器（Fluent Builder）
│       └── WatermarkRenderContext.cs  # 渲染上下文
│
└── ImgLib.UI/                       # 桌面应用（Avalonia UI）
    ├── Views/                       # 视图层
    │   ├── MainWindow.axaml         # 主窗口
    │   ├── WatermarkDesignView.axaml # 预览 + 水印设计
    │   ├── WatermarkSettingsView.axaml # 设置面板
    │   ├── ImgListBoxView.axaml     # 图片列表（无限滚动）
    │   ├── HistogramView.axaml      # RGB 直方图
    │   ├── ExportDialog.axaml       # 批量导出选择对话框
    │   ├── SettingsWindow.axaml     # 系统设置对话框
    │   └── ToastView.axaml         # Toast 通知
    ├── ViewModels/                  # 视图模型层
    ├── Models/                      # UI 数据模型
    ├── Services/                    # 服务层
    │   ├── SystemSettingsService.cs # 设置持久化
    │   ├── ThumbnailCacheService.cs # 缩略图缓存
    │   ├── ToastService.cs          # 全局 Toast 通知
    │   └── ExifFieldConfigService.cs # EXIF 字段配置
    ├── Behaviors/                   # Avalonia Behavior
    └── Converters/                  # 值转换器
```

### 设计模式
- **MVVM**：CommunityToolkit.Mvvm 源生成器（`[ObservableProperty]`、`[RelayCommand]`）
- **Command + Visitor**：水印渲染管线，支持未来扩展其他渲染后端
- **Fluent Builder**：`WatermarkPipelineRunner` 流式 API

### 技术栈

| 技术                      | 用途              |
| ------------------------- | ----------------- |
| .NET 8                    | 运行时            |
| Avalonia UI 11.3          | 跨平台桌面 UI     |
| CommunityToolkit.Mvvm 8.4 | MVVM 源生成器     |
| SkiaSharp 3.119           | 图片渲染引擎      |
| MetadataExtractor 2.8.1   | EXIF 元数据读取   |
| PanAndZoom 11.3           | 图片预览缩放/平移 |

---

## 快速开始

### 环境要求
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10+ / macOS / Linux

### 构建与运行
```bash
# 克隆仓库
git clone <repo-url>
cd ImgLib

# 还原依赖并编译
dotnet restore
dotnet build

# 运行
dotnet run --project ImgLib.UI
```

### 发布
```bash
# 发布为单文件可执行程序
dotnet publish ImgLib.UI -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Roadmap

### v1.1 — 完善与优化

- [ ] **自定义水印图片**：支持叠加自定义 Logo / 图片水印，可调节透明度、位置和缩放
- [ ] **快捷键支持**：常用操作快捷键（打开文件夹、导出、切换面板等）
- [ ] **拖放添加图片**：支持从资源管理器拖放图片到列表
- [ ] **缩略图缓存管理**：支持查看缓存大小、清除缓存
- [ ] **最近打开的文件夹**：记录并快速切换历史文件夹

### v1.2 — 高级水印功能

- [ ] **平铺水印**：文字/图片水印全图平铺模式，支持间距和旋转角度调节
- [ ] **水印样式预设**：用户可保存/加载/导出水印样式预设（JSON）
- [ ] **批量水印样式**：支持对同一批图片应用不同水印样式
- [ ] **位置微调**：方向键精确调整水印位置
- [ ] **不透明度调节**：水印透明度和混合模式

### v1.3 — 编辑与处理

- [ ] **图片旋转/翻转**：预览中支持旋转和镜像翻转
- [ ] **基础调色**：亮度、对比度、饱和度调节
- [ ] **裁剪工具**：支持自由裁剪和固定比例裁剪
- [ ] **撤销/重做**：编辑操作历史栈

### v1.4 — 工作流增强

- [ ] **配置文件导入/导出**：完整设置备份和迁移
- [ ] **多语言支持**：国际化框架（i18n），英文为首批扩展语言
- [ ] **暗色模式**：深色主题支持
- [ ] **命令行导出**：支持通过 CLI 参数批量导出（用于脚本/自动化）
- [ ] **右键菜单增强**：图片列表右键菜单（重命名、删除、打开位置等）

### v2.0 — 平台扩展

- [ ] **macOS 原生适配**：macOS 原生菜单栏、文件关联
- [ ] **Linux 支持验证**：完整测试和打包（AppImage / Flatpak）
- [ ] **ARM64 支持**：Apple Silicon 和 Windows on ARM 原生构建

---

## 项目结构

```
ImgLib/
├── ImgLib/                              # 核心类库
│   ├── ImageFile.cs                     # 图片文件抽象
│   ├── ImageService*.cs                 # 图片处理引擎
│   ├── ImageGenerateOption.cs           # 生成参数
│   ├── ExifInfo*.cs                     # EXIF 读取
│   ├── NikonExifInfo.cs                 # Nikon EXIF
│   └── WatermarkPipeline/               # 渲染管线
│       ├── Commands/                    # 渲染命令
│       ├── SkiaWatermarkRenderer.cs     # Skia 渲染器
│       └── WatermarkPipelineRunner.cs   # 管线执行器
│
├── ImgLib.UI/                           # 桌面应用
│   ├── Views/                           # Avalonia 视图
│   ├── ViewModels/                      # MVVM 视图模型
│   ├── Models/                          # UI 数据模型
│   ├── Services/                        # 应用服务
│   ├── Behaviors/                       # Avalonia Behavior
│   ├── Controls/                        # 自定义控件
│   └── Converters/                      # 值转换器
│
├── ImgLib.sln                           # 解决方案文件
└── README.md
```

---

## 许可证

MIT License

---

*最后更新于 2026-06-10*
