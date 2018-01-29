
<img src="https://user-images.githubusercontent.com/7021835/35534920-d2410822-04f6-11e8-9de1-87da10468172.png" />

**WPFSpark** project was started by me in December 2009, with the aim of creating a library of rich user controls which can be used by the developer community. Initially, I ported the Circular Progress Control (which I had developed using C# and GDI+) to WPF. Eventually, as I gained more experience in WPF, I added more controls.

**WPFSpark v1.0** was released in December 2011 followed by **WPFSpark v1.1** in January 2012. Now, after a really long gap of nearly 4 years, I am happy to announce that the next version of WPFSpark is finally released. :) It has been ported to **.NET 4.6** using **C# 6.0**.

## Updates

### WPFSpark v1.4 (Jan 29, 2018)
**WPFSpark** now supports the following dot Net Frameworks
- 4.6
- 4.6.1
- 4.6.2
- 4.7
- 4.7.1
- 4.7.2

### WPFSpark.UWP retired (Jul 16, 2016)
The **WPFSPark.UWP** project has been officially retired. Henceforth, the **WPFSpark** project will only contain WPF related code and controls.  
Those who were using the UWP version of **FluidWrapPanel** need not worry. **FluidWrapPanel** has been ported from **WPFSpark  v1.3**, its XAML animation replaced with **Composition** animations and is now a part of the **[CompositionProToolkit](https://github.com/ratishphilip/CompositionProToolkit)** project.
**CompositionProToolkit v0.3** has been released containing the new **FluidWrapPanel** control.

### WPFSpark v1.3 Release (Jul 10, 2016)
WPFSpark v1.3 is now released. The following updates were made
- The Layout logic for **FluidWrapPanel** has been updated with a more robust code.
- WPFSpark project target DotNet Framework has been upgraded to **4.6.2**.

> _**Note: If you are installing this version in your project, please ensure that your project is also targeting the version 4.6.2 of the DotNet Framework.**_

### WPFSpark.UWP Released (Jan 21, 2016)
WPFSpark library for Universal Windows Apps is now released. Check this [blog](https://wpfspark.wordpress.com/2016/01/21/wpfspark-uwp-creating-a-single-nuget-package-containing-x86-x64-and-arm-binaries/) for more details.

# Installing from NuGet

To install **WPFSpark**, run the following command in the **Package Manager Console**

```powershell
Install-Package WPFSpark
```

More details available [here](https://www.nuget.org/packages/WPFSpark/).

## WPFSpark Controls

WPFSpark contains the following controls

- [ClipBorder](#clipborder)
- [SprocketControl](#sprocketcontrol)
- [ToggleSwitch](#toggleswitch)
- [FluidWrapPanel](#fluidwrappanel)
- [SparkWindow](#sparkwindow)
- [FluidPivotPanel](#fluidpivotpanel)
- [FluidProgressBar](#fluidprogressbar)
- [FluidStatusBar](#fluidstatusbar)

### ClipBorder
**ClipBorder** is similar to to **Border** class with an additional feature. It clips its content within its boundaries. It derives from the **Decorator** class and provides the same dependency properties as the **Border** class. You can set the `CornerRadius` property of the **ClipBorder** to obtain a Rounded Rectangular clip. **ClipBorder** powers the **ToggleSwitch** control.

### SprocketControl
**SprocketControl** is a circular progress control similar to the Asynchronous Circular Progress Indicator in Mac OS X. It can behave as either normal or indeterminate progress control.

[SprocketControl Details](http://www.codeproject.com/Articles/203966/WPFSpark-of-n-SprocketControl)

<img src="https://cloud.githubusercontent.com/assets/7021835/12405766/de797bd6-bdfb-11e5-91e1-6389f017d9f4.png" />

### ToggleSwitch

**ToggleSwitch** control derives from **ToggleButton** and supports only two states: `True` or `False`. This control has been completely rewritten from scratch and provides many properties which allow the user to customize the look and feel of the control (e.g. Windows 10 mobile style or iOS style). Users can create their own customized styles.

[ToggleSwitch Details](http://www.codeproject.com/Articles/233583/WPFSpark-of-n-ToggleSwitch). Also check [this](http://www.codeproject.com/Articles/1060961/WPFSpark-v) for new features added in WPFSpark v1.2.

<img src="https://cloud.githubusercontent.com/assets/7021835/12362235/0d5885b4-bb77-11e5-9f4d-b4184adea8f6.png" />

<img src="https://cloud.githubusercontent.com/assets/7021835/12362238/10141a0c-bb77-11e5-9949-5021405e3334.png" />

### FluidWrapPanel
**FluidWrapPanel** is another control which derives from **Panel** and provides the functionality of a WrapPanel with an added advantage - *the child elements of the panel can be easily rearranged by simple drag and drop*. It has been rewritten from scratch to accommodate children of non-uniform size.

[FluidWrapPanel Details](http://www.codeproject.com/Articles/244134/WPFSpark-of-n-FluidWrapPanel). Also check [this](http://www.codeproject.com/Articles/1060961/WPFSpark-v) for new features added in WPFSpark v1.2.

<img src="https://cloud.githubusercontent.com/assets/7021835/12362226/040ef8a8-bb77-11e5-8cef-30f1c3a8d11d.png" />

### SparkWindow
**SparkWindow** is a custom Window which has the look and feel of Windows 10 desktop window with an additional feature : **Blur behind (Aero glass effect)**.

[SparkWindow Details](http://www.codeproject.com/Articles/303688/WPFSpark-of-n-SparkWindow). Also check [this](http://www.codeproject.com/Articles/1060961/WPFSpark-v) for new features added in WPFSpark v1.2.

<img src="https://cloud.githubusercontent.com/assets/7021835/12362259/27ee7082-bb77-11e5-9a35-ea5422d49e75.png" />

### FluidPivotPanel
**FluidPivotPanel** is inspired from the **PivotControl** of Windows Phone 7/8/8.1.

[FluidPivotPanel Details](http://www.codeproject.com/Articles/303690/WPFSpark-of-n-FluidPivotPanel)

<img src="https://cloud.githubusercontent.com/assets/7021835/12362258/27ee4f94-bb77-11e5-96a8-1b019310fd5e.png" />

### FluidProgressBar
**FluidProgressBar** is inspired from the Indeterminate ProgressBar of Windows Phone 7/8/8.1.

[FluidProgressBar Details](http://www.codeproject.com/Articles/303694/WPFSpark-of-n-FluidProgressBar)

<img src="https://cloud.githubusercontent.com/assets/7021835/12362260/27ef7004-bb77-11e5-802f-2f64e4f9dc1d.png" />

### FluidStatusBar
**FluidStatusBar** is a custom control used to display status messages to the user. Whenever the status is updated, the previous status message slides out and fades out. At the same time, the new message fades in.

[FluidStatusBar Details](http://www.codeproject.com/Articles/303697/WPFSpark-of-n-FluidStatusBar)

<img src="https://cloud.githubusercontent.com/assets/7021835/12362257/27ecfc8e-bb77-11e5-8c26-10f0624aa72f.png" />
