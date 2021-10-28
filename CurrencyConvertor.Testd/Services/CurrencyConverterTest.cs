using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHiring.Services;
using Xunit;
using static Xunit.Assert;

namespace CurrencyConvertor.Testd.Services

{
    public class CurrencyConverterTest
    {
        private TaskHiring.Services.CurrencyConverter _currencyConverter;

        public CurrencyConverterTest() => _currencyConverter = new CurrencyConverter();

        [Fact]
        public async Task Test1()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("USD", "CAD", 1.34),
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),
                new ("ABC", "GBP", 2),
                new ("ABC", "EFG", 0.6),
                new ("EFG", "HIJ", 0.7),

            });
            var result = await _currencyConverter.Convert("EUR", "HIJ", 0);
            Equal(0.18978139534883723, result);
        }
        [Fact]
        public async Task Test2()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("USD", "CAD", 1.34),
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),
                new ("ABC", "GBP", 2),
                new ("ABC", "EFG", 0.6),
                new ("EFG", "HIJ", 0.7),

            });
            var result = await _currencyConverter.Convert("EUR", "HIJ", 0);
            Equal(0.18978139534883723, result);
        }






    }
}
