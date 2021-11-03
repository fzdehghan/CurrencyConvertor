using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskHiring.Model;
using TaskHiring.Services;

namespace TaskHiring.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConverterController : ControllerBase
    {
        private readonly ICurrencyConverter _currencyConverter;

        public ConverterController(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }

        [HttpPost]
        public IActionResult Config(List<Currency> input)
        {
            try
            {
                var newInput = input
                    .Select(x => new Tuple<string, string, double>(x.FromCurrency, x.ToCurrency, x.Amount)).ToList();
                _currencyConverter.UpdateConfiguration(newInput);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }
        [HttpDelete]
        public IActionResult Clear()
        {
            try
            {
                _currencyConverter.ClearConfiguration();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        public async Task<ActionResult<double>> Get(string fromCurrency, string toCurrency)
        {
            try
            {
                var result = await _currencyConverter.Convert(fromCurrency, toCurrency, 0);
                return Ok(result);
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }
    }
}
