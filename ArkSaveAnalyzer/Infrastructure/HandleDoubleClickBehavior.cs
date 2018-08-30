using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArkSaveAnalyzer.Infrastructure {
    public sealed class HandleDoubleClickBehavior {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command", typeof(ICommand), typeof(HandleDoubleClickBehavior),
            new PropertyMetadata(default(ICommand), onComandChanged));

        public static void SetCommand(DependencyObject element, ICommand value) {
            element.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject element) {
            return (ICommand) element.GetValue(CommandProperty);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter", typeof(object), typeof(HandleDoubleClickBehavior),
            new PropertyMetadata(default(object)));

        public static void SetCommandParameter(DependencyObject element, object value) {
            element.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject element) {
            return (object) element.GetValue(CommandParameterProperty);
        }

        private static void onComandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is Control c))
                throw new InvalidOperationException($"can only be attached to {nameof(Control)}");
            c.MouseDoubleClick -= onDoubleClick;
            if (GetCommand(c) != null)
                c.MouseDoubleClick += onDoubleClick;
        }

        private static void onDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!(sender is DependencyObject d))
                return;
            ICommand command = GetCommand(d);
            if (command == null)
                return;
            object parameter = GetCommandParameter(d);
            if (!command.CanExecute(parameter))
                return;
            e.Handled = true;
            command.Execute(parameter);
        }
    }
}
