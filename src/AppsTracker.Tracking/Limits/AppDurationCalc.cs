﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;

namespace AppsTracker.Tracking.Limits
{
    [Export(typeof(IAppDurationCalc))]
    public class AppDurationCalc : IAppDurationCalc
    {
        private readonly IDataService dataService;

        [ImportingConstructor]
        public AppDurationCalc(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public async Task<long> GetDuration(Aplication app, LimitSpan span)
        {
            switch (span)
            {
                case LimitSpan.Day:
                    var dayBegin = DateTime.Now.Date;
                    return await SumDuration(app, l => l.DateCreated >= dayBegin);
                case LimitSpan.Week:
                    var tuple = DateTime.Today.GetWeekBeginAndEnd();
                    var weekBegin = tuple.Item1;
                    var weekEnd = tuple.Item2;
                    return await SumDuration(app, l => l.DateCreated >= weekBegin && l.DateCreated <= weekEnd);
                default:
                    throw new ArgumentOutOfRangeException("span");
            }
        }

        private async Task<long> SumDuration(Aplication app, Expression<Func<Log, bool>> filter)
        {
            var now = DateTime.Now.Date.AddDays(2);
            var appsList = await dataService.GetFilteredAsync<Aplication>(a => a.ApplicationID == app.ApplicationID
                                                                           && a.Windows.SelectMany(w => w.Logs)
                                                                                       .AsQueryable().Where(filter).Any(),
                                                                           a => a.Windows,
                                                                           a => a.Windows.Select(w => w.Logs));

            Console.WriteLine(appsList.ToString());

            return appsList.SelectMany(a => a.Windows)
                           .SelectMany(w => w.Logs)
                           .Where(filter.Compile())
                           .Sum(l => l.Duration);
        }
    }
}
