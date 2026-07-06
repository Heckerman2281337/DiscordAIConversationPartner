using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordVoiceBotMark.src.Conversion
{
    public interface Decoder
    {
        public Task<byte[]> ConvertToPcmAsync(List<byte[]> data);
    }
    internal class Mp3Decoder : Decoder
    {
        public Task<byte[]> ConvertToPcmAsync(List<byte[]> data)
        {
            throw new NotImplementedException();
        }
    }
}
