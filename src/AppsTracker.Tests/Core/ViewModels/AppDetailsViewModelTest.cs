﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class AppDetailsViewModelTest : TestMockBase
    {
        [TestInitialize]
        public void Init()
        {
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>()))
                .ReturnsAsync(new List<Aplication>());
        }

        [TestMethod]
        public void TestGetApps()
        {
            var viewModel = new AppDetailsViewModel(dataService.Object,
                                                    trackingService.Object,
                                                    mediator);

            var apps = viewModel.AppList.Result;
            while (viewModel.Working)
            {
            }

            Assert.IsInstanceOfType(viewModel.AppList.Result, typeof(IEnumerable<Aplication>));
            dataService.Verify(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>()));
        }
    }
}
