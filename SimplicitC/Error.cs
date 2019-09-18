namespace SimplicitC
{
    public class Error
    {
        public readonly string msg;

        public Error(string msg)
        {
            this.msg = msg;
        }

        public override string ToString()
        {
            return msg;
        }
    }
}