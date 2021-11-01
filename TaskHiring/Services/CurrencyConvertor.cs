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
        private object _lockingObj = new object();
        private object _lockingBaseObj = new object();
        private bool _updating = false;
        private DateTime _lastUpdatingConfiguration;

        public void ClearConfiguration()
        {
            lock (_lockingBaseObj)
            {
                _updating = true;
                _lastUpdatingConfiguration = DateTime.Now;
                lock (_lockingObj)
                {
                    _pathValue = new ConcurrentDictionary<string, PathDetails>();
                    _basePathValue = new ConcurrentDictionary<string, PathDetails>();
                    _currencies = new HashSet<string>();
                }
                _updating = false;
            }
        }
        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            lock (_lockingBaseObj)
            {
                _updating = true;
                _lastUpdatingConfiguration = DateTime.Now;

                foreach (var item in conversionRates)
                {
                    AddPathValue(item.Item1, item.Item2, item.Item3);
                }
                lock (_lockingObj)
                {
                    _pathValue = new ConcurrentDictionary<string, PathDetails>();
                    foreach (var key in _basePathValue.Keys)
                    {
                        _pathValue.TryAdd(key, _basePathValue[key]);
                    }
                }
            }
        }

        public async Task<double> Convert(string fromCurrency, string toCurrency, double amount)
        {
            var localPathDetails = new ConcurrentDictionary<string, PathDetails>();
            lock (_lockingObj)
            {
                foreach (var key in _pathValue.Keys)
                {
                    localPathDetails.TryAdd(key, _pathValue[key]);
                }
            }
            var details = await FindShortestWay(fromCurrency, toCurrency, true, localPathDetails);
            return Math.Round(details.Multiply, 3);
        }
        public List<string> GetShortestPath(string fromCurrency, string toCurrency)
        {
            var xy = GetPairs(fromCurrency, toCurrency);
            return _pathValue[xy]?.SequentialPath;

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
                SequentialPath = new List<string> { xy },
                Multiply = multiply
            });
            _basePathValue.TryAdd(yx, new PathDetails
            {
                Count = 1,
                SequentialPath = new List<string> { yx },
                Multiply = 1 / multiply
            });
            _currencies.Add(x);
            _currencies.Add(y);
        }
        private async Task<PathDetails> FindShortestWay(string x, string y, bool isMainMethod,
                                                        ConcurrentDictionary<string, PathDetails> localPathDetails,
                                                        List<string> forbiddenPath = null,
                                                        HashSet<string> improperPath = null,
                                                        DateTime? startingDateTime = null)
        {
            if (!startingDateTime.HasValue)
                startingDateTime = DateTime.Now;
            if (forbiddenPath == null || forbiddenPath.Count < 1)
                forbiddenPath = new List<string>() { x, y };
            if (improperPath == null || improperPath.Count < 1)
                improperPath = new HashSet<string>();
            var xy = GetPairs(x, y);
            var yx = GetPairs(y, x);

            if (localPathDetails.Keys.Contains(xy))
            {
                return localPathDetails[xy];
            }
            PathDetails pathDetails = new PathDetails();
            if (improperPath.Contains(xy))
                return pathDetails;

            int? minCount = null;
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
                var first = await FindShortestWay(x, currency, false, localPathDetails, newForbiddenPath, improperPath);
                var second = await FindShortestWay(currency, y, false, localPathDetails, newForbiddenPath, improperPath);
                CheckImproperPath(improperPath, first, x, currency);
                CheckImproperPath(improperPath, second, currency, y);

                int newCount = first.Count + second.Count;
                if (first.Count > 0 && second.Count > 0)
                {
                    if (!minCount.HasValue || newCount < minCount)
                    {
                        minCount = newCount;
                        pathDetails.Count = minCount.Value;
                        pathDetails.SequentialPath = new List<string>();
                        pathDetails.SequentialPath.AddRange(first.SequentialPath);
                        pathDetails.SequentialPath.AddRange(second.SequentialPath);
                        pathDetails.Multiply = first.Multiply * second.Multiply;
                        if (localPathDetails.ContainsKey(xy))
                        {
                            localPathDetails[xy] = pathDetails;
                            localPathDetails[yx] = new PathDetails()
                            {
                                Count = pathDetails.Count,
                                Multiply = 1 / pathDetails.Multiply,
                                SequentialPath = ReversePath(pathDetails.SequentialPath)
                            };
                        }
                        else
                        {
                            localPathDetails.TryAdd(xy, pathDetails);
                            localPathDetails.TryAdd(yx, new PathDetails()
                            {
                                Count = pathDetails.Count,
                                Multiply = 1 / pathDetails.Multiply,
                                SequentialPath = ReversePath(pathDetails.SequentialPath)
                            });
                        }
                        //return pathDetails;
                    }
                }

            }

            if (!localPathDetails.ContainsKey(xy))
            {
                improperPath.Add(xy);
                improperPath.Add(yx);
                return pathDetails;
            }



            if (isMainMethod)
            {
                if (startingDateTime > _lastUpdatingConfiguration)
                {
                    var minimalPathValue = GetMinimalPathValue(xy, pathDetails, localPathDetails);
                    lock(_lockingObj)
                    {
                        if (startingDateTime > _lastUpdatingConfiguration)
                            foreach (var key in minimalPathValue.Keys)
                            {
                                try
                                {
                                    _pathValue[key] = minimalPathValue[key];
                                }
                                catch (Exception)
                                {

                                    //throw;
                                }
                            }

                    }
                    //calculate all path in xy and save in a dictionary
                    //check starting datetime 
                    //lock lockingobj and update pathvalue
                }
                return pathDetails;
            }
            return pathDetails;
        }

        private void CheckImproperPath(HashSet<string> improperPath, PathDetails first, string currency1, string currency2)
        {
            if (first.Count == 0)
            {
                improperPath.Add(GetPairs(currency1, currency2));
                improperPath.Add(GetPairs(currency2, currency1));
            }
        }

        private ConcurrentDictionary<string,PathDetails> GetMinimalPathValue(string path, PathDetails pathDetails, ConcurrentDictionary<string, PathDetails> localPathValue)
        {
            var minimalPathValue = new ConcurrentDictionary<string, PathDetails>();
            if (localPathValue.ContainsKey(path))
                if (localPathValue[path].Count > pathDetails.Count)
                    localPathValue[path] = pathDetails;
            string[] currenciesInSequentialPath = GetCurrenciesInSequentialPath(pathDetails.SequentialPath);
            var length = currenciesInSequentialPath.Length;
            for (int i = 0; i < length; i++)
            {
                for (int j = i+1; j < length; j++)
                {
                    var pair = GetPairs(currenciesInSequentialPath[i], currenciesInSequentialPath[j]);
                    if (localPathValue.ContainsKey(pair))
                        minimalPathValue.TryAdd(pair, localPathValue[pair]);
                    else
                    {

                    }
                    var reversePair = GetPairs(currenciesInSequentialPath[j], currenciesInSequentialPath[i]);
                    if (localPathValue.ContainsKey(pair))
                        minimalPathValue.TryAdd(pair, localPathValue[pair]);
                    else
                    {

                    }
                }
            }
            return minimalPathValue;
        }

        private string[] GetCurrenciesInSequentialPath(List<string> sequentialPath)
        {
            var currencies = new HashSet<string>();
            if(sequentialPath!=null&&sequentialPath.Count>0)
                foreach (var item in sequentialPath)
                {
                    var pairCurrencies = item.Split('_');
                    currencies.Add(pairCurrencies[0]);
                    currencies.Add(pairCurrencies[1]);
                }
            String[] stringArray = new String[currencies.Count];
            currencies.CopyTo(stringArray);
            return stringArray;
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
