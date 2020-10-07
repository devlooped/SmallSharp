using System;

namespace SmallSharp.Build
{
    class DisposableAction : IDisposable
    {
        readonly Action action;

        public DisposableAction(Action action) => this.action = action;

        public void Dispose() => action();
    }
}
