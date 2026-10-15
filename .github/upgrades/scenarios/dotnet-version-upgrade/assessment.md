# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
  - [Binding Redirect Configuration](#binding-redirect-configuration)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [DnDesktop.csproj](#dndesktopcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 1 | All require upgrade |
| Total NuGet Packages | 0 | All compatible |
| Total Code Files | 9 |  |
| Total Code Files with Incidents | 9 |  |
| Total Lines of Code | 612 |  |
| Total Number of Issues | 141 |  |
| Estimated LOC to modify | 139+ | at least 22.7% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Binding Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| [DnDesktop.csproj](#dndesktopcsproj) | net48 | 🟡 Medium | 0 | 139 | 0 | 139+ | ClassicWpf, Sdk Style = False |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 0 | 0.0% |
| ⚠️ Incompatible | 0 | 0.0% |
| 🔄 Upgrade Recommended | 0 | 0.0% |
| ***Total NuGet Packages*** | ***0*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 128 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 9 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 539 |  |
| ***Total APIs Analyzed*** | ***678*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |
| WPF (Windows Presentation Foundation) | 39 | 28.1% | WPF APIs for building Windows desktop applications with XAML-based UI that are available in .NET on Windows. WPF provides rich desktop UI capabilities with data binding and styling. Enable Windows Desktop support: Option 1 (Recommended): Target net9.0-windows; Option 2: Add <UseWindowsDesktop>true</UseWindowsDesktop>. |
| Legacy Configuration System | 2 | 1.4% | Legacy XML-based configuration system (app.config/web.config) that has been replaced by a more flexible configuration model in .NET Core. The old system was rigid and XML-based. Migrate to Microsoft.Extensions.Configuration with JSON/environment variables; use System.Configuration.ConfigurationManager NuGet package as interim bridge if needed. |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Windows.RoutedEventHandler | 16 | 11.5% | Binary Incompatible |
| T:System.Windows.MessageBoxResult | 11 | 7.9% | Binary Incompatible |
| T:System.Windows.RoutedEventArgs | 10 | 7.2% | Binary Incompatible |
| T:System.Windows.MessageBox | 9 | 6.5% | Binary Incompatible |
| E:System.Windows.Controls.Primitives.ButtonBase.Click | 8 | 5.8% | Binary Incompatible |
| T:System.Windows.MessageBoxImage | 6 | 4.3% | Binary Incompatible |
| T:System.Windows.MessageBoxButton | 6 | 4.3% | Binary Incompatible |
| M:System.Windows.MessageBox.Show(System.String) | 6 | 4.3% | Binary Incompatible |
| T:System.Uri | 5 | 3.6% | Behavioral Change |
| T:System.Windows.Application | 4 | 2.9% | Binary Incompatible |
| M:System.Uri.#ctor(System.String,System.UriKind) | 4 | 2.9% | Behavioral Change |
| M:System.Windows.Controls.Page.#ctor | 4 | 2.9% | Binary Incompatible |
| T:System.Windows.Controls.Frame | 4 | 2.9% | Binary Incompatible |
| T:System.Windows.Controls.ListView | 3 | 2.2% | Binary Incompatible |
| M:System.Windows.Application.LoadComponent(System.Object,System.Uri) | 3 | 2.2% | Binary Incompatible |
| T:System.Windows.Navigation.NavigationService | 3 | 2.2% | Binary Incompatible |
| P:System.Windows.Controls.Page.NavigationService | 3 | 2.2% | Binary Incompatible |
| M:System.Windows.Navigation.NavigationService.Navigate(System.Object) | 3 | 2.2% | Binary Incompatible |
| M:System.Windows.MessageBox.Show(System.String,System.String,System.Windows.MessageBoxButton,System.Windows.MessageBoxImage) | 3 | 2.2% | Binary Incompatible |
| T:System.Windows.Markup.IComponentConnector | 3 | 2.2% | Binary Incompatible |
| T:System.Windows.Input.MouseButtonEventHandler | 2 | 1.4% | Binary Incompatible |
| F:System.Windows.MessageBoxImage.Information | 2 | 1.4% | Binary Incompatible |
| F:System.Windows.MessageBoxButton.OK | 2 | 1.4% | Binary Incompatible |
| P:System.Windows.FrameworkElement.DataContext | 2 | 1.4% | Binary Incompatible |
| T:System.Windows.Controls.Page | 2 | 1.4% | Binary Incompatible |
| M:System.Windows.Controls.Frame.Navigate(System.Object) | 2 | 1.4% | Binary Incompatible |
| M:System.Windows.Window.#ctor | 2 | 1.4% | Binary Incompatible |
| M:System.Configuration.ApplicationSettingsBase.#ctor | 1 | 0.7% | Source Incompatible |
| T:System.Configuration.ApplicationSettingsBase | 1 | 0.7% | Source Incompatible |
| E:System.Windows.Controls.Control.MouseDoubleClick | 1 | 0.7% | Binary Incompatible |
| T:System.Windows.Input.MouseButtonEventArgs | 1 | 0.7% | Binary Incompatible |
| F:System.Windows.MessageBoxResult.Yes | 1 | 0.7% | Binary Incompatible |
| F:System.Windows.MessageBoxImage.Warning | 1 | 0.7% | Binary Incompatible |
| F:System.Windows.MessageBoxButton.YesNo | 1 | 0.7% | Binary Incompatible |
| M:System.Windows.Application.Run | 1 | 0.7% | Binary Incompatible |
| P:System.Windows.Application.StartupUri | 1 | 0.7% | Binary Incompatible |
| M:System.Windows.Application.#ctor | 1 | 0.7% | Binary Incompatible |
| T:System.Windows.Window | 1 | 0.7% | Binary Incompatible |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>⚙️&nbsp;DnDesktop.csproj</b><br/><small>net48</small>"]
    click P1 "#dndesktopcsproj"

```

## Project Details

<a id="dndesktopcsproj"></a>
### DnDesktop.csproj

#### Project Info

- **Current Target Framework:** net48
- **Proposed Target Framework:** net10.0-windows
- **SDK-style**: False
- **Project Kind:** ClassicWpf
- **Dependencies**: 0
- **Dependants**: 0
- **Number of Files**: 10
- **Number of Files with Incidents**: 9
- **Lines of Code**: 612
- **Estimated LOC to modify**: 139+ (at least 22.7% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["DnDesktop.csproj"]
        MAIN["<b>⚙️&nbsp;DnDesktop.csproj</b><br/><small>net48</small>"]
        click MAIN "#dndesktopcsproj"
    end

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 128 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 9 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 539 |  |
| ***Total APIs Analyzed*** | ***678*** |  |

#### Project Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |
| Legacy Configuration System | 2 | 1.4% | Legacy XML-based configuration system (app.config/web.config) that has been replaced by a more flexible configuration model in .NET Core. The old system was rigid and XML-based. Migrate to Microsoft.Extensions.Configuration with JSON/environment variables; use System.Configuration.ConfigurationManager NuGet package as interim bridge if needed. |
| WPF (Windows Presentation Foundation) | 39 | 28.1% | WPF APIs for building Windows desktop applications with XAML-based UI that are available in .NET on Windows. WPF provides rich desktop UI capabilities with data binding and styling. Enable Windows Desktop support: Option 1 (Recommended): Target net9.0-windows; Option 2: Add <UseWindowsDesktop>true</UseWindowsDesktop>. |

