namespace ChatGPT.Net
{
    public interface IGptDebug
    {
        public void Debug(string message);
        public void Debug(string message, byte r, byte g, byte b);
    }
}
