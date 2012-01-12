﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Data;

namespace LatestChatty.Classes
{
	public class BooleanToStringConverter : BooleanToValueConverter<string> {}
	public class BooleanToNewColorConverter : BooleanToValueConverter<Brush> { }
	public class BooleanToVisibilityConverter : BooleanToValueConverter<Visibility> { }

	public class BooleanToValueConverter<T> : IValueConverter
	{
		public T FalseValue { get; set; }
		//But not the hardware store... har har.
		public T TrueValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) { return this.FalseValue; }
			return (bool)value ? this.TrueValue : this.FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null ? value.Equals(this.TrueValue) : false;
		}
	}
}
