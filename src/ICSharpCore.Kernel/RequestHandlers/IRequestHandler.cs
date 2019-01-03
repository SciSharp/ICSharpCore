using ICSharpCore.Protocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.RequestHandlers
{
    public interface IRequestHandler<T>
    {
        void Process(Message<T> message);
    }
}
