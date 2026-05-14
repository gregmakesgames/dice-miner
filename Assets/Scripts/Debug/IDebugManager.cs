namespace DiceMiner.Debug
{
    public interface IDebugManager
    {
        public void DebugWarningString(string message);
        public void DebugErrorString(string message);
    }
}