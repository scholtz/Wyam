﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IExecutionContext
    {
        IEngine Engine { get; }
        IPipeline Pipeline { get; }
        string RootFolder { get; }
        string InputFolder { get; }
        string OutputFolder { get; }
        ITrace Trace { get; }
        IReadOnlyDictionary<string, IReadOnlyList<IDocument>> Documents { get; }

        // This executes the specified modules on the specified input contexts and returns the final result contexts
        // If you pass in null for inputDocuments, a new input context with the initial metadata from the engine will be used
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputDocuments);
    }
}