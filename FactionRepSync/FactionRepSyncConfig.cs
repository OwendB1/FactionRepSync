using System;
using System.Collections.Generic;
using Torch;

namespace FactionRepSync
{
    public class FactionRepSyncConfig : ViewModel
    {
        private bool _enabledProperty = true;

        public bool EnabledProperty { get => _enabledProperty; set => SetValue(ref _enabledProperty, value); }
    }
}
