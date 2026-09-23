using ServiceContracts;
using Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our SignalsService
    /// </summary>
    public class SignalsServiceTest
    {

        private readonly ISignalsService _signalsService;
        private readonly ITestOutputHelper _outputHelper;
        public SignalsServiceTest(ITestOutputHelper outputHelper)
        {
            _signalsService = new SignaslService();
            _outputHelper = outputHelper;
        }
    }
}
