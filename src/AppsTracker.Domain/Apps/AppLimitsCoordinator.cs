﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export]
    public sealed class AppLimitsCoordinator
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppLimitsCoordinator(IRepository repository,
                                    ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppModel> GetApps()
        {
            return repository.GetFiltered<Aplication>(a => a.User.ID == trackingService.SelectedUserID,
                                                           a => a.Limits)
                                                        .ToList()
                                                        .Distinct()
                                                        .Select(a => new AppModel(a))
                                                        .ToList();
        }

        public async Task SaveChanges(IEnumerable<AppLimitModel> modifiedLimits,
                                      IEnumerable<AppLimitModel> newLimits,
                                      IEnumerable<AppLimitModel> deletedLimits)
        {
            await SaveModifiedLimits(modifiedLimits);

            await SaveNewLimits(newLimits);

            await DeleteLimits(deletedLimits);

        }

        private async Task DeleteLimits(IEnumerable<AppLimitModel> deletedLimits)
        {
            var deletedIds = deletedLimits.Select(l => l.ID);
            var limitsToDelete = await repository.GetFilteredAsync<AppLimit>(l => deletedIds.Contains(l.ID));
            await repository.DeleteEntityRangeAsync(limitsToDelete);
        }

        private async Task SaveNewLimits(IEnumerable<AppLimitModel> newLimits)
        {
            foreach (var limit in newLimits)
            {
                var newLimit = new AppLimit()
                {
                    ApplicationID = limit.ApplicationID,
                    Limit = limit.Limit,
                    LimitReachedAction = limit.LimitReachedAction,
                    LimitSpan = limit.LimitSpan
                };
                await repository.SaveNewEntityAsync(newLimit);
            }
        }

        private async Task SaveModifiedLimits(IEnumerable<AppLimitModel> modifiedLimits)
        {
            foreach (var limit in modifiedLimits)
            {
                var dbLimit = await repository.GetSingleAsync<AppLimit>(limit.ID);
                if (dbLimit != null)
                {
                    dbLimit.Limit = limit.Limit;
                    dbLimit.LimitReachedAction = limit.LimitReachedAction;
                    dbLimit.LimitSpan = limit.LimitSpan;
                    await repository.SaveModifiedEntityAsync(dbLimit);
                }
            }
        }
    }
}
