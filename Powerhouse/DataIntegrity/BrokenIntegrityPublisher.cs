using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse.DataIntegrity
{
    class BrokenIntegrityPublisher
    {
        public delegate void BrokenIntegrityEventHandler(BrokenIntegrityEventArgs e);
        public event BrokenIntegrityEventHandler OnBrokenDataIntegrity;

        public BrokenIntegrityPublisher() { } 

        protected virtual void RaiseBrokenDataIntegrity(BrokenIntegrityEventArgs e)
        {
            BrokenIntegrityEventHandler handler = OnBrokenDataIntegrity;
            OnBrokenDataIntegrity?.Invoke(e);
        }
    }
}
