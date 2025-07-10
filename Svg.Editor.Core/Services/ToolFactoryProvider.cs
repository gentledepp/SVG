using System;
using System.Collections.Generic;
using Svg.Editor.Tools;

namespace Svg.Editor.Services
{
    public class ToolFactoryProvider
    {
        public IEnumerable<Func<ITool>> ToolFactories { get; }

        public ToolFactoryProvider(IEnumerable<Func<ITool>> toolFactories)
        {
            ToolFactories = toolFactories;
        }
    }
}