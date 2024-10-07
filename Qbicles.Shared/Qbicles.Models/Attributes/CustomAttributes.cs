using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Qbicles.Models.Attributes
{
	//public class EncryptedAttribute : Attribute { }
    public class DecimalPrecisionAttribute : Attribute
    {
        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public DecimalPrecisionAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }
    }
	public class DecimalPrecisionAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DecimalPrecisionAttribute>
    {
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DecimalPrecisionAttribute attribute)
        {
            configuration.HasPrecision(attribute.Precision, attribute.Scale);
        }
    }
}
