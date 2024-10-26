using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ChordSheetConverter
{
    public partial class InputWindow : Window
    {
        private readonly IChordSheetAnalyzer _targetObject;

        public InputWindow(IChordSheetAnalyzer targetObject)
        {
            InitializeComponent();
            _targetObject = targetObject;
            GenerateInputFields();
        }

        private void GenerateInputFields()
        {
            // Get the properties to be displayed from propertyMapTags (only the first values)
            var propertiesToDisplay = _targetObject.PropertyMapTags.Select(kvp => kvp.Key).ToHashSet();

            Type targetType = _targetObject.GetType();

            // Iterate over the properties of the object and filter them by the ones in propertyMapTags
            foreach (var property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Check if the property name exists in propertyMapTags
                if (propertiesToDisplay.Contains(property.Name))
                {
                    // Get the display name from propertyMapDisplayNames if it exists, otherwise use the property name
                    string displayName = _targetObject.PropertyMapDisplayNames.TryGetValue(property.Name, out string displayValue) ? displayValue : property.Name;

                    var label = new Label { Content = displayName };
                    var textBox = new TextBox
                    {
                        Name = property.Name,
                        Text = property.GetValue(_targetObject)?.ToString() ?? "",
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    InputStackPanel.Children.Add(label);
                    InputStackPanel.Children.Add(textBox);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Type targetType = _targetObject.GetType();

            foreach (var child in InputStackPanel.Children.OfType<TextBox>())
            {
                // Get the property by name
                PropertyInfo propInfo = targetType.GetProperty(child.Name);
                if (propInfo != null && propInfo.CanWrite)
                {
                    // Set the value from the textbox back into the object
                    propInfo.SetValue(_targetObject, child.Text);
                }
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
