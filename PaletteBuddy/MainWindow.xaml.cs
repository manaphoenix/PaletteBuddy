using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace PaletteBuddy
{
	public class ColorItem
	{
		public string Name { get; set; }
		public Color Rgb { get; set; }

		public ColorItem(string name, Color rgb)
		{
			Name = name;
			Rgb = rgb;
		}

		public ColorItem()
		{
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly CultureInfo Culture = new CultureInfo("en-US", false);
		private ObservableCollection<ColorItem> items = new ObservableCollection<ColorItem>();

		public MainWindow()
		{
			InitializeComponent();
			Load_object();
		}

		private void Save_object()
		{
			var serl = new XmlSerializer(typeof(ObservableCollection<ColorItem>));
			using var writer = XmlWriter.Create("Colors.xml");
			serl.Serialize(writer, items);
		}

		private void Load_object()
		{
			if (File.Exists("Colors.xml"))
			{
				var serl = new XmlSerializer(typeof(ObservableCollection<ColorItem>));
				using var reader = new StreamReader("Colors.xml");
				using var xmlreader = XmlReader.Create(reader);
				items = serl.Deserialize(xmlreader)
					as ObservableCollection<ColorItem>;
			}
			Refresh();
		}

		private void ItemList_Loaded(object sender, RoutedEventArgs e)
		{
			if (items.Count > 0)
			{
				var item = items.First();
				ItemList.SelectedItem = item;
				ColorPicker.SelectedColor = item.Rgb;
				ColorPicker.SecondaryColor = item.Rgb;
				ColorName.Text = item.Name;
			}
		}

		private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Load Button
			if ((sender as ListBox).SelectedItem is ColorItem item)
			{
				ColorPicker.SelectedColor = item.Rgb;
				ColorPicker.SecondaryColor = item.Rgb;
				ColorName.Text = item.Name;
			}
		}

		private void Refresh()
		{
			items = new ObservableCollection<ColorItem>(items.OrderBy(x => x.Name));
			ItemList.ItemsSource = items;
		}

		private void Button_Save(object sender, RoutedEventArgs e)
		{
			//Save Button
			if (ColorName.Text.Length == 0)
			{
				return;
			}
			if (!items.Where(x => x.Name == ColorName.Text).Any())
			{
				var item = new ColorItem(ColorName.Text, ColorPicker.SelectedColor);
				items.Add(item);
			}
			else
			{
				var result = MessageBox.Show("Do you wish to overrite?", Properties.Resources.WarningLabel, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
				if (result.Equals("No")) return;
				var item = items.Where(x => x.Name == ColorName.Text).First();
				items.Remove(item);
				item.Rgb = ColorPicker.SelectedColor;
				items.Add(item);
			}
			Refresh();
		}

		private void Button_Remove(object sender, RoutedEventArgs e)
		{
			//Remove
			var result = MessageBox.Show("This cannot be undone, are you sure?", Properties.Resources.WarningLabel, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
			if (result.Equals("No")) return;
			if (ItemList.SelectedItem is ColorItem item)
			{
				items.Remove(item);
				Refresh();
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Save_object();
		}

		private void Button_ResetColors(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("This cannot be undone, are you sure?", Properties.Resources.WarningLabel, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
			if (result.Equals("Yes")) items.Clear();
			Refresh();
		}

		private void Button_OpenFile(object sender, RoutedEventArgs e)
		{
			if (File.Exists("Colors.xml"))
			{
				var proc = Process.Start("Colors.xml");
				proc.Exited += (object s, EventArgs ev) => { Load_object(); };
			}
		}

		private void Button_Exit(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Button_SaveToFile(object sender, RoutedEventArgs e)
		{
			Save_object();
		}

		private void Search_Bar_TextChanged(object sender, TextChangedEventArgs e)
		{
			var txt = (sender as TextBox).Text;
			if (txt.Length > 0)
			{
				ItemList.ItemsSource = items.Where(x => x.Name.ToLower(Culture).Contains(txt.ToLower(Culture)));
			}
			else
			{
				Refresh();
			}
		}

		private Color KelvinToRGB(double temp)
		{
			var Temperature = temp / 100;
			double Red, Green, Blue;

			if (Temperature <= 66)
			{
				Red = 255;
			}
			else
			{
				Red = Temperature - 60;
				Red = 329.698727446 * Math.Pow(Red, -0.1332047592);
				Red = Red < 0 ? 0 : Red > 255 ? 255 : Red;
			}

			if (Temperature <= 66)
			{
				Green = Temperature;
				Green = 99.4708025861 * Math.Log(Green) - 161.1195681661;
				Green = Green < 0 ? 0 : Green > 255 ? 255 : Green;
			}
			else
			{
				Green = Temperature - 60;
				Green = 288.1221695283 * Math.Pow(Green, -0.0755148492);
				Green = Green < 0 ? 0 : Green > 255 ? 255 : Green;
			}

			if (Temperature >= 66)
			{
				Blue = 255;
			}
			else
			{
				if (Temperature <= 19)
				{
					Blue = 0;
				}
				else
				{
					Blue = Temperature - 10;
					Blue = 138.5177312231 * Math.Log(Blue) - 305.0447927307;
					Blue = Blue < 0 ? 0 : Blue > 255 ? 255 : Blue;
				}
			}

			var col = Color.FromRgb((byte)Red, (byte)Green, (byte)Blue);

			return col;
		}

		private void Kelvin_TextChanged(object sender, TextChangedEventArgs e)
		{
			var convert = double.TryParse(Kelvin.Text, out double parse);
			if (convert)
			{
				var col = KelvinToRGB(parse);
				ColorPicker.SelectedColor = col;
			}
		}
	}
}