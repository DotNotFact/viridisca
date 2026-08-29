using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ViridiscaUi.ViewModels.Components;
using ViridiscaUi.Domain.Services.Statistic;

namespace ViridiscaUi.Windows;

public partial class StatusHistoryWindow : Window
{
    private bool _isDragging;
    private Point _dragStartPoint;

    public StatusHistoryWindow()
    {
        InitializeComponent();
        DataContext = null; // DataContext будет установлен извне
        
        // Подписка на события для перетаскивания окна
        var headerArea = this.FindControl<Border>("HeaderArea");
        if (headerArea != null)
        {
            headerArea.PointerPressed += OnHeaderPointerPressed;
            headerArea.PointerMoved += OnHeaderPointerMoved;
            headerArea.PointerReleased += OnHeaderPointerReleased;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnMessageDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is StatusMessage message)
        {
            if (DataContext is StatusBarViewModel viewModel)
            {
                // Выполняем команду копирования сообщения
                viewModel.CopyMessageCommand.Execute(message).Subscribe();
            }
        }
    }

    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            e.Pointer.Capture(sender as Control);
        }
    }

    private void OnHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;

            Position = new PixelPoint(
                Position.X + (int)deltaX,
                Position.Y + (int)deltaY
            );
        }
    }

    private void OnHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }
    }

    private void OnCloseButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Центрируем окно относительно главного окна
        if (Owner != null)
        {
            var ownerBounds = Owner.Bounds;
            var centerX = ownerBounds.X + (ownerBounds.Width - Width) / 2;
            var centerY = ownerBounds.Y + (ownerBounds.Height - Height) / 2;
            
            Position = new PixelPoint((int)centerX, (int)centerY);
        }
    }
} 