using System;
using System.Collections.Generic;

namespace HousingSearchListener.Tests.V1.Gateway.Steps
{
    public class BaseSteps
    {
        protected readonly List<Action> _cleanup = new List<Action>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }
    }
}
