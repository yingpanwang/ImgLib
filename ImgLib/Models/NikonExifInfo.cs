namespace ImgLib.Models;

public sealed partial record class NikonExifInfo : ExifInfo
{
    public NikonExifInfo(ExifInfo original) : base(original) { }

    public NikonExifInfo(string filePath) : base(filePath) { }

    public NikonExifInfo(Stream fileStream) : base(fileStream) { }

    // ═══ 水印模板字段解析 ═══

    /// <summary>
    /// 覆盖基类，追加 Nikon 专用 EXIF 字段映射。
    /// </summary>
    public override IReadOnlyDictionary<string, string?> GetTemplateReplacements()
    {

        var dict = base.GetTemplateReplacements().ToDictionary();
        dict["CameraSerialNumber"] = CameraSerialNumber;
        dict["FirmwareVersion"] = FirmwareVersion;
        dict["ShutterCount"] = ShutterCount;
        dict["LensType"] = LensType;
        dict["AfType"] = AfType;
        dict["AfFocusPosition"] = AfFocusPosition;
        dict["QualityAndFileFormat"] = QualityAndFileFormat;
        dict["ColorMode"] = ColorMode;
        dict["ActiveDLighting"] = ActiveDLighting;
        dict["VignetteControl"] = VignetteControl;
        dict["HighIsoNoiseReduction"] = HighIsoNoiseReduction;
        dict["PictureControlName"] = PictureControlName;
        dict["ImageStabilisation"] = ImageStabilisation;
        dict["ShootingMode"] = ShootingMode;
        dict["FlashUsed"] = FlashUsed;
        dict["FlashMode"] = FlashMode;
        dict["IsoMode"] = IsoMode;
        dict["SceneMode"] = SceneMode;
        dict["MultiExposure"] = MultiExposure;
        dict["PictureControlSharpness"] = PictureControlSharpness;
        dict["PictureControlClarity"] = PictureControlClarity;
        dict["PictureControlContrast"] = PictureControlContrast;
        dict["PictureControlBrightness"] = PictureControlBrightness;
        dict["PictureControlSaturation"] = PictureControlSaturation;
        return dict.AsReadOnly();
    }

    // ═══ Makernote Directory Accessors ═══

    private NikonType1MakernoteDirectory? Type1Directory
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonType1MakernoteDirectory>().FirstOrDefault();
            return field;
        }
    }

    private NikonType2MakernoteDirectory? Type2Directory
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonType2MakernoteDirectory>().FirstOrDefault();
            return field;
        }
    }

    private NikonPictureControl1Directory? PictureControl1
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonPictureControl1Directory>().FirstOrDefault();
            return field;
        }
    }

    private NikonPictureControl2Directory? PictureControl2
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonPictureControl2Directory>().FirstOrDefault();
            return field;
        }
    }

    // ═══ 机身信息 ═══

    /// <summary>机身序列号</summary>
    public string? CameraSerialNumber =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagCameraSerialNumber)
        ?? Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagCameraSerialNumber2);

    /// <summary>固件版本</summary>
    public string? FirmwareVersion =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagFirmwareVersion);

    /// <summary>快门次数（ImageCount，部分机型提供）</summary>
    public string? ShutterCount =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagImageCount);

    /// <summary>开机时间（PowerUpTime）</summary>
    public string? PowerUpTime =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagPowerUpTime);

    // ═══ 镜头 / 对焦 ═══

    /// <summary>镜头类型（Nikon 专用，比标准 LensModel 更详细）</summary>
    public string? LensType =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagLensType);

    /// <summary>镜头数据</summary>
    public string? Lens =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagLens);

    /// <summary>对焦类型（AF-S / AF-C / MF 等）</summary>
    public string? AfType =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagAfType);

    /// <summary>AF 对焦点位置</summary>
    public string? AfFocusPosition =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagAfFocusPosition);

    /// <summary>手动对焦距离</summary>
    public string? ManualFocusDistance =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagManualFocusDistance);

    /// <summary>AF 微调（AF Fine Tune）</summary>
    public string? AfTune =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagAfTune);

    // ═══ 画质 / 处理 ═══

    /// <summary>画质与文件格式（JPEG Fine / RAW 等）</summary>
    public string? QualityAndFileFormat =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagQualityAndFileFormat);

    /// <summary>色彩模式</summary>
    public string? ColorMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagColorMode);

    /// <summary>Active D-Lighting</summary>
    public string? ActiveDLighting =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagActiveDLighting);

    /// <summary>暗角控制</summary>
    public string? VignetteControl =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagVignetteControl);

    /// <summary>高 ISO 降噪</summary>
    public string? HighIsoNoiseReduction =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagHighIsoNoiseReduction);

    /// <summary>降噪设置</summary>
    public string? NoiseReduction =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagNoiseReduction);

    /// <summary>NEF 压缩方式</summary>
    public string? NefCompression =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagNefCompression);

    /// <summary>传感器像素尺寸</summary>
    public string? SensorPixelSize =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagSensorPixelSize);

    // ═══ 拍摄信息 ═══

    /// <summary>拍摄模式（单张/连拍/自拍等）</summary>
    public string? ShootingMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagShootingMode);

    /// <summary>场景模式</summary>
    public string? SceneMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagSceneMode);

    /// <summary>Image Stabilisation / VR</summary>
    public string? ImageStabilisation =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagImageStabilisation);

    /// <summary>多重曝光</summary>
    public string? MultiExposure =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagMultiExposure);

    /// <summary>Digital Vari-Program / Picture Control 模式</summary>
    public string? DigitalVariProgram =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagDigitalVariProgram);

    /// <summary>曝光序列号（包围曝光）</summary>
    public string? ExposureSequenceNumber =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagExposureSequenceNumber);

    /// <summary>色调效果</summary>
    public string? ToningEffect =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagToningEffect);

    /// <summary>饱和度2</summary>
    public string? Saturation =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagSaturation2);

    // ═══ 闪光灯 ═══

    /// <summary>是否使用闪光灯</summary>
    public string? FlashUsed =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagFlashUsed);

    /// <summary>闪光灯模式</summary>
    public string? FlashMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagFlashMode);

    /// <summary>闪光灯同步模式</summary>
    public string? FlashSyncMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagFlashSyncMode);

    /// <summary>闪光灯曝光补偿</summary>
    public string? FlashExposureCompensation =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagFlashExposureCompensation);

    // ═══ ISO ═══

    /// <summary>ISO 模式</summary>
    public string? IsoMode =>
        Type2Directory?.GetDescription(NikonType2MakernoteDirectory.TagIsoMode);

    // ═══ Picture Control (PictureControl2 优先，包含 Clarity 等新参数) ═══

    /// <summary>Picture Control 名称（如 "STANDARD"、"NEUTRAL"）</summary>
    public string? PictureControlName =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagPictureControlName)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagPictureControlName);

    /// <summary>Picture Control 快速调整</summary>
    public string? PictureControlQuickAdjust =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagPictureControlQuickAdjust)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagPictureControlQuickAdjust);

    /// <summary>锐度</summary>
    public string? PictureControlSharpness =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagSharpness)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagSharpness);

    /// <summary>清晰度（Clarity，仅 PictureControl2）</summary>
    public string? PictureControlClarity =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagClarity);

    /// <summary>对比度</summary>
    public string? PictureControlContrast =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagContrast)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagContrast);

    /// <summary>亮度</summary>
    public string? PictureControlBrightness =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagBrightness)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagBrightness);

    /// <summary>饱和度</summary>
    public string? PictureControlSaturation =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagSaturation)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagSaturation);

    /// <summary>色相</summary>
    public string? PictureControlHue =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagHue)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagHueAdjustment);

    /// <summary>滤镜效果</summary>
    public string? PictureControlFilterEffect =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagFilterEffect)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagFilterEffect);

    /// <summary>调色效果</summary>
    public string? PictureControlToningEffect =>
        PictureControl2?.GetDescription(NikonPictureControl2Directory.TagToningEffect)
        ?? PictureControl1?.GetDescription(NikonPictureControl1Directory.TagToningEffect);
}
