using System;
using System.Globalization;
using System.Windows.Data;

namespace LatestChatty.Classes
{
	public class MultiplicationConverter : IValueConverter
	{
		public double Multiplier { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return 0;
			var v = System.Convert.ToDouble(value);
			return v * this.Multiplier;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}