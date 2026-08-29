using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Components;

namespace ViridiscaUi.Views.Components;

/// <summary>
/// StatusBar компонент для отображения статус-сообщений с перетаскиваемым окном истории
/// </summary>
public partial class StatusBarView : ReactiveUserControl<StatusBarViewModel>
{
    private Border? _historyDialog;
    private Border? _dragHandle;
    private Button? _minimizeButton;
    private ScrollViewer? _messagesScrollViewer;
    private Canvas? _historyCanvas;
    private bool _isDragging;
    private Point _lastPointerPosition;
    private bool _isMinimized;

    public StatusBarView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Получаем ссылки на элементы управления
        _historyDialog = this.FindControl<Border>("HistoryDialog");
        _dragHandle = this.FindControl<Border>("DragHandle");
        _minimizeButton = this.FindControl<Button>("MinimizeButton");
        _messagesScrollViewer = this.FindControl<ScrollViewer>("MessagesScrollViewer");
        _historyCanvas = this.FindControl<Canvas>("HistoryCanvas");

        if (_dragHandle != null)
        {
            _dragHandle.PointerPressed += OnDragHandlePointerPressed;
            _dragHandle.PointerMoved += OnDragHandlePointerMoved;
            _dragHandle.PointerReleased += OnDragHandlePointerReleased;
        }

        if (_minimizeButton != null)
        {
            _minimizeButton.Click += OnMinimizeButtonClick;
        }

        // Подписываемся на изменения размера Canvas контейнера
        if (_historyCanvas != null)
        {
            _historyCanvas.SizeChanged += OnHistoryCanvasSizeChanged;
        }
    }

    private void OnDragHandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_dragHandle).Properties.IsLeftButtonPressed && _dragHandle != null)
        {
            _isDragging = true;
            _lastPointerPosition = e.GetPosition(_historyCanvas);
            _dragHandle.Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }
    }

    private void OnDragHandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && _historyDialog != null && _historyCanvas != null)
        {
            var currentPosition = e.GetPosition(_historyCanvas);
            var deltaX = currentPosition.X - _lastPointerPosition.X;
            var deltaY = currentPosition.Y - _lastPointerPosition.Y;

            var currentLeft = Canvas.GetLeft(_historyDialog);
            var currentTop = Canvas.GetTop(_historyDialog);

            // Если позиция не установлена, используем начальную
            if (double.IsNaN(currentLeft)) currentLeft = 50;
            if (double.IsNaN(currentTop)) currentTop = 50;

            var newLeft = currentLeft + deltaX;
            var newTop = currentTop + deltaY;

            // Ограничиваем перемещение границами Canvas
            var maxLeft = _historyCanvas.Bounds.Width - _historyDialog.Width;
            var maxTop = _historyCanvas.Bounds.Height - _historyDialog.Height;

            newLeft = Math.Max(0, Math.Min(newLeft, Math.Max(0, maxLeft)));
            newTop = Math.Max(0, Math.Min(newTop, Math.Max(0, maxTop)));

            Canvas.SetLeft(_historyDialog, newLeft);
            Canvas.SetTop(_historyDialog, newTop);

            _lastPointerPosition = currentPosition;
            e.Handled = true;
        }
    }

    private void OnDragHandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _dragHandle != null)
        {
            _isDragging = false;
            _dragHandle.Cursor = new Cursor(StandardCursorType.Hand);
            e.Handled = true;
        }
    }

    private void OnMinimizeButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_messagesScrollViewer != null && _historyDialog != null)
        {
            _isMinimized = !_isMinimized;
            
            if (_isMinimized)
            {
                // Сворачиваем окно
                _messagesScrollViewer.IsVisible = false;
                _historyDialog.Height = 60; // Только заголовок
                if (_minimizeButton != null)
                {
                    ((TextBlock)_minimizeButton.Content!).Text = "□";
                    ToolTip.SetTip(_minimizeButton, "Развернуть");
                }
            }
            else
            {
                // Разворачиваем окно
                _messagesScrollViewer.IsVisible = true;
                _historyDialog.Height = 450;
                if (_minimizeButton != null)
                {
                    ((TextBlock)_minimizeButton.Content!).Text = "−";
                    ToolTip.SetTip(_minimizeButton, "Свернуть");
                }
            }
        }
    }

    private void OnHistoryCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Проверяем, что диалог не выходит за новые границы Canvas
        if (_historyDialog != null && _historyDialog.IsVisible)
        {
            var currentLeft = Canvas.GetLeft(_historyDialog);
            var currentTop = Canvas.GetTop(_historyDialog);

            // Если позиция не установлена, используем начальную
            if (double.IsNaN(currentLeft)) currentLeft = 50;
            if (double.IsNaN(currentTop)) currentTop = 50;

            var maxLeft = e.NewSize.Width - _historyDialog.Width;
            var maxTop = e.NewSize.Height - _historyDialog.Height;

            if (currentLeft > maxLeft || currentTop > maxTop)
            {
                var newLeft = Math.Max(0, Math.Min(currentLeft, Math.Max(0, maxLeft)));
                var newTop = Math.Max(0, Math.Min(currentTop, Math.Max(0, maxTop)));

                Canvas.SetLeft(_historyDialog, newLeft);
                Canvas.SetTop(_historyDialog, newTop);
            }
        }
    }
} 