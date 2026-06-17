// @CodeScene(disable:"Constructor Over-Injection")
using ImgLib.Models;
using SkiaSharp;

namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 水印渲染上下文 —— 携带渲染所需的所有运行时数据。
/// 由管线执行器创建并传递给每个命令/访问者对。
/// </summary>
public class WatermarkRenderContext
{
    // ─── 画布与位图（由管线执行器注入）───

    /// <summary>SkiaSharp 绘制画布</summary>
    public SKCanvas Canvas { get; }

    /// <summary>原始解码图片</summary>
    public SKBitmap SourceBitmap { get; }

    /// <summary>当前工作位图（可能经降采样处理）</summary>
    public SKBitmap WorkingBitmap { get; }

    /// <summary>画布/位图宽度（像素）</summary>
    public int Width { get; }

    /// <summary>画布/位图高度（像素）</summary>
    public int Height { get; }

    // ─── 中央图片布局（由管线执行器预计算）───

    /// <summary>中央图片在画布上的目标矩形</summary>
    public SKRect ImageDestRect { get; }

    /// <summary>中央图片缩放后的宽度</summary>
    public float NewWidth { get; }

    /// <summary>中央图片缩放后的高度</summary>
    public float NewHeight { get; }

    // ─── 可选元数据 ───

    /// <summary>EXIF 信息（用于模板变量替换）</summary>
    public ExifInfo? Exif { get; }

    // ─── 文本水印布局缓存（由 TextWatermarkHandler 计算并存储）───

    /// <summary>所有文本水印块的布局信息列表（按绘制顺序）</summary>
    public List<TextBlockLayout> TextBlockLayouts { get; } = new();

    // ─── 以下属性保留用于向后兼容（单文本场景）───

    /// <summary>解析并拆分后的文本行</summary>
    public string[]? TextLines { get; set; }

    /// <summary>文本行数</summary>
    public int TextLineCount { get; set; }

    /// <summary>实际使用的字体大小</summary>
    public float FontSize { get; set; }

    /// <summary>最长行的宽度（像素）</summary>
    public float MaxLineWidth { get; set; }

    /// <summary>行高（含间距）</summary>
    public float LineHeight { get; set; }

    /// <summary>基线以上的字形高度</summary>
    public float AscentAbove { get; set; }

    /// <summary>基线以下的字形高度</summary>
    public float DescentBelow { get; set; }

    /// <summary>文本块总高度</summary>
    public float TotalBlockHeight { get; set; }

    /// <summary>首行基线 Y 坐标</summary>
    public float FirstLineBaselineY { get; set; }

    /// <summary>文本水平起始 X 坐标</summary>
    public float TextX { get; set; }

    /// <summary>水平对齐方式</summary>
    public SKTextAlign TextAlign { get; set; }

    /// <summary>边框矩形（调试用）</summary>
    public SKRect DebugBorderRect { get; set; }

    // ─── 构造函数 ───

    public WatermarkRenderContext(
        SKCanvas canvas,
        SKBitmap sourceBitmap,
        SKBitmap workingBitmap,
        int width,
        int height,
        float scale,
        ExifInfo? exif = null)
    {
        Canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        SourceBitmap = sourceBitmap ?? throw new ArgumentNullException(nameof(sourceBitmap));
        WorkingBitmap = workingBitmap ?? throw new ArgumentNullException(nameof(workingBitmap));
        Width = width;
        Height = height;
        Exif = exif;

        // 预计算中央图片布局
        NewWidth = width * scale;
        NewHeight = height * scale;
        float x = (width - NewWidth) / 2;
        float y = (height - NewHeight) / 2;
        ImageDestRect = new SKRect(x, y, x + NewWidth, y + NewHeight);
    }
}
