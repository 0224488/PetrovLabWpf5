using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PetrovLabWpf5
{
    // Логика взаимодействия для MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // обработка выбора названия шрифта
        private void ComboBoxFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fontName = ((sender as ComboBox).SelectedItem as TextBlock).Text;
            if (textbox != null) 
            {
                textbox.FontFamily = new FontFamily(fontName);
            }
        }

        // обработка выбора размера шрифта
        private void ComboBoxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fontSize = ((sender as ComboBox).SelectedItem as TextBlock).Text;
            if(textbox != null)
            {
                textbox.FontSize = int.Parse(fontSize);
            }
        }

        // обработка выбора цвета текста
        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e)
		{
            if(textbox != null)
            {
                var radioButton = sender as RadioButton;
                if(radioButton != null)
                {
                    textbox.Foreground = radioButton.Foreground;
                }
            }
        }

        // обработка выбора стиля шрифта (жирный, обычный)
        private void ToggleButtonBold_CheckedChanged(object sender, RoutedEventArgs e)
		{
            var toggleButton = sender as ToggleButton;
			if(toggleButton != null)
			{
				if(toggleButton.IsChecked == true)
					textbox.FontWeight = FontWeights.Bold;
                else
                    textbox.FontWeight = FontWeights.Normal;
            }
        }

        // обработка выбора стиля шрифта (курсив, обычный)
        private void ToggleButtonItalic_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.FontStyle = FontStyles.Italic;
                else
                    textbox.FontStyle = FontStyles.Normal;
            }
        }

        // обработка выбора стиля шрифта (подчеркнутый, обычный)
        private void ToggleButtonUnderline_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.TextDecorations = TextDecorations.Underline;
                else
                    textbox.TextDecorations = null;
            }
        }

        // обработка пункта меню Открыть
        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
			if(dlg.ShowDialog() == true)
			{
                // читаем документ из выбранного файла
                var jsonService = new JsonFileService();
                var document = jsonService.Open(dlg.FileName);

                InitializeDocument(document);
            }
        }

        // обработка пункта меню Сохранить
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                // создаем документ
                var document = new Document()
                {
					Text = textbox.Text,
					FontName = textbox.FontFamily.Source,
					FontSize = textbox.FontSize,
					IsBold = textbox.FontWeight == FontWeights.Bold,
                    IsItalic = textbox.FontStyle == FontStyles.Italic,
                    IsUnderline = textbox.TextDecorations == TextDecorations.Underline,
					Color = (textbox.Foreground as SolidColorBrush).Color
				};
                // сериализуем его в файл
                var jsonService = new JsonFileService();
                jsonService.Save(saveFileDialog.FileName, document);
            }
        }

        // обработка пункта меню Закрыть
        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Сохранить изменения в документ?", "Сохранение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.OK)
			{
                MenuItemSave_Click(null, null);
			}
            ResetValues();
        }

        // выставление свойств документа
        private void InitializeDocument(Document document)
		{
            textbox.Text = document.Text;
			comboBoxFontName.SelectedItem = comboBoxFontName.Items.OfType<TextBlock>()
                .FirstOrDefault(t => t.Text.Equals(document.FontName.ToString(), StringComparison.InvariantCultureIgnoreCase));
            comboBoxFontSize.SelectedItem = comboBoxFontSize.Items.OfType<TextBlock>()
                .FirstOrDefault(t => t.Text == document.FontSize.ToString());
            toggleButtonBold.IsChecked = document.IsBold;
            toggleButtonItalic.IsChecked = document.IsItalic;
            toggleButtonUnderline.IsChecked = document.IsUnderline;
            if(document.Color == Colors.Black)
            {
                radioButtonBlack.IsChecked = true;
            }
            else if(document.Color == Colors.Red)
            {
                radioButtonRed.IsChecked = true;
            }
        }

        // выставление исходных свойств нового документа
        private void ResetValues()
		{
            textbox.Text = string.Empty;
            comboBoxFontName.SelectedIndex = 0;
            comboBoxFontSize.SelectedIndex = 3;
            toggleButtonBold.IsChecked = false;
            toggleButtonItalic.IsChecked = false;
            toggleButtonUnderline.IsChecked = false;
            radioButtonBlack.IsChecked = true;
        }

        public class JsonFileService
        {
            // чтение документа из файла с указанным именем
            public Document Open(string fileName)
            {
                Document document = null;
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                
                using(StreamReader reader = new StreamReader(fileName))
                {
					var jsonString = reader.ReadToEnd();
					document = JsonSerializer.Deserialize(jsonString, typeof(Document), options) as Document;
				}
                return document;
            }

            // запись документа в файл с указанным именем
            public void Save(string fileName, Document document)
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize(document, options);
                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(jsonString);
                }
            }
        }

        public class Document
        {
            public string Text { get; set; }
            public double FontSize{ get; set; }
            public string FontName { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
            public bool IsUnderline { get; set; }
            public Color Color { get; set; }
        }
    }
}
