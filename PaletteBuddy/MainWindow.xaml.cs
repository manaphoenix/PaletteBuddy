using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Media;
using ColorPicker.Models;

namespace PaletteBuddy
{
	public class ColorItem
	{
		public string Name { get; set; }
		public Color Rgb { get; set; }
		public string Hex { get; set; }

		public ColorItem(string name, Color rgb, string hex)
		{
			Name = name;
			Rgb = rgb;
			Hex = hex;
		}

		public ColorItem() { }
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly CultureInfo Culture = new CultureInfo("en-US", false);
		ObservableCollection<ColorItem> items = new ObservableCollection<ColorItem>();
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
				NameBox.Text = item.Name;
				RGBBox.Text = $"{item.Rgb.A},{item.Rgb.R},{item.Rgb.G},{item.Rgb.B}";
				HexBox.Text = item.Hex;
				HSVBox.Text = ColorHelper.ColorConverter.HexToHsv(new ColorHelper.HEX(item.Hex)).ToString();
				ColorPicker.SelectedColor = item.Rgb;
			}
		}

		private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Load Button
			if ((sender as ListBox).SelectedItem is ColorItem item)
			{
				NameBox.Text = item.Name;
				RGBBox.Text = $"{item.Rgb.A},{item.Rgb.R},{item.Rgb.G},{item.Rgb.B}";
				HexBox.Text = item.Hex;
				HSVBox.Text = ColorHelper.ColorConverter.HexToHsv(new ColorHelper.HEX(item.Hex)).ToString();
				ColorPicker.SelectedColor = item.Rgb;
				UpdateStatus("Loaded", item.Name);
			}
			else
			{
				UpdateStatus("Load Failed");
			}
		}

		private void UpdateStatus(string status, string obj = "")
		{
			Status.Content = $"\"{obj}\" {status}!";
		}

		private void Refresh()
		{
			items = new ObservableCollection<ColorItem>(items.OrderBy(x => x.Name));
			ItemList.ItemsSource = items;
		}

		private void Button_Save(object sender, RoutedEventArgs e)
		{
			//Save Button
			if (NameBox.Text.Length == 0)
			{
				UpdateStatus("Name cannot be empty");
				return;
			}
			if (!items.Where(x => x.Name == NameBox.Text).Any())
			{
				var item = new ColorItem(NameBox.Text, ColorPicker.SelectedColor.Value, HexBox.Text);
				items.Add(item);
				UpdateStatus("Saved", NameBox.Text);
			}
			else
			{
				var result = MessageBox.Show("Do you wish to overrite?", Properties.Resources.WarningLabel, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
				if (result.Equals("No")) return;
				var item = items.Where(x => x.Name == NameBox.Text).First();
				items.Remove(item);
				item.Rgb = ColorPicker.SelectedColor.Value;
				item.Hex = HexBox.Text;
				items.Add(item);
				UpdateStatus("Updated", NameBox.Text);
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
				UpdateStatus("Removed!", NameBox.Text);
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
			UpdateStatus("Saved!", NameBox.Text);
		}

		private void RGBBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.Equals(Key.Enter))
			{
				var txt = RGBBox.Text;
				var mt = Regex.Match(txt, "(\\d+),(\\d+),(\\d+),(\\d+)");
				if (mt.Success)
				{
					var c = new Color();
					var g = mt.Groups;
					var A = byte.Parse(g[1].Value, Culture);
					var R = byte.Parse(g[2].Value, Culture);
					var G = byte.Parse(g[3].Value, Culture);
					var B = byte.Parse(g[4].Value, Culture);
					c.A = A;
					c.R = R;
					c.G = G;
					c.B = B;
					ColorPicker.SelectedColor = c;
				}
			}
		}

		private void HexBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.Equals(Key.Enter))
			{
				var txt = HexBox.Text;
				if (txt.Length != 9) return;
				var mt = Regex.Match(txt, "#.{8,8}");
				if (mt.Success)
				{
					var c = (Color)ColorConverter.ConvertFromString(txt);
					if (c != null)
					{
						ColorPicker.SelectedColor = c;
					}
				}
			}
		}

		private void RGBBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var col = ColorPicker.SelectedColor.Value;
			RGBBox.Text = $"{col.A},{col.R},{col.G},{col.B}";
		}

		private void HexBox_LostFocus(object sender, RoutedEventArgs e)
		{
			HexBox.Text = ColorPicker.SelectedColor.ToString();
		}

		private void Search_Bar_TextChanged(object sender, TextChangedEventArgs e)
		{
			var txt = (sender as TextBox).Text;
			if (txt.Length > 0)
			{
				ItemList.ItemsSource = items.Where(x => x.Name.ToLower(Culture).Contains(txt.ToLower(Culture)));
			} else
			{
				Refresh();
			}
		}

		private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			var val = e.NewValue.Value;
			Preview.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(val.ToString(Culture));
			HexBox.Text = val.ToString(Culture);
			RGBBox.Text = $"{val.A},{val.R},{val.G},{val.B}";
			HSVBox.Text = ColorHelper.ColorConverter.HexToHsv(new ColorHelper.HEX(HexBox.Text)).ToString();

		}
	}
}
