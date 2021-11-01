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
                new ("USD", "CAD", 1.34)
            });
            var result = await _currencyConverter.Convert("USD", "CAD", 0);
            Equal(1.34, result);
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


            });
            var expectedResult = Math.Round(0.7772, 3);
            var result = await _currencyConverter.Convert("USD", "GBP", 0);
            Equal(expectedResult, result);
        }
        [Fact]
        public async Task Test3()
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
            var expectedResult = Math.Round(0.18978139534883723, 3);
            var result = await _currencyConverter.Convert("EUR", "HIJ", 0);
            Equal(expectedResult, result);
        }
        [Fact]
        public async Task Test4()
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
                new("HIJ","LMN",0.4),
                new ("LMN","SOT",0.5)

            });
            var result = await _currencyConverter.Convert("EFG", "SOT", 0);
            Equal(0.14, result);
        }
        [Fact]
        public async Task ReturnCorrectAnswerWhenConfigIsUpdated()
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
                new("HIJ","LMN",0.4),
                new ("LMN","SOT",0.5)

            });
            var result = await _currencyConverter.Convert("EFG", "SOT", 0);
            Equal(0.14, result);
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new("HIJ","LMN",4)
            });
            var result2 = await _currencyConverter.Convert("EFG", "SOT", 0);
            Equal(1.4, result2);
        }

        [Fact]
        public async Task Test6()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("USD", "CAD", 1.34),
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),
                new ("GBP", "ABC", 0.5),
                new ("ABC", "EFG", 0.6),
                new ("EFG", "HIJ", 0.7),
                new("HIJ","LMN",0.4),
                new ("LMN","SOT",0.5)

            });
            var result = await _currencyConverter.Convert("EFG", "SOT", 0);
            Equal(0.14, result);
        }

        [Fact]
        public async Task ReturnZeroWhenNoPathExist()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("USD", "CAD", 1.34),
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),
                new ("GBP", "ABC", 0.5),
                new ("ABC", "EFG", 0.6),
                new ("EFG", "HIJ", 0.7),
               // new("HIJ","LMN",0.4),
                new ("LMN","SOT",0.5)

            });
            var result = await _currencyConverter.Convert("EFG", "SOT", 0);
            Equal(0, result);
        }
        [Fact]
        public async Task ReturnShorterPathWhenMoreThanOnePathExist()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("USD", "CAD", 1.34),
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),
                new ("GBP", "ABC", 0.5),
                new ("ABC", "EFG", 0.6),
                new ("EFG", "HIJ", 0.7),
                new("HIJ","LMN",0.4),
                new ("LMN","SOT",0.5),
                new("GBP", "SOT",0.042)

            });
            var result = await _currencyConverter.Convert("USD", "SOT", 0);
            Equal(0.033, result);
        }

        [Fact]
        public async Task ReturnZeroWhenNoConfigurationExist()
        {
            _currencyConverter.ClearConfiguration();
            
            var result = await _currencyConverter.Convert("USD", "SOT", 0);
            Equal(0, result);
        }

        [Fact]
        public async Task ReturnZeroWhenNoPathExist2()
        {
            _currencyConverter.ClearConfiguration();
            _currencyConverter.UpdateConfiguration(new List<Tuple<string, string, double>>()
            {
                new ("CAD", "GBP", 0.58),
                new ("USD", "EUR", 0.86),

            });
            var result = await _currencyConverter.Convert("CAD", "EUR", 0);
            Equal(0, result);
        }






    }
}
