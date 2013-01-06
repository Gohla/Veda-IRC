using System.Collections.Generic;

namespace Veda.Configuration
{
    public class BotData
    {
        public IList<ConnectionData> Connections { get; set; }

        public bool ReplyNoCommand { get; set; }
        public bool ReplyAmbiguousCommands { get; set; }
        public bool ReplyIncorrectArguments { get; set; }
        public bool ReplyError { get; set; }
        public bool ReplyErrorDetailed { get; set; }
        public bool ReplyNoPermission { get; set; }
        public bool ReplyWithNickname { get; set; }
        public bool ReplySuccess { get; set; }

        public IList<char> AddressedCharacters { get; set; }
        public bool AddressedNickame { get; set; }

        public BotData()
        {
            Connections = new List<ConnectionData>();

            ReplyNoCommand = false;
            ReplyAmbiguousCommands = true;
            ReplyIncorrectArguments = true;
            ReplyError = true;
            ReplyErrorDetailed = true;
            ReplyNoPermission = true;
            ReplyWithNickname = true;
            ReplySuccess = true;

            AddressedCharacters = new List<char>();
            AddressedNickame = true;
        }
    }
}
