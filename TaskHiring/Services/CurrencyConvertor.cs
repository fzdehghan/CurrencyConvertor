using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHiring.Model;

namespace TaskHiring.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private ConcurrentDictionary<string, PathDetails> _pathValue = new ConcurrentDictionary<string, PathDetails>();
        private ConcurrentDictionary<string, PathDetails> _basePathValue = new ConcurrentDictionary<string, PathDetails>();
        private HashSet<string> _currencies = new HashSet<string>();
        private object lockingObj = new object();

        public void ClearConfiguration()
        {
            _pathValue = new ConcurrentDictionary<string, PathDetails>();
            _basePathValue = new ConcurrentDictionary<string, PathDetails>();
            _currencies = new HashSet<string>();
        }



        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {

            foreach (var item in conversionRates)
            {
                AddPathValue(item.Item1, item.Item2, item.Item3);
            }
            lock (lockingObj)
            {
                _pathValue = new ConcurrentDictionary<string, PathDetails>();
                foreach (var key in _basePathValue.Keys)
                {
                    _pathValue.TryAdd(key, _basePathValue[key]);
                }
            }
        }

        public async Task<double> Convert(string fromCurrency, string toCurrency, double amount)
        {
            var details = await FindShortestWay(fromCurrency, toCurrency);
            return details.Multiply;
        }
        public async Task<List<string>> GetShortestPath(string fromCurrency, string toCurrency)
        {
            var details = await FindShortestWay(fromCurrency, toCurrency);
            return details.SequentialPath;
        }



        private void AddPathValue(string x, string y, double multiply)
        {
            var xy = GetPairs(x, y);
            var yx = GetPairs(y, x);
            if (_basePathValue.ContainsKey(xy))
            {
                _basePathValue[xy] = new PathDetails
                {
                    Count = 1,
                    SequentialPath = new List<string> { xy },
                    Multiply = multiply
                };
                _basePathValue[yx] = new PathDetails
                {
                    Count = 1,
                    SequentialPath = new List<string> { yx },
                    Multiply = 1 / multiply
                };
            }

            if (multiply == 0)
                //todo send error
                return;
            _basePathValue.TryAdd(xy, new PathDetails
            {
                Count = 1,
                SequentialPath = new List<string> { yx },
                Multiply = multiply
            });
            _basePathValue.TryAdd(xy, new PathDetails
            {
                Count = 1,
                SequentialPath = new List<string> { yx },
                Multiply = 1 / multiply
            });
            _currencies.Add(x);
            _currencies.Add(y);
        }
        private async Task<PathDetails> FindShortestWay(string x, string y, List<string> forbiddenPath = null)
        {
            if (forbiddenPath == null || forbiddenPath.Count < 1)
                forbiddenPath = new List<string>() { x, y };
            var xy = GetPairs(x, y);
            var yx = GetPairs(y, x);

            if (_pathValue.Keys.Contains(xy))
            {
                return _pathValue[xy];
            }
            else
            {
                int? minCount = null;
                PathDetails pathDetails = new PathDetails();

                foreach (var currency in _currencies)
                {
                    if (forbiddenPath.Contains(currency))
                        continue;
                    var newForbiddenPath = new List<string> { currency };
                    newForbiddenPath.AddRange(forbiddenPath);
                    //var firstTask = FindShortestWay(x, currency, newForbiden);
                    //var secondTask = FindShortestWay(currency, y, newForbiden);
                    //await Task.WhenAll(firstTask, secondTask);
                    //var first = firstTask.Result;
                    //var second = secondTask.Result;
                    var first = await FindShortestWay(x, currency, newForbiddenPath);
                    var second = await FindShortestWay(currency, y, newForbiddenPath);
                    int newCount = first.Count + second.Count;
                    if (first.Count > 0 && second.Count > 0)
                        if (!minCount.HasValue || newCount < minCount)
                        {
                            minCount = newCount;
                            pathDetails.Count = minCount.Value;
                            pathDetails.SequentialPath = new List<string>();
                            pathDetails.SequentialPath.AddRange(first.SequentialPath);
                            pathDetails.SequentialPath.AddRange(second.SequentialPath);
                            pathDetails.Multiply = first.Multiply * second.Multiply;
                        }
                }
                _pathValue.TryAdd(xy, pathDetails);
                _pathValue.TryAdd(yx, new PathDetails()
                {
                    Count = pathDetails.Count,
                    Multiply = 1 / pathDetails.Multiply,
                    //SequentialPath = ReversePath(pathDetails.SequentialPath)
                });
                return pathDetails;
            }


        }

        private string GetPairs(string x, string y)
        {
            return x + "_" + y;
        }

        private string ReversePair(string pair)
        {
            var pairArray = pair.Split('_');
            return GetPairs(pairArray[1], pairArray[0]);
        }

        private List<string> ReversePath(List<string> sequentialPath)
        {
            if (sequentialPath == null)
                return null;
            var reversePath = new List<string>();
            var count = sequentialPath.Count;
            for (int i = 0; i < count; i++)
            {
                reversePath.Add(ReversePair(sequentialPath[count - i - 1]));
            }

            return reversePath;
        }


    }
}
