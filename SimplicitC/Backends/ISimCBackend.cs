using System.Collections.Generic;

namespace SimplicitC.Backends
{
    interface ISimCBackend
    {

        IEnumerable<Error> Errors { get; }

        bool Compile(string code);

        int Execute();
    }
}
