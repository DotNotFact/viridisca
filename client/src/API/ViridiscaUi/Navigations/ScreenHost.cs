using ReactiveUI;
using System;

namespace ViridiscaUi.Navigations;

/// <summary>
/// Простая реализация IScreen для ReactiveUI навигации
/// </summary>
public class ScreenHost(RoutingState router) : IScreen
{
    public RoutingState Router => router ?? throw new ArgumentNullException(nameof(router));
} 