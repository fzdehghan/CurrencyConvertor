using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TaskHiring.Services;
using Xunit;

namespace CurrencyConvertor.Testd
{
    public class UnitTest1
    {
        private readonly Mock<ICurrencyConverter> _currencyConvertor;

        [ExcludeFromCodeCoverage]
        public UnitTest1(Mock<ICurrencyConverter> currencyConvertor)
        {
            _currencyConvertor = currencyConvertor;
        }

        [Fact]
        public void Test1()
        {
            _currencyConvertor.Setup(x => x.ClearConfiguration());
            _currencyConvertor.Setup(x => x.UpdateConfiguration(new List<Tuple<string, string, double>>()));
            _currencyConvertor.Setup(x => (x.Convert("", "", 1).Result)).Returns(2);
            //var currencyConvertor = new TaskHiring.Services.CurrencyConverter();
            var convertorController = new TaskHiring.Controllers.ConverterController(_currencyConvertor.Object);
            var actionResult = convertorController.Get("", "");
        }
    }
}
