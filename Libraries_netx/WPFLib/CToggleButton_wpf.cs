using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfLib
{
    public class CToggleButton_wpf : Button
    {
        // Dependency Properties
        public static readonly DependencyProperty TextState1Property = DependencyProperty.Register(
            nameof(TextState1), typeof(string), typeof(CToggleButton_wpf), new PropertyMetadata("State1"));

        public static readonly DependencyProperty TextState2Property = DependencyProperty.Register(
            nameof(TextState2), typeof(string), typeof(CToggleButton_wpf), new PropertyMetadata("State2"));

        public static readonly DependencyProperty ColorState1Property = DependencyProperty.Register(
            nameof(ColorState1), typeof(Brush), typeof(CToggleButton_wpf), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ColorState2Property = DependencyProperty.Register(
            nameof(ColorState2), typeof(Brush), typeof(CToggleButton_wpf), new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty AcceptChangeProperty = DependencyProperty.Register(
            nameof(AcceptChange), typeof(bool), typeof(CToggleButton_wpf), new PropertyMetadata(true));

        // CLR Properties for Dependency Properties
        public string TextState1
        {
            get => (string)GetValue(TextState1Property);
            set => SetValue(TextState1Property, value);
        }

        public string TextState2
        {
            get => (string)GetValue(TextState2Property);
            set => SetValue(TextState2Property, value);
        }

        public Brush ColorState1
        {
            get => (Brush)GetValue(ColorState1Property);
            set => SetValue(ColorState1Property, value);
        }

        public Brush ColorState2
        {
            get => (Brush)GetValue(ColorState2Property);
            set => SetValue(ColorState2Property, value);
        }

        public bool AcceptChange
        {
            get => (bool)GetValue(AcceptChangeProperty);
            set => SetValue(AcceptChangeProperty, value);
        }

        // Events
        public event EventHandler? ToState1;
        public event EventHandler? ToState2;

        // Constructor
        public CToggleButton_wpf()
        {
            Content = TextState1;
            Background = ColorState1;
            Click += CToggleButton_Click;
        }

        private void CToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between states
            AcceptChange = true;
            if (Content.ToString() == TextState2)
            {
                // State 2 -> State 1
                ToState1?.Invoke(this, EventArgs.Empty);
                if (AcceptChange)
                {
                    Content = TextState1;
                    Background = ColorState1;
                }
            }
            else
            {
                // State 1 -> State 2
                ToState2?.Invoke(this, EventArgs.Empty);
                if (AcceptChange)
                {
                    Content = TextState2;
                    Background = ColorState2;
                }
            }
        }

        // Methods to change states programmatically
        public void GoToState1(bool triggerEvent)
        {
            Content = TextState1;
            Background = ColorState1;
            if (triggerEvent)
            {
                ToState1?.Invoke(this, EventArgs.Empty);
            }
        }

        public void GoToState2(bool triggerEvent)
        {
            Content = TextState2;
            Background = ColorState2;
            if (triggerEvent)
            {
                ToState2?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
