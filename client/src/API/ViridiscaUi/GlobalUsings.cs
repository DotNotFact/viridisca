// System Namespaces
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.ComponentModel;
global using System.Linq;
global using System.Reactive;
global using System.Reactive.Disposables;
global using System.Reactive.Linq;
global using System.Threading.Tasks;

// Microsoft Extensions
global using Microsoft.Extensions.Logging;

// Third-party Libraries
global using ReactiveUI;
global using ReactiveUI.Fody.Helpers;
global using DynamicData;

// Avalonia UI
global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Data;
global using Avalonia.Markup.Xaml;
global using Avalonia.Threading;

// Domain Entities
global using ViridiscaUi.Domain.Entities.Auth;
global using ViridiscaUi.Domain.Entities.Education;
global using ViridiscaUi.Domain.Entities.Analytics;
global using ViridiscaUi.Domain.Entities.System;
global using ViridiscaUi.Domain.Entities.Library;

// Domain Enums
global using ViridiscaUi.Domain.Entities.Auth.Enums;
global using ViridiscaUi.Domain.Entities.Education.Enums;
global using ViridiscaUi.Domain.Entities.System.Enums;

// Domain Services
global using ViridiscaUi.Domain.Services;
global using ViridiscaUi.Domain.Services.Auth;
global using ViridiscaUi.Domain.Services.Education;
global using ViridiscaUi.Domain.Services.System;
global using ViridiscaUi.Domain.Services.File;
global using ViridiscaUi.Domain.Services.Notification;
global using ViridiscaUi.Domain.Services.Statistic;

// Local Services
global using ViridiscaUi.Services;

// Local Models and ViewModels
global using ViridiscaUi.Models.ViewModels;
global using ViridiscaUi.Models.DataInfo;
global using ViridiscaUi.ViewModels.Bases;

// Navigation
global using ViridiscaUi.Navigations;

// Type Aliases
global using DomainValidationResult = ViridiscaUi.Domain.Models.ValidationResult;
global using DomainNotification = ViridiscaUi.Domain.Entities.System.Notification;
global using EducationSubject = ViridiscaUi.Domain.Entities.Education.Subject;
global using RouteAttribute = ViridiscaUi.Navigations.RouteAttribute;
global using StatusLogger = ViridiscaUi.Infrastructure.Logger.StatusLogger;

// Enums
global using StatusMessageType = ViridiscaUi.Domain.Entities.System.Enums.StatusMessageType;
global using NotificationType = ViridiscaUi.Domain.Entities.System.Enums.NotificationType; 