using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Simplayer5 {
	[ValueConversion(typeof(String), typeof(string))]
	public class BindingConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture) {

			try {
				if (value == null || parameter == null) { return ""; }

				string param = parameter.ToString();
				switch (param) {
					case "Caption":
						return value.ToString() == "" ? "<null>" : value.ToString();
				}

				return "";
			} catch {
				return "";
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter,
					  System.Globalization.CultureInfo culture) {
						  return "";
		}
	}
}
