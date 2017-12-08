namespace TickTrader.BotAgent.CmdUtil
{
    static class ArgsExtensions
    {
        public static string Read(this string[] args, string param)
        {
            foreach (var argument in args)
            {
                if (argument.Contains(param))
                {
                    return argument.Replace(param, "");
                }
            }
            return "";
        }
    }
}
