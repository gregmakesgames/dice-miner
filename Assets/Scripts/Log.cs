namespace DiceMiner
{
    public static class Log
    {
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void Warning(string message)
        {
            G.debug.DebugWarningString(message);
            UnityEngine.Debug.LogWarning(message);
        }
        
        public static void Error(string message)
        {
            G.debug.DebugErrorString(message);
            UnityEngine.Debug.LogError(message);
        }
    }
}