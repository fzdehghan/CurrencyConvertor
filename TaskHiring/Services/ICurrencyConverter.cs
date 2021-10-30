using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskHiring.Services
{
    public interface ICurrencyConverter
    {
        /// <summary>
        /// Clears any prior configuration.
        /// </summary>
        void ClearConfiguration();
        /// <summary>
        /// Updates the configuration. Rates are inserted or replaced internally.
        /// </summary>
        void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates);

        /// <summary>
        /// Converts the specified amount to the desired currency.
        /// </summary>
        Task<double> Convert(string fromCurrency, string toCurrency, double amount);

        /////////
        /// my methods
        /// 
        List<string> GetShortestPath(string fromCurrency, string toCurrency);

    }
}
