using System;
using System.Collections.Generic;
using Torch;

namespace FactionRepSync
{
    public class FactionRepSyncConfig : ViewModel
    {
        private bool _enabledProperty = true;
        private int _interval = 1;
        public bool EnabledProperty { get => _enabledProperty; set => SetValue(ref _enabledProperty, value); }
        public int Interval { get => _interval; set => SetValue(ref _interval, value); }
    }
}
