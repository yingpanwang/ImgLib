global using System.Diagnostics;
global using System.IO;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Collections.Specialized;
global using System.ComponentModel;
global using System.Linq;
global using System.Text;
global using System.Text.Json;



global using Microsoft.Extensions.DependencyInjection;
// ImgLib
global using ImgLib.Models;
global using ImgLib.UI.Messages;
global using ImgLib.UI.Models;
global using ImgLib.UI.Services;
global using ImgLib.UI.Views;
global using ImgLib.WatermarkPipeline;
global using ImgLib.Services;

#region MVVM 组件

global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using CommunityToolkit.Mvvm.Messaging;
global using System;

#endregion

#region Avalonia 的组件

global using SkiaSharp;

global using Avalonia.Controls;
global using Avalonia.Media.Imaging;
global using Avalonia.Threading;
global using Avalonia.Media;

#endregion