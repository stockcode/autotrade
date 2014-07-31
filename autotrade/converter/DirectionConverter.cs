using System;
using System.ComponentModel;
using System.Globalization;
using autotrade.Properties;
using QuantBox.CSharp2CTP;

namespace autotrade.converter
{
    public class DirectionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(TThostFtdcDirectionType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {

            var direction = (TThostFtdcDirectionType)value;
            int index = (int)direction - (int)'0';

            return Settings.Default.Direction[index];
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }
    }
}