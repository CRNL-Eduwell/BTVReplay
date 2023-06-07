using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTV.Data;

namespace SimpleExpressionEngine
{
    public class ChannelContext : IContext
    {
        public ChannelContext(List<BtvChannel> channels)
        {
            m_Channels = channels;
            Index = 0;
        }

        List<BtvChannel> m_Channels;
        public int Index { get; set; }

        public double ResolveVariable(string name)
        {
            BtvChannel channel = m_Channels.FirstOrDefault(c => c.Label == name);
            if (channel == null)
                throw new InvalidDataException($"Unknown channel: '{name}'");
            if (Index >= channel.Data.Length)
                throw new InvalidDataException("Index out of range");

            return channel.Data[Index];
        }

        public double CallFunction(string name, double[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
